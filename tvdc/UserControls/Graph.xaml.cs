using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Globalization;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace tvdc
{
    /// <summary>
    /// Interaction logic for Graph.xaml
    /// </summary>
    public partial class Graph : UserControl, INotifyPropertyChanged
    {

        //Support for INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public enum DisplayMode
        {
            VIEWER, CHATFREQ
        }

        private DisplayMode _mode = DisplayMode.VIEWER;
        public DisplayMode Mode
        {
            get { return _mode; }
            set
            {
                _mode = value;
                NotifyPropertyChanged();
            }
        }

        //Model properties
        private const double highOpacity = 1.0;
        private const double lowOpacity = 0.35;
        private const int chatFreqSampleLength = 10; //In seconds

        private int maxValueViewers = 0;
        private int maxValueChatFreq = 0;

        private List<HistoryStop> history = new List<HistoryStop>();
        private CultureInfo iC = CultureInfo.InvariantCulture;
        private Queue<DateTime> chatTimes = new Queue<DateTime>();

        public Graph()
        {
            InitializeComponent();
            DataContext = this;
            PropertyChanged += Graph_PropertyChanged;
            Mode = DisplayMode.VIEWER;
        }

        private void Graph_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Mode")
            {
                if (Mode == DisplayMode.VIEWER)
                {
                    ViewerLinePath.Opacity = highOpacity;
                    ViewerFillPath.Opacity = highOpacity;
                    ChatFreqLinePath.Opacity = lowOpacity;
                    ChatFreqFillPath.Opacity = lowOpacity;
                } else
                {
                    ViewerLinePath.Opacity = lowOpacity;
                    ViewerFillPath.Opacity = lowOpacity;
                    ChatFreqLinePath.Opacity = highOpacity;
                    ChatFreqFillPath.Opacity = highOpacity;
                }
            }
        }

        public async Task addStop(int viewerCount)
        {

            int chatFreq = getChatFreq();

            history.Add(new HistoryStop(viewerCount, getChatFreq()));

            if (viewerCount > maxValueViewers)
                maxValueViewers = viewerCount + (int)Math.Round(viewerCount * 0.1);
            if (chatFreq > maxValueChatFreq)
                maxValueChatFreq = chatFreq + (int)Math.Round(chatFreq * 0.1);

            await Application.Current.Dispatcher.InvokeAsync(setPath);

        }

        public void reset()
        {
            history = new List<HistoryStop>();
            maxValueViewers = 10;
            maxValueChatFreq = 1;
        }

        private async Task setPath()
        {

            calcLabels();

            string viewerPathData = "M20," + getYbyIndex(0, DisplayMode.VIEWER).ToString(iC);
            await Task.Run(() =>
            {
                for (int x = 20; x <= ActualWidth; x++)
                {
                    viewerPathData += "L" + x.ToString() + "," + getYbyX(x, DisplayMode.VIEWER).ToString(iC);
                }
            });
            ViewerLinePath.Data = Geometry.Parse(viewerPathData);
            ViewerFillPath.Data = Geometry.Parse("M20,100L" + viewerPathData.Substring(1) + "V100Z");

            string chatFreqPathData = "M20," + getYbyIndex(0, DisplayMode.CHATFREQ).ToString(iC);
            await Task.Run(() =>
            {
                for (int x = 20; x <= ActualWidth; x++)
                {
                    chatFreqPathData += "L" + x.ToString() + "," + getYbyX(x, DisplayMode.CHATFREQ).ToString(iC);
                }
            });
            ChatFreqLinePath.Data = Geometry.Parse(chatFreqPathData);
            ChatFreqFillPath.Data = Geometry.Parse("M20,100L" + chatFreqPathData.Substring(1) + "V100Z");

            updateInfoPanel();

        }

        private void calcLabels()
        {

            int maxValue = Mode == DisplayMode.VIEWER ? maxValueViewers : maxValueChatFreq;   
            TextBlock[] tbs = new TextBlock[] { lbl0, lbl20, lbl40, lbl60, lbl80, lbl100 };

            double stepSize = maxValue / 5;
            string s = "";

            for (int i = 1; i <= 4; i++)
            {
                s = Math.Round(stepSize * (5 - i)).ToString();
                tbs[i].Text = s;
            }

            tbs[0].Text = maxValue.ToString();

        }

        private void UserControl_MouseMove(object sender, MouseEventArgs e)
        {
            updateInfoPanel();            
        }

        private int getYbyX(int x, DisplayMode m)
        {
            return getYbyIndex((int)Math.Round(((x - 20) / (ActualWidth - 20) * (history.Count - 1))), m);
        }

        private int getYbyIndex(int i, DisplayMode m)
        {

            if (m == DisplayMode.VIEWER)
            {

                if (history.Count == 0)
                    return 100;

                //Prevent out of bounds exception
                if (i >= history.Count)
                    i = history.Count - 1;

                //Prevent i from being negative, same reason as above
                i = i < 0 ? 0 : i;

                return (int)Math.Round((100 - (100 * (history[i].count / (double)maxValueViewers))));

            } else
            {

                if (history.Count == 0)
                    return 100;

                //Prevent out of bounds exception
                if (i >= history.Count)
                    i = history.Count - 1;

                //Prevent i from being negative, same reason as above
                i = i < 0 ? 0 : i;

                return (int)Math.Round((100 - (100 * (history[i].freq / (double)maxValueChatFreq))));

            }
            
        }

        private int getIndexByX(int x)
        {
            return (int)Math.Round(((x - 20) / (ActualWidth - 20)) * (history.Count - 1));
        }

        private HistoryStop getHistoryStopByX(int x)
        {
            if (history.Count == 0)
                return new HistoryStop(0, 0);

            return history[getIndexByX(x)];
        }

        private void UserControl_MouseLeave(object sender, MouseEventArgs e)
        {
            DoubleAnimation fadeOut = new DoubleAnimation(0, TimeSpan.FromSeconds(0.5));
            InfoPanel.BeginAnimation(OpacityProperty, fadeOut);
            InfoEllipse.BeginAnimation(OpacityProperty, fadeOut);
        }

        private void updateInfoPanel()
        {

            if (!IsMouseOver)
                return;

            int x = (int)Mouse.GetPosition(this).X;
            if (x > ActualWidth)
                return;

            int roundedX = (x / 2) * 2;

            if (x < 20)
            {
                if (InfoPanel.Opacity == 1)
                {
                    DoubleAnimation fadeOut = new DoubleAnimation(0, TimeSpan.FromSeconds(0.5));
                    InfoPanel.BeginAnimation(OpacityProperty, fadeOut);
                    InfoEllipse.BeginAnimation(OpacityProperty, fadeOut);
                }
            }
            else
            {
                if (InfoPanel.Opacity == 0)
                {
                    DoubleAnimation fadeIn = new DoubleAnimation(1, TimeSpan.FromSeconds(0.5));
                    InfoPanel.BeginAnimation(OpacityProperty, fadeIn);
                    InfoEllipse.BeginAnimation(OpacityProperty, fadeIn);
                }

                if (x <= ActualWidth - 50 && x >= 50)
                {
                    Canvas.SetLeft(InfoPanel, x - 50);
                }
                else if (x > ActualWidth - 50)
                {
                    Canvas.SetLeft(InfoPanel, (int)ActualWidth - 100);
                }
                else if (x < 50)
                {
                    Canvas.SetLeft(InfoPanel, 0);
                }

                int y = getYbyX(x, Mode);

                if (y >= 50)
                {
                    //Arrow downwards
                    Canvas.SetTop(InfoPanel, y - 60);
                    InfoPanel.Clip = Geometry.Parse("M0,40 L40,40 L50,50 L60,40 L100,40 L100,0 L0,0 L0,40");
                    lblViewerInfoPanel.Margin = new Thickness(0, 1, 0, 0);
                    lblTimeInfoPanel.Margin = new Thickness(0, 0, 0, 14);
                }
                else
                {
                    //Arrow upwards
                    Canvas.SetTop(InfoPanel, y + 10);
                    InfoPanel.Clip = Geometry.Parse("M0,10 L40,10 L50,0 L60,10 L100,10 L100,50 L0,50 L0,0");
                    lblViewerInfoPanel.Margin = new Thickness(0, 11, 0, 0);
                    lblTimeInfoPanel.Margin = new Thickness(0, 0, 0, 4);
                }

                HistoryStop hs = getHistoryStopByX(x);

                lblViewerInfoPanel.Text = Mode == DisplayMode.VIEWER ? hs.count.ToString() : hs.freq.ToString() + " msg/min";

                lblTimeInfoPanel.Text = hs.timestamp.ToString("HH:mm:ss");
                Canvas.SetLeft(InfoEllipse, roundedX - 2);
                Canvas.SetTop(InfoEllipse, y - 2);
            }

        }

        public void AddChatEvent()
        {
            chatTimes.Enqueue(DateTime.Now);
        }

        private int getChatFreq()
        {
            if (chatTimes.Count == 0)
                return 0;

            while ((DateTime.Now - chatTimes.Peek()).TotalSeconds > chatFreqSampleLength)
                chatTimes.Dequeue();

            return chatTimes.Count * (60 / chatFreqSampleLength);
        }

        private class HistoryStop
        {

            /// <summary>
            /// Viewer Count
            /// </summary>
            public int count { get; private set; }

            /// <summary>
            /// Chat Frequency (Messages per minute)
            /// </summary>
            public int freq { get; private set; }

            public DateTime timestamp { get; }

            public HistoryStop(int v, int c)
            {
                count = v;
                freq = c;
                timestamp = DateTime.Now;
            }
        }

        private void UserControl_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (Mode == DisplayMode.VIEWER)
            {
                Mode = DisplayMode.CHATFREQ;
            } else
            {
                Mode = DisplayMode.VIEWER;
            }
            calcLabels();
            updateInfoPanel();
        }
    }
}
