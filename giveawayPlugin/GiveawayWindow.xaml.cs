using System.Windows;

namespace giveawayPlugin
{
    /// <summary>
    /// Interaction logic for GiveawayWindow.xaml
    /// </summary>
    public partial class GiveawayWindow : Window
    {

        private bool isHidden = false;
        private bool forceClose = false;

        public GiveawayWindow()
        {
            InitializeComponent();
        }

        new public void Show()
        {
            if (isHidden)
            {
                Visibility = Visibility.Visible;
            } else
            {
                base.Show();
            }
        }

        public void ForceClose()
        {
            forceClose = true;
            Close();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!forceClose)
            {
                Visibility = Visibility.Collapsed;
                e.Cancel = true;
            }
        }

    }
}
