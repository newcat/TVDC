using System.Windows;
using System.Windows.Controls;

namespace tvdc
{
    /// <summary>
    /// Interaction logic for Settings.xaml
    /// </summary>
    public partial class Settings : Window
    {

        private bool clearedCache = false;
        private int oldState = 0;

        public Settings()
        {
            InitializeComponent();

            tbChannel.Text = Properties.Settings.Default.channel;
            cbDebug.IsChecked = Properties.Settings.Default.debug;
            cbShowEvents.IsChecked = Properties.Settings.Default.showJoinLeave;

            if (AccountManager.Username != "")
            {
                btnLogout.IsEnabled = true;
                lblUsername.Text = "Currently logged in as: " + AccountManager.Username;
            }

            cbChatlogAlways.IsChecked = Properties.Settings.Default.uploadChatlog == 2;
            cbChatlogAsk.IsChecked = Properties.Settings.Default.uploadChatlog == 1;
            cbChatlogNever.IsChecked = Properties.Settings.Default.uploadChatlog == 0;
            oldState = Properties.Settings.Default.uploadChatlog;

        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            
            if (tbChannel.Text.Equals(""))
            {
                MessageBox.Show("Invalid input!");
                return;
            }

            DialogResult = true;

            string channel = tbChannel.Text.ToLower();

            if (channel.StartsWith("https://www.twitch.tv/"))
            {
                channel = channel.Substring(22);
            } else if (channel.StartsWith("http://www.twitch.tv/"))
            {
                channel = channel.Substring(21);
            }

            if (cbChatlogAlways.IsChecked == true)
            {
                Properties.Settings.Default.uploadChatlog = 2;
            }
            else if (cbChatlogAsk.IsChecked == true)
            {
                Properties.Settings.Default.uploadChatlog = 1;
            }
            else if (cbChatlogNever.IsChecked == true)
            {
                Properties.Settings.Default.uploadChatlog = 0;
            }

            Properties.Settings.Default.channel = channel;
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
            EmoticonManager.ClearCache();
            clearedCache = true;
            MessageBox.Show(this, "Cache cleared.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void btnCheckUpdate_Click(object sender, RoutedEventArgs e)
        {
            UpdateWindow uw = new UpdateWindow(false);
            uw.ShowDialogAndSearchForUpdates();
            Activate();
        }

        private void btnLogout_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.oauth = "";
            Properties.Settings.Default.Save();
            System.Diagnostics.Process.Start(Application.ResourceAssembly.Location);
            Application.Current.Shutdown();
        }

        private void cbChatlog_Checked(object sender, RoutedEventArgs e)
        {

            if (cbChatlogAlways.IsChecked == true && oldState != 2)
            {
                cbChatlogAsk.IsChecked = false;
                cbChatlogNever.IsChecked = false;
                oldState = 2;
            } else if (cbChatlogAsk.IsChecked == true && oldState != 1)
            {
                cbChatlogAlways.IsChecked = false;
                cbChatlogNever.IsChecked = false;
                oldState = 1;
            } else if (cbChatlogNever.IsChecked == true && oldState != 0)
            {
                cbChatlogAlways.IsChecked = false;
                cbChatlogAsk.IsChecked = false;
                oldState = 0;
            }

        }

        private void cbChatlog_Unchecked(object sender, RoutedEventArgs e)
        {

            if (cbChatlogAlways.IsChecked == false && cbChatlogAsk.IsChecked == false && cbChatlogNever.IsChecked == false)
            {
                ((CheckBox)sender).IsChecked = true;
            }                

        }
    }
}
