using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
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

        public static readonly DependencyProperty TagsProperty = DependencyProperty.Register("Tags", typeof(Dictionary<string, string>), typeof(ChatRow));
        public Dictionary<string, string> Tags
        {
            get { return (Dictionary<string, string>)GetValue(TagsProperty); }
            set { SetValue(TagsProperty, value); }
        }

        private List<Image> images = new List<Image>();

        public ChatRow()
        {
            InitializeComponent();

            DependencyPropertyDescriptor tagsDesc = DependencyPropertyDescriptor.FromProperty(TagsProperty, typeof(ChatRow));
            tagsDesc.AddValueChanged(this, new EventHandler((object sender, EventArgs e) => updateData()));
        }

        private void updateData()
        {

            if (Tags.ContainsKey("message_deleted"))
            {
                clearText();
                return;
            }

            if ((Tags.ContainsKey("badges") && Tags["badges"] != null && Tags["badges"].Contains("moderator")) ||
                (Tags.ContainsKey("mod") && Tags["mod"] != null && Tags["mod"] == "1"))
            {
                addBadge(Badges.moderator);
            }

            if (((Tags.ContainsKey("badges") && Tags["badges"] != null && Tags["badges"].Contains("subscriber")) ||
                (Tags.ContainsKey("subscriber") && Tags["subscriber"] != null && Tags["subscriber"] == "1")) &&
                Badges.hasSubscriberBadge)
            {
                addBadge(Badges.subscriber);
            }

            if ((Tags.ContainsKey("badges") && Tags["badges"] != null && Tags["badges"].Contains("turbo")) ||
                (Tags.ContainsKey("turbo") && Tags["turbo"] != null && Tags["turbo"] == "1"))
            {
                addBadge(Badges.turbo);
            }

            if (Tags.ContainsKey("badges") && Tags["badges"] != null && Tags["badges"].Contains("staff"))
            {
                addBadge(Badges.staff);
            }

            if (Tags.ContainsKey("badges") && Tags["badges"] != null && Tags["badges"].Contains("admin"))
            {
                addBadge(Badges.admin);
            }

            if (Tags.ContainsKey("badges") && Tags["badges"] != null && Tags["badges"].Contains("broadcaster"))
            {
                addBadge(Badges.broadcaster);
            }

            if (Tags.ContainsKey("badges") && Tags["badges"] != null && Tags["badges"].Contains("global_mod"))
            {
                addBadge(Badges.global_mod);
            }

            //Split the text into not emoticon parts
            //Therefore we first need to parse the emoticons
            ArrayList paragraphs = new ArrayList();
            List<EmoticonPosition> positions = new List<EmoticonPosition>();
            string emoticons = Tags["emotes"];

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
                        paragraphs.Add(Tags["text"].Substring(i, ep.startIndex - i));
                    paragraphs.Add(ep.emoteID);
                    i = ep.endIndex + 1;
                }

            } else
            {
                paragraphs.Add(Tags["text"]);
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

        private void addBadge(BitmapImage b)
        {
            Image i = new Image();
            i.Source = b;
            i.Width = 18;  //not using b.Width / b.Heigh because the alpha images
            i.Height = 18; //might have different dimensions
            i.VerticalAlignment = VerticalAlignment.Center;
            i.SnapsToDevicePixels = true;
            i.Margin = new Thickness(5, 0, 0, 0);
            i.Tag = "Badge";
            mainPanel.Children.Insert(0, i);
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

        private void clearText()
        {

            List<UIElement> removeList = new List<UIElement>();

            foreach (UIElement uie in mainPanel.Children)
            {
                if (uie is TextBlock && (string)((TextBlock)uie).Tag != "username")
                    removeList.Add(uie);
                if (uie is Image && (string)((Image)uie).Tag != "Badge")
                    removeList.Add(uie);
            }

            foreach (UIElement r in removeList)
            {
                mainPanel.Children.Remove(r);
            }

            TextBlock tb = new TextBlock();
            tb.Text = "<message deleted>";
            tb.VerticalAlignment = VerticalAlignment.Center;
            mainPanel.Children.Add(tb);

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
