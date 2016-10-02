﻿using System.Threading.Tasks;
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

        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            
            if (tbChannel.Text.Equals(""))
            {
                MessageBox.Show("Invalid input!");
                return;
            }

            DialogResult = true;

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

        private void btnLogout_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.oauth = "";
            Properties.Settings.Default.Save();
            System.Diagnostics.Process.Start(Application.ResourceAssembly.Location);
            Application.Current.Shutdown();
        }
    }
}
