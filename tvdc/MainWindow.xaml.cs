using System;
using System.Linq;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace tvdc
{

    //TODO: Add support for cheering

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        ScrollViewer eventListSV;
        Timer viewerGraphTimer = new Timer(1000);
        MainWindowVM vm;

        public MainWindow(MainWindowVM vm)
        {
            InitializeComponent();
            this.vm = vm;
            vm.chatEntryList.CollectionChanged += ChatEntryList_CollectionChanged;
            viewerGraphTimer.Elapsed += ViewerGraphTimer_Elapsed;

            BindingOperations.EnableCollectionSynchronization(vm.chatEntryList, MainWindowVM.chatListLock);
            BindingOperations.EnableCollectionSynchronization(vm.viewerList, MainWindowVM.viewerListLock);

        }

        public void initCompleted()
        {
            viewerGraphTimer.Start();
        }

        private void tbChat_TextChanged(object sender, TextChangedEventArgs e)
        {

            if (tbChat.LineCount > 1 && vm.cmdSendChat.CanExecute(""))
            {
                if (tbChat.Text.Replace(Environment.NewLine, "").Trim() == "")
                {
                    tbChat.Text = "";
                    return;
                }

                string text = tbChat.Text.Replace(Environment.NewLine, "").Trim();
                vm.cmdSendChat.Execute(text);
                tbChat.Text = "";
            }

        }

        private void Image_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is Image)
                vm.pluginClicked(((Image)sender).Tag.ToString());
        }

        private async void ViewerGraphTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            await ViewerGraph.addStop(vm.ViewerCount);
        }

        private void ChatEntryList_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (eventListSV == null && VisualTreeHelper.GetChildrenCount(eventList) > 0)
            {
                Border border = (Border)VisualTreeHelper.GetChild(eventList, 0);
                eventListSV = (ScrollViewer)VisualTreeHelper.GetChild(border, 0);
            }

            if (vm.chatEntryList.Count > 0 && cbAutoscroll.IsChecked == true && eventListSV != null)
                eventListSV.ScrollToBottom();
        }

        private void Image1_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!vm.Initialized)
                return;

            Settings s = new Settings();
            s.Owner = this;
            if (s.ShowDialog() == true)
            {
                ViewerGraph.reset();
                vm.InvokeInit();
            }

        }

        private void Window_Closed(object sender, EventArgs e)
        {
            viewerGraphTimer.Stop();
            vm.shutdown();
        }

        private void btnChatBigger_Click(object sender, RoutedEventArgs e)
        {
            vm.ChatSize += 2;
        }

        private void btnChatSmaller_Click(object sender, RoutedEventArgs e)
        {
            vm.ChatSize -= 2;
        }

    }
}
