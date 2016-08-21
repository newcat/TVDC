using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Collections.ObjectModel;
using System.Windows.Media.Animation;
using Microsoft.Expression.Shapes;
using System.Windows.Threading;

namespace tvdc
{
    /// <summary>
    /// Interaction logic for FNFGraph.xaml
    /// </summary>
    public partial class FNFGraph : UserControl, INotifyPropertyChanged
    {

        //Support for INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private string _lblInfoPanel_Content = "";
        public string lblInfoPanel_Content
        {
            get { return _lblInfoPanel_Content; }
            set
            {
                _lblInfoPanel_Content = value;
                NotifyPropertyChanged();
            }
        }

        private Brush _lblInfoPanel_Foreground = Brushes.White;
        public Brush lblInfoPanel_Foreground
        {
            get { return _lblInfoPanel_Foreground; }
            set
            {
                _lblInfoPanel_Foreground = value;
                NotifyPropertyChanged();
            }
        }

        private Thickness _infoPanel_Margin = new Thickness(0);
        public Thickness infoPanel_Margin
        {
            get { return _infoPanel_Margin; }
            set
            {
                _infoPanel_Margin = value;
                NotifyPropertyChanged();
            }
        }

        private bool isInfoPanelVisible = false;
        private double currentF;
        private double currentNF;
        private int total;
        private int follower;
        private int nonFollower;
        private int unknown;

        private DispatcherTimer t = new DispatcherTimer();

        public FNFGraph()
        {
            InitializeComponent();
            DataContext = this;
            t.Interval = TimeSpan.FromSeconds(1);
            t.Tick += update;
            t.Start();
        }

        public void update(object sender, EventArgs e)
        {

            MainWindowVM vm = MainWindowVM.Instance;

            if (vm.viewerList == null || vm.viewerList.Count == 0)
                return;

            total = vm.viewerList.Count;
            follower = 0;
            nonFollower = 0;

            User[] userlist = new User[total];
            vm.viewerList.CopyTo(userlist, 0);

            foreach (User u in userlist)
            {

                if (!u.updating)
                {
                    if (u.isFollower)
                    {
                        follower++;
                    } else
                    {
                        nonFollower++;
                    }
                }

            }

            if (total < follower + nonFollower)
                total = follower + nonFollower;

            unknown = total - follower - nonFollower;

            double f = (follower / (total * 1.0)) * 360;
            double nf = (nonFollower / (total * 1.0)) * 360 + f;

            if (f != currentF || nf != currentNF)
            {

                DoubleAnimation followerEA = new DoubleAnimation(f, TimeSpan.FromSeconds(0.5));
                followerEA.AccelerationRatio = 0.4;
                followerEA.DecelerationRatio = 0.4;

                DoubleAnimation nonfollowerEA = new DoubleAnimation(nf, TimeSpan.FromSeconds(0.5));
                nonfollowerEA.AccelerationRatio = 0.4;
                nonfollowerEA.DecelerationRatio = 0.4;

                arcFollower.BeginAnimation(Arc.EndAngleProperty, followerEA);
                arcNonfollower.BeginAnimation(Arc.StartAngleProperty, followerEA);
                arcNonfollower.BeginAnimation(Arc.EndAngleProperty, nonfollowerEA);
                arcUnknown.BeginAnimation(Arc.StartAngleProperty, nonfollowerEA);

                currentF = f;
                currentNF = nf;

            }
            
        }

        private void arcUnknown_MouseEnter(object sender, MouseEventArgs e)
        {
            lblInfoPanel_Content = unknown.ToString() + " unknown";
            lblInfoPanel_Foreground = Brushes.White;
            setInfoPanelVisibility(true);
        }

        private void arcNonfollower_MouseEnter(object sender, MouseEventArgs e)
        {
            lblInfoPanel_Content = nonFollower.ToString() + " non-followers";
            lblInfoPanel_Foreground = Brushes.Red;
            setInfoPanelVisibility(true);
        }

        private void arcFollower_MouseEnter(object sender, MouseEventArgs e)
        {
            lblInfoPanel_Content = follower.ToString() + " followers";
            lblInfoPanel_Foreground = new SolidColorBrush(Color.FromArgb(255, 0, 255, 0));
            setInfoPanelVisibility(true);
        }

        private void UserControl_MouseLeave(object sender, MouseEventArgs e)
        {
            setInfoPanelVisibility(false);
        }

        private void setInfoPanelVisibility(bool visible)
        {
            if (visible == isInfoPanelVisible)
                return;

            if (!visible)
            {
                DoubleAnimation blendOutAnim = new DoubleAnimation(0, TimeSpan.FromSeconds(0.5));
                infoPanel.BeginAnimation(OpacityProperty, blendOutAnim);
                isInfoPanelVisible = false;
            } else
            {
                DoubleAnimation blendInAnim = new DoubleAnimation(1, TimeSpan.FromSeconds(0.5));
                infoPanel.BeginAnimation(OpacityProperty, blendInAnim);
                isInfoPanelVisible = true;
            }
        }

    }
}
