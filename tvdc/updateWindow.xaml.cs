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

        private UpdateManager manager = new UpdateManager(new Uri("http://www.twitch-viewer-display.net/tvd/downloads/updates.json"),
            "<RSAKeyValue><Modulus>u5neiVZ9PlhqBtnymDRpJW3q2F/arQY2+ouRP0BNIuULr8UrBv48fx+mxc5kn9FASDtBuPghYMGHQJLGd85sHsCpgSHcFuEIQo/u3nMYlJATuc14OCGIFGvShHKAeLL6VHSCPtUZQHFHuazbr1XAE3QGKSeiNTQlFDHDZxJ0Lal+IWAwAyPhXmF+ZXTO9PHmqb5FOBRCaDkP0V4L9kV53Zq7RFIzZoX5APInlGC9KSCunVNZjPH0BJXNl7C32plAt7IRlA0wl8ZN/YWpZXSPxVYg1Yu7/0llXJJ2yc7oXTn8PtoL2BQ8DD59ec4qu5Y4GATlJcqGCyJejT+QRebRr2i+kingU20HJuV5TfeCSqDLWE3Izo0agDO/F7Yf1k3OIzZ4sjIAy6Qm9xTeWJnHujVNEV0xJXdii33sIN/d5swMnhk4+SZ9jDKAvOnruIxjGxW0a91cm32/nQwcBFyfQEZhj9TqEniwiMGaSPVMMeIYzJvKWNERIoh1cSZvw58EBoWaseyMwK8faiJI8OkPb3cHeaor5S6BL5W0+f0Tm6m7wtY62LV03Tt7W1ViLALKqK6kkgCqMt0K8ced0OOUPmwrQNeJz6XoLCmx0DvXFJUH63zi30SGe3YBHrzy7h6WpX9e/vGK+aOlsvkuHyFQKLKC3L5n4CMvBW8LXz6yk9Cl89eATlcZOcGsyQ++aATplW4LqDVt0vzYLgNGuzknWT4dw+/izML94mK+KluVPeavNThVVh9/XLm9NfxJ4O5U3MpPVZL1in8NDPUmb50wCbErOi0v3uBzBYPfw/cyCt0J1SRm5ERXLA6YVr4E0xOcxyc3L6ZE8LoF79tKq8DB7+6W8k/2/c6kDpy8pwXTwwb8IiTnnm/rsmpTYLbflRYb+FQ3KoQdL4E8Nz7N9DV/z9gW4YfTK5ufcN6v6KWNdTwDybK7D5YviZ/O3dg/p1ohcNs82vIdDWreNdeXOIlwAXNlrgcalHIlXBakgmm+NWsgB/I+KIe4rDHpXD2lWA462f9WZ5Gam4I7MFsyWBN1mxk1XfNX9ZrjPHXy7hDcJ7KATvNC710v/whezQHYSkS9/xnZ+vY1dHqyFbhUiRJBrt3OsIk8hthtJ+LmrieFlvaPSHjz6+C5l8b0Po1hFfgsROqG/QmMTBVc+4uawTxBB3vtt1yoIKZ17P3EyJKFpOGerzSY3/IExZzziRWL3Y99ScvbpG1D9hIOBzw4/vrlknjzFIFXxOHXCeeYCFlaFQDS7LbKmZpzSmIeBFYYWsnBFNvglgsgS5DGuJueifAw7DN+MUkNBrZA7OnGSFN3K/hV9rHVqlF5t2kT/1N/+cZwaW68OZ5cuyGDmoyb2gQZqw==</Modulus><Exponent>AQAB</Exponent></RSAKeyValue>",
            new CultureInfo("en"));
        private bool isSearching = false;
        private bool isDownloading = false;
        private bool readyToInstall = false;
        private bool silent = false;
        private bool searchWhenOpening = false;

        public UpdateWindow(bool silent)
        {
            InitializeComponent();
            this.silent = silent;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1060:MovePInvokesToNativeMethodsClass")]
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
            await SearchForUpdates();
            Dispatcher.PushFrame(frame);
        }

        public void ShowAndSearchForUpdates()
        {
            searchWhenOpening = true;
            Show();
        }

        public void ShowDialogAndSearchForUpdates()
        {
            searchWhenOpening = true;
            ShowDialog();
        }

        public async Task SearchForUpdates()
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

            manager.InstallPackage();
            Environment.Exit(0);
        }

        private string convertToMB(double bytes)
        {
            return Math.Round(bytes / 1048576, 2).ToString() + " MB";
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (searchWhenOpening)
                await SearchForUpdates();
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
