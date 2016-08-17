using System;
using System.Linq;
using System.Windows;
using nUpdate.Updating;
using nUpdate.Exceptions;
using nUpdate.UpdateEventArgs;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Windows.Threading;
using System.Windows.Interop;

namespace tvdc
{
    /// <summary>
    /// Interaction logic for updateWindow.xaml
    /// </summary>
    public partial class UpdateWindow : Window, IDisposable
    {

        private UpdateManager manager = new UpdateManager(new Uri("http://newcat.bplaced.net/tvd/updates.json"),
            "<RSAKeyValue><Modulus>8l5adrCGpcd1yzQ6gTORp/zUDwl/jxtPW3g3dqFgEFpjS9ZFW05YA6/GdGMyU0nJQ6mdnd6j3fmqYsXSPsfG8iQmsCYvjEj/2eHUl5u3I7alxsqTViifuI1pm4hJ6A5+vVU4cCW7o8twzr1tgWElvIA/AOLLADu7PfeWmH+RwQaO5uGnIklmE8A4Qsz0IBr+0BvXwn9cgj7N6jw7hQNWGnb4bOU7yrVD64J1j9GxXuYS0GRjmddtYk67EaIb8/eHW1XzzMwPiOFVb+n/S4jnfQ11bYbq35K7ruklEnWVCg21JuB+i5D5ho746RXnlLFfNcN4wi5W0X0iadwJChlN64D12QtWZtkgUacte0vlwUZojvQXVKCSRqTXQjExZwA5eyptumjtYn1firqzsRvSjeqEtK/WXFL/ArGlwteJMGIh1OLfgXsSyd0CLvAtiE3TKWsIB1ZsT1ut5Bk304cukSWtKp//ixislc00z/w/NXPSQfaNEC2jE+7fKZ4lu92tLnVVmwGT7sVLUj/hrrhwIX3biu07d5wVNvc6uXLosF7jnPrmAX2XdwghiaN77uyLVfIeUJ2RGMHfF8lVnNCyOORl+R/L1Sxww09jk1nRLWGn8ZhiRHt8t5Mj7E6cZl9kV+TBxNPkTblpZbgdZOe4HThy3NHZU9zJh7+KtNiCsziqk+iySY/ew75uHgRUYWpAncloa7rYzFkvySTBPkpSM9rqnMEmB7c1EM9XX3x+OqC+y25Viop7e7xRGYPnYaolCbiJMNdKT82fIMQEU8iQ5cvboirLd9u8pWzItkIUlg/HPaZeX0r5Nhu4ZeeduHie3OoTeS6RZ0GWZoEmgNotpzc6iSDrH5QZ1zb+LuBW+t+oi1Tyeuy7fIEPV9dkB6/ibMdUVp/qMuzPzDeJdMrNiU+VHZiulrVr2v2RUoU3Eom8pZWWWgEqwsc5FZ6/LWtLWtYBQ3mSaPWS/ilTY6DAAI0ScMU+/ahYgMk9pmexUiOc8TJDtVn4Q5DzdAmO5atyiBeFgokq8BvNnwLJRRLODF7n+qoU9eMMOWVujE61a+NqbM7bkYutF9rNH+U4F8RGz/aDOPq+m2FYcomKLXE6Hgk47DkB2Q3An2ou4Xvw5PdvmBYVAp4brR9dJ1JhsZml5J8ukXYp+XqBdTlghW/FhAIfmbF32EAzoHpa/oHV2IOr2IEGpARKj2pJAIW4oMEhqYCPV+ZEA56h78YVVJKqVeghhZGrmbhLLfe+tx0BcN/YyEvbDhwjkr8nD6QXVH239ziPiszlnJ0kxGP+dnaobYJd9pADXF9qxAC4n5RIaUT587FHJeW//oEfaPXQ5OnYUcTUX+xJbeAwvCwsEcTRbQ==</Modulus><Exponent>AQAB</Exponent></RSAKeyValue>",
            new CultureInfo("en"));
        private bool isSearching = false;
        private bool isDownloading = false;
        private bool readyToInstall = false;
        private bool silent = false;

        public UpdateWindow(bool silent)
        {
            InitializeComponent();
            this.silent = silent;
        }

        [DllImport("user32")]
        internal static extern bool EnableWindow(IntPtr hwnd, bool bEnable);

        public async Task ShowModal(IntPtr ownerHwnd)
        {
            EnableWindow(ownerHwnd, false);

            DispatcherFrame frame = new DispatcherFrame();

            Closed += delegate
            {
                EnableWindow(ownerHwnd, true);
                frame.Continue = false;
            };
            
            Show();
            await searchForUpdates();
            Dispatcher.PushFrame(frame);
        }

        public async Task searchForUpdates()
        {

            bool updatesFound = false;
            isSearching = true;

            try
            {
                updatesFound = await manager.SearchForUpdatesAsync();
            }
            catch (Exception ex)
            {
                if (ex is OperationCanceledException && !silent)
                {
                    Close();
                    MessageBox.Show(this, "Failed to search for updates:\n" + ex.Message, "TVD Updater",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    manager.Dispose();
                    return;
                }
                else if (!(ex is SizeCalculationException))
                {
                    if (silent)
                    {
                        return;
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            finally
            {
                isSearching = false;
                pb.IsIndeterminate = false;
            }

            if (!updatesFound)
            {
                if (silent)
                {
                    manager.Dispose();
                    return;
                }
                else
                {
                    Close();
                    MessageBox.Show(this, "No updates found.", "TVD Updater", MessageBoxButton.OK, MessageBoxImage.Information);
                    manager.Dispose();
                    return;
                }
            }

            if (silent)
                Show();

            btnInstall.IsEnabled = true;
            lblStatus.Content = "Update(s) found.";

            string availableVersions = "";
            string currentVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
            string totalSize = convertToMB(manager.TotalSize);
            string changelog = "";

            UpdateConfiguration[] packages = manager.PackageConfigurations.ToArray();

            for (int i = 0; i < packages.Length; i++)
            {

                availableVersions += packages[i].LiteralVersion;
                changelog += packages[i].LiteralVersion + "\n" + packages[i].Changelog[new CultureInfo("en")];

                if (i < packages.Length - 1)
                {
                    availableVersions += ", ";
                    changelog = "\n\n";
                }

            }

            tbChangelog.Text = string.Format("Available Versions: {0}\nCurrent Version: {1}\nTotal Size: {2}\n\nChangelog:\n{3}",
                availableVersions, currentVersion, totalSize, changelog);

            readyToInstall = true;

        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            if (isSearching)
                manager.CancelSearch();

            if (isDownloading)
                manager.CancelDownload();

            Close();
        }

        private void btnInstall_Click(object sender, RoutedEventArgs e)
        {

            if (!readyToInstall)
                return;

            btnInstall.IsEnabled = false;

            manager.PackagesDownloadFinished += Manager_PackagesDownloadFinished;
            manager.PackagesDownloadFailed += Manager_PackagesDownloadFailed;
            manager.PackagesDownloadProgressChanged += Manager_PackagesDownloadProgressChanged;

            lblStatus.Content = "Downloading...";

            isDownloading = true;
            manager.DownloadPackagesAsync();

        }

        private void Manager_PackagesDownloadProgressChanged(object sender, UpdateDownloadProgressChangedEventArgs e)
        {
            Application.Current.Dispatcher.Invoke(delegate ()
            {
                lblStatus.Content = string.Format("Downloading... {0}/{1}",
                convertToMB(e.BytesReceived), convertToMB(e.TotalBytesToReceive));
                pb.Value = e.Percentage;
            });
        }

        private void Manager_PackagesDownloadFailed(object sender, FailedEventArgs e)
        {
            Close();
            MessageBox.Show(this, "Failed to download updates:\n" + e.Exception.Message, "TVD Updater",
                MessageBoxButton.OK, MessageBoxImage.Error);
            manager.Dispose();
            return;
        }

        private void Manager_PackagesDownloadFinished(object sender, EventArgs e)
        {
            if (!manager.ValidatePackages())
            {
                Close();
                MessageBox.Show(this, "Invalid package found! Please try again.", "TVD Updater",
                MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            string path = Path.Combine(Environment.CurrentDirectory, "tvd_settings.cfg");
            using (FileStream fs = new FileStream(path, FileMode.Create))
            {
                StreamWriter sw = new StreamWriter(fs, Encoding.ASCII);
                sw.WriteLine(Properties.Settings.Default.nick);
                sw.WriteLine(Properties.Settings.Default.oauth);
                sw.WriteLine(Properties.Settings.Default.channel);
                sw.WriteLine(Properties.Settings.Default.debug);
                sw.WriteLine(Properties.Settings.Default.showJoinLeave);
            }
            File.SetAttributes(path, File.GetAttributes(path) | FileAttributes.Hidden);

            manager.InstallPackage();
            Environment.Exit(0);
        }

        private string convertToMB(double bytes)
        {
            return Math.Round(bytes / 1048576, 2).ToString() + " MB";
        }

        #region IDisposable Support
        private bool disposedValue = false; // Dient zur Erkennung redundanter Aufrufe.

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                    manager.Dispose();

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
        #endregion


    }
}
