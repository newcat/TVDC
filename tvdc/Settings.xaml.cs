using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace tvdc
{
    /// <summary>
    /// Interaction logic for Settings.xaml
    /// </summary>
    public partial class Settings : Window
    {

        private bool clearedCache = false;
        private string oauth;

        public Settings()
        {
            InitializeComponent();

            tbNick.Text = Properties.Settings.Default.nick;
            oauth = Properties.Settings.Default.oauth;
            tbOauth.Text = "(Hidden)";
            tbChannel.Text = Properties.Settings.Default.channel;
            cbDebug.IsChecked = Properties.Settings.Default.debug;
            cbShowEvents.IsChecked = Properties.Settings.Default.showJoinLeave;
        }

        private void linkGenerate_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            //Process.Start("http://www.twitchapps.com/tmi");
            AuthenticationWindow aw = new AuthenticationWindow();
            aw.ShowDialog();
            if (aw.oauth != "")
            {
                oauth = aw.oauth;
            }
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            if (tbNick.Text.Equals("") || oauth.Equals("") || tbChannel.Text.Equals("") || oauth.Substring(0, 6) != "oauth:")
            {
                MessageBox.Show("Invalid input!");
                return;
            }

            DialogResult = true;

            Properties.Settings.Default.nick = tbNick.Text.ToLower();
            Properties.Settings.Default.oauth = oauth;
            Properties.Settings.Default.channel = tbChannel.Text.ToLower();
            Properties.Settings.Default.debug = (bool)cbDebug.IsChecked;
            Properties.Settings.Default.showJoinLeave = (bool)cbShowEvents.IsChecked;
            Properties.Settings.Default.Save();

            Close();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false || clearedCache;
            Close();
        }

        private void btnClearCache_Click(object sender, RoutedEventArgs e)
        {
            EmoticonManager.clearCache();
            clearedCache = true;
            MessageBox.Show(this, "Cache cleared.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private async void btnCheckUpdate_Click(object sender, RoutedEventArgs e)
        {
            UpdateWindow uw = new UpdateWindow(false);
            await uw.ShowModal(new System.Windows.Interop.WindowInteropHelper(this).Handle);
            Activate();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            tbOauth.Text = oauth;
        }
    }
}
