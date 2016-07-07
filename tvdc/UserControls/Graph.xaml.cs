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
        
        #region ViewModel properties
        private string _lbl0Text = "50";
        public string lbl0Text
        {
            get { return _lbl0Text; }
            set
            {
                _lbl0Text = value;
                NotifyPropertyChanged();
                lbl0LeftMargin = leftMargin(value);
            }
        }

        private string _lbl20Text = "40";
        public string lbl20Text
        {
            get { return _lbl20Text; }
            set
            {
                _lbl20Text = value;
                NotifyPropertyChanged();
                lbl20LeftMargin = leftMargin(value);
            }
        }

        private string _lbl40Text = "30";
        public string lbl40Text
        {
            get { return _lbl40Text; }
            set
            {
                _lbl40Text = value;
                NotifyPropertyChanged();
                lbl40LeftMargin = leftMargin(value);
            }
        }

        private string _lbl60Text = "20";
        public string lbl60Text
        {
            get { return _lbl60Text; }
            set
            {
                _lbl60Text = value;
                NotifyPropertyChanged();
                lbl60LeftMargin = leftMargin(value);
            }
        }

        private string _lbl80Text = "10";
        public string lbl80Text
        {
            get { return _lbl80Text; }
            set
            {
                _lbl80Text = value;
                NotifyPropertyChanged();
                lbl80LeftMargin = leftMargin(value);
            }
        }

        private int leftMargin(string s)
        {
            switch (s.Length)
            {
                case 1:
                    return 9;
                case 2:
                    return 5;
                case 3:
                    return 3;
                default:
                    return 0;
            }
        }

        private int _lbl0LeftMargin = 5;
        public int lbl0LeftMargin
        {
            get { return _lbl0LeftMargin; }
            set
            {
                _lbl0LeftMargin = value;
                NotifyPropertyChanged();
            }
        }

        private int _lbl80LeftMargin = 5;
        public int lbl80LeftMargin
        {
            get { return _lbl80LeftMargin; }
            set
            {
                _lbl80LeftMargin = value;
                NotifyPropertyChanged();
            }
        }

        private int _lbl60LeftMargin = 5;
        public int lbl60LeftMargin
        {
            get { return _lbl60LeftMargin; }
            set
            {
                _lbl60LeftMargin = value;
                NotifyPropertyChanged();
            }
        }

        private int _lbl40LeftMargin = 5;
        public int lbl40LeftMargin
        {
            get { return _lbl40LeftMargin; }
            set
            {
                _lbl40LeftMargin = value;
                NotifyPropertyChanged();
            }
        }

        private int _lbl20LeftMargin = 5;
        public int lbl20LeftMargin
        {
            get { return _lbl20LeftMargin; }
            set
            {
                _lbl20LeftMargin = value;
                NotifyPropertyChanged();
            }
        }

        private Geometry _linePathData = Geometry.Parse("");
        public Geometry linePathData
        {
            get { return _linePathData; }
            set
            {
                _linePathData = value;
                NotifyPropertyChanged();
            }
        }

        private Geometry _fillPathData = Geometry.Parse("");
        public Geometry fillPathData
        {
            get { return _fillPathData; }
            set
            {
                _fillPathData = value;
                NotifyPropertyChanged();
            }
        }

        private int _InfoPanelLeftMargin = 5;
        public int InfoPanelLeftMargin
        {
            get { return _InfoPanelLeftMargin; }
            set
            {
                _InfoPanelLeftMargin = value;
                NotifyPropertyChanged();
            }
        }

        private int _InfoPanelTopMargin = 5;
        public int InfoPanelTopMargin
        {
            get { return _InfoPanelTopMargin; }
            set
            {
                _InfoPanelTopMargin = value;
                NotifyPropertyChanged();
            }
        }

        private string _InfoPanelClip = "";
        public string InfoPanelClip
        {
            get { return _InfoPanelClip; }
            set
            {
                _InfoPanelClip = value;
                NotifyPropertyChanged();
            }
        }

        private int _InfoEllipseLeftMargin = 5;
        public int InfoEllipseLeftMargin
        {
            get { return _InfoEllipseLeftMargin; }
            set
            {
                _InfoEllipseLeftMargin = value;
                NotifyPropertyChanged();
            }
        }

        private int _InfoEllipseTopMargin = 5;
        public int InfoEllipseTopMargin
        {
            get { return _InfoEllipseTopMargin; }
            set
            {
                _InfoEllipseTopMargin = value;
                NotifyPropertyChanged();
            }
        }

        private Thickness _lblViewerMargin = new Thickness(0);
        public Thickness lblViewerMargin
        {
            get { return _lblViewerMargin; }
            set
            {
                _lblViewerMargin = value;
                NotifyPropertyChanged();
            }
        }

        private string _lblViewerText = "";
        public string lblViewerText
        {
            get { return _lblViewerText; }
            set
            {
                _lblViewerText = value;
                NotifyPropertyChanged();
            }
        }

        private Thickness _lblTimeMargin = new Thickness(0);
        public Thickness lblTimeMargin
        {
            get { return _lblTimeMargin; }
            set
            {
                _lblTimeMargin = value;
                NotifyPropertyChanged();
            }
        }

        private string _lblTimeText = "";
        public string lblTimeText
        {
            get { return _lblTimeText; }
            set
            {
                _lblTimeText = value;
                NotifyPropertyChanged();
            }
        }

        #endregion

        //Model properties
        private List<ViewerHistoryStop> viewerHistory = new List<ViewerHistoryStop>();
        private CultureInfo iC = CultureInfo.InvariantCulture;

        private int _maxY;
        private int maxY
        {
            get { return _maxY; }
            set
            {
                _maxY = value;
                calcLabels();
            }
        }

        public Graph()
        {
            InitializeComponent();
            DataContext = this;
        }

        private class ViewerHistoryStop
        {

            public int count { get; }
            public DateTime timestamp { get; }

            public ViewerHistoryStop(int v)
            {
                count = v;
                timestamp = DateTime.Now;
            }
        }

        public async Task addStop(int viewerCount)
        {
            viewerHistory.Add(new ViewerHistoryStop(viewerCount));
            if (viewerCount > maxY)
                maxY = viewerCount + (int)Math.Round(viewerCount * 0.1);
            await setPath();
        }

        public void reset()
        {
            viewerHistory = new List<ViewerHistoryStop>();
            maxY = 10;
        }

        private async Task setPath()
        {
            string pData = "M20," + getY(0).ToString(iC);

            await Task.Run(() =>
            {
                for (int x = 20; x <= ActualWidth; x++)
                {
                    pData += "L" + x.ToString() + "," + getY((int)Math.Round(((x - 20) / (MainCanvas.ActualWidth - 20)) * (viewerHistory.Count - 1))).ToString(iC);
                }
            });

            linePathData = Geometry.Parse(pData);
            fillPathData = Geometry.Parse("M20,100L" + pData.Substring(1) + "V100Z");
        }

        private int getY(int i)
        {
            if (i >= viewerHistory.Count)
                i = viewerHistory.Count - 1;

            i = i < 0 ? 0 : i;

            if (viewerHistory.Count == 0)
                return 100;

            return (int)Math.Round((100 - (100 * (viewerHistory[i].count / (double)maxY))));
        }

        private void calcLabels()
        {

            double stepSize = maxY / 5;
            string[] ret = new string[5];

            for (int i = 1; i <= 4; i++)
            {
                ret[i] = Math.Round(stepSize * (5 - i)).ToString();
            }

            lbl0Text = maxY.ToString();
            lbl20Text = ret[1];
            lbl40Text = ret[2];
            lbl60Text = ret[3];
            lbl80Text = ret[4];

        }

        private void UserControl_MouseMove(object sender, MouseEventArgs e)
        {

            int x = (int)e.GetPosition(this).X;
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
                    InfoPanelLeftMargin = x - 50;
                } else if (x > ActualWidth - 50)
                {
                    InfoPanelLeftMargin = (int)ActualWidth - 100;
                } else if (x < 50)
                {
                    InfoPanelLeftMargin = 0;
                }

                int y = getYbyX(x);

                if (y > 60)
                {
                    InfoPanelTopMargin = y - 60;
                    InfoPanelClip = "M0,40 L40,40 L50,50 L60,40 L100,40 L100,0 L0,0 L0,40";
                    lblViewerMargin = new Thickness(0, -2, 0, 0);
                    lblTimeMargin = new Thickness(0, 0, 0, 10);
                } else
                {
                    InfoPanelTopMargin = y + 10;
                    InfoPanelClip = "M0,10 L40,10 L50,0 L60,10 L100,10 L100,50 L0,50 L0,0";
                    lblViewerMargin = new Thickness(0, 5, 0, 0);
                    lblTimeMargin = new Thickness(0);
                }

                ViewerHistoryStop vhs = getViewerHistoryStopByX(x);

                lblViewerText = vhs.count.ToString();
                lblTimeText = vhs.timestamp.ToString("HH:mm:ss");
                InfoEllipseLeftMargin = roundedX - 2;
                InfoEllipseTopMargin = y - 2;
            }

        }

        private int getYbyX(int x)
        {
            return getY((int)Math.Round(((x - 20) / (ActualWidth - 20) * (viewerHistory.Count - 1))));
        }

        private ViewerHistoryStop getViewerHistoryStopByX(int x)
        {
            if (viewerHistory.Count == 0)
                return new ViewerHistoryStop(0);

            return viewerHistory[(int)Math.Round(((x - 20) / (ActualWidth - 20)) * (viewerHistory.Count - 1))];
        }

        private void UserControl_MouseLeave(object sender, MouseEventArgs e)
        {
            DoubleAnimation fadeOut = new DoubleAnimation(0, TimeSpan.FromSeconds(0.5));
            InfoPanel.BeginAnimation(OpacityProperty, fadeOut);
            InfoEllipse.BeginAnimation(OpacityProperty, fadeOut);
        }

    }
}
