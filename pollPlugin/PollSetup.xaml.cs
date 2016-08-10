using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using tvdc.Plugin;
using tvdc.EventArguments;

namespace pollPlugin
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class PollSetup : Window, IPlugin
    {

        private IPluginHost host;
        private ObservableCollection<string> pollOptions = new ObservableCollection<string>();
        private ResultsWindow rw;

        public PollSetup()
        {
            Assembly.LoadFrom("plugin.dll");
            InitializeComponent();
            listBox.ItemsSource = pollOptions;
        }

        public void Initialize(IPluginHost host)
        {
            this.host = host;
        }

        public string pluginName
        {
            get { return "Poll Plugin"; }
        }

        public ImageSource getMenuIcon()
        {
            return BmpToImg(Properties.Resources.btn);
        }

        public ImageSource getMenuIconHover() {
            return BmpToImg(Properties.Resources.btn_hover);
        }

        public void IconClicked()
        {
            Show();
            tbAddOption.Focus();
        }

        public void End()
        {
            Close();
        }

        private BitmapImage BmpToImg(System.Drawing.Bitmap bmp)
        {
            MemoryStream ms = new MemoryStream();
            bmp.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
            BitmapImage bi = new BitmapImage();
            bi.BeginInit();
            ms.Seek(0, SeekOrigin.Begin);
            bi.StreamSource = ms;
            bi.EndInit();
            return bi;
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            if (tbAddOption.Text == "")
                return;

            if (pollOptions.Contains(tbAddOption.Text.ToLower()))
            {
                MessageBox.Show("This poll option already exists.");
                return;
            }

            if (pollOptions.Count >= 6)
            {
                btnAdd.IsEnabled = false;
            }

            if (pollOptions.Count >= 1)
                btnStartPoll.IsEnabled = true;

            pollOptions.Add(tbAddOption.Text.ToLower());
            tbAddOption.Text = "";
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Hide();
            e.Cancel = true;
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (listBox.SelectedItem == null)
                return;

            pollOptions.Remove((string)listBox.SelectedItem);
        }

        private void btnStartPoll_Click(object sender, RoutedEventArgs e)
        {
            Hide();
            ResultsWindow rw = new ResultsWindow(pollOptions, host, (bool)cbMultiVote.IsChecked);
        }
    }
}
