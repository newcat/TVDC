using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.ComponentModel;
using System.Collections;

namespace tvdc
{
    /// <summary>
    /// Interaction logic for ChatRow.xaml
    /// </summary>
    public partial class ChatRow : UserControl
    {

        public static readonly DependencyProperty UsernameProperty = DependencyProperty.Register("Username", typeof(string), typeof(ChatRow));
        public string Username
        {
            get { return (string)GetValue(UsernameProperty); }
            set { SetValue(UsernameProperty, value); }
        }

        public static readonly DependencyProperty ColorProperty = DependencyProperty.Register("Color", typeof(string), typeof(ChatRow));
        public string Color
        {
            get { return (string)GetValue(ColorProperty); }
            set { SetValue(ColorProperty, value); }
        }

        public static readonly DependencyProperty IsModProperty = DependencyProperty.Register("IsMod", typeof(bool), typeof(ChatRow));
        public bool IsMod
        {
            get { return (bool)GetValue(IsModProperty); }
            set { SetValue(IsModProperty, value); }
        }

        public static readonly DependencyProperty DataProperty = DependencyProperty.Register("Data", typeof(IRCClient.PrivmsgReceivedEventArgs), typeof(ChatRow));
        public IRCClient.PrivmsgReceivedEventArgs Data
        {
            get { return (IRCClient.PrivmsgReceivedEventArgs)GetValue(DataProperty); }
            set { SetValue(DataProperty, value); }
        }

        private List<Image> images = new List<Image>();

        public ChatRow()
        {
            InitializeComponent();

            DependencyPropertyDescriptor isModDesc = DependencyPropertyDescriptor.FromProperty(IsModProperty, typeof(ChatRow));
            isModDesc.AddValueChanged(this, new EventHandler((object sender, EventArgs e) => updateMod()));

            DependencyPropertyDescriptor textDesc = DependencyPropertyDescriptor.FromProperty(DataProperty, typeof(ChatRow));
            textDesc.AddValueChanged(this, new EventHandler((object sender, EventArgs e) => updateText()));
        }

        private void updateText()
        {

            if (mainPanel.Children.Count > 2)
                mainPanel.Children.RemoveRange(2, mainPanel.Children.Count - 1);

            //Split the text into not emoticon parts
            //Therefore we first need to parse the emoticons
            ArrayList paragraphs = new ArrayList();
            List<EmoticonPosition> positions = new List<EmoticonPosition>();
            string emoticons = Data.tags["emotes"];

            if (emoticons.Length > 0)
            {
                int i = -1;
                while (i < emoticons.Length)
                {

                    i++;
                    string emoteIDString = "";
                    while (emoticons[i] != ':')
                    {
                        emoteIDString += emoticons[i];
                        i++;
                    }
                    int emoteID = int.Parse(emoteIDString);

                    bool exit = false;
                    while (!exit)
                    {

                        i++;
                        string startPosString = "";
                        while (emoticons[i] != '-')
                        {
                            startPosString += emoticons[i];
                            i++;
                        }
                        int startPos = int.Parse(startPosString);

                        i++;
                        string endPosString = "";
                        while (i < emoticons.Length && !(emoticons[i] == ',' || emoticons[i] == '/'))
                        {
                            endPosString += emoticons[i];
                            i++;
                        }
                        int endPos = int.Parse(endPosString);

                        positions.Add(new EmoticonPosition(emoteID, startPos, endPos));

                        if (i >= emoticons.Length || emoticons[i] == '/')
                            exit = true;

                    }

                }

                positions.Sort();

                i = 0;

                foreach (EmoticonPosition ep in positions)
                {
                    if (i != ep.startIndex)
                        paragraphs.Add(Data.message.Substring(i, ep.startIndex - i));
                    paragraphs.Add(ep.emoteID);
                    i = ep.endIndex;
                }

            } else
            {
                paragraphs.Add(Data.message);
            }

            foreach (object p in paragraphs)
            {
                if (p is int)
                {

                    Emoticon e = EmoticonManager.requestEmoticon((int)p);
                    Image i = new Image();

                    if (e.isLoaded)
                    {
                        setImageData(i, e);
                    } else
                    {
                        e.ImageDownloadFinished += E_ImageDownloadFinished;
                    }

                    i.Tag = (int)p;
                    i.VerticalAlignment = VerticalAlignment.Center;
                    images.Add(i);
                    mainPanel.Children.Add(i);

                } else
                {

                    List<Paragraph> localParagraphs = new List<Paragraph>();
                    string[] wordList = (p as string).Split(' ');
                    string s = "";

                    foreach (string word in wordList)
                    {
                        if (isUrl(word))
                        {
                            if (s != " ")
                            {
                                localParagraphs.Add(new Paragraph() { type = 0, text = s });
                                s = " ";
                            }
                            localParagraphs.Add(new Paragraph() { type = 0, text = word });
                        }
                        else
                        {
                            s += word + " ";
                        }
                    }

                    localParagraphs.Add(new Paragraph() { type = 0, text = s });

                    foreach (Paragraph pg in localParagraphs)
                    {
                        TextBlock t = new TextBlock();
                        t.Text = pg.text;
                        t.VerticalAlignment = VerticalAlignment.Center;


                        if (isUrl(pg.text))
                        {
                            t.Cursor = Cursors.Hand;
                            t.MouseDown += LinkClicked;
                            t.TextDecorations = TextDecorations.Underline;
                            t.Foreground = Brushes.LightBlue;
                        }

                        mainPanel.Children.Add(t);
                    }

                }
            }

        }

        private void E_ImageDownloadFinished(object sender, EventArgs e)
        {

            Emoticon em = sender as Emoticon;

            foreach (Image i in images)
            {
                if ((int)i.Tag == em.id)
                {
                    setImageData(i, em);
                }
            }

        }

        private void updateMod()
        {
            if (IsMod)
            {
                pathMod.Visibility = Visibility.Visible;
            } else
            {
                pathMod.Visibility = Visibility.Collapsed;
            }
        }

        private bool isUrl(string s)
        {
            Uri uriResult;
            if (s.StartsWith("www."))
                return Uri.TryCreate("http://" + s, UriKind.Absolute, out uriResult) && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
            return Uri.TryCreate(s, UriKind.Absolute, out uriResult) && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
        }

        private void LinkClicked(object sender, MouseButtonEventArgs e)
        {
            TextBlock tb = (TextBlock)sender;
            System.Diagnostics.Process.Start(tb.Text.Trim());
        }

        private void setImageData(Image i, Emoticon e)
        {
            i.Width = e.width;
            i.Height = e.height;
            BitmapImage b = new BitmapImage();
            b.BeginInit();
            b.UriSource = new Uri(e.image);
            b.EndInit();
            i.Source = b;
        }

        private class Paragraph
        {
            public int type { get; set; } //0 = Text, 1 = Emoticon
            public string text { get; set; }
        }

        private class EmoticonPosition : IComparable
        {
            public int emoteID { get; private set; }
            public int startIndex { get; private set; }
            public int endIndex { get; private set; }

            public EmoticonPosition(int emoteID, int startIndex, int endIndex)
            {
                this.emoteID = emoteID;
                this.startIndex = startIndex;
                this.endIndex = endIndex;
            }

            public int CompareTo(object obj)
            {
                if (obj == null) return 1;

                EmoticonPosition otherPos = obj as EmoticonPosition;

                if (otherPos != null)
                {
                    return startIndex.CompareTo(otherPos.startIndex);
                } else
                {
                    throw new ArgumentException("Object is not an emoticon position");
                }
                
            }
        }

    }
}
