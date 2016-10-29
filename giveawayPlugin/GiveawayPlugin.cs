using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using tvdc.EventArguments;
using tvdc.Plugin;

namespace giveawayPlugin
{
    class GiveawayPlugin : IPlugin, INotifyPropertyChanged
    {

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName]string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public string pluginName { get { return "Giveaway"; } }

        #region ViewModel Properties
        private string _startNotificationMessage = Properties.Settings.Default.StartNotificationMessage;
        public string StartNotificationMessage
        {
            get { return _startNotificationMessage; }
            set
            {
                _startNotificationMessage = value;
                NotifyPropertyChanged();
            }
        }

        private string _enterCommandString = Properties.Settings.Default.EnterCommandString;
        public string EnterCommandString
        {
            get { return _enterCommandString; }
            set
            {
                _enterCommandString = value;
                NotifyPropertyChanged();
            }
        }

        private bool _notifyWhenEntering = Properties.Settings.Default.NotifyWhenEntering;
        public bool NotifyWhenEntering
        {
            get { return _notifyWhenEntering; }
            set
            {
                _notifyWhenEntering = value;
                NotifyPropertyChanged();
                NotifyPropertyChanged("EnableEnterNotificationTextBox");
            }
        }

        public bool EnableEnterNotificationTextBox
        {
            get { return EnableStartSection && NotifyWhenEntering; }
        }

        private string _enterNotificationMessage = Properties.Settings.Default.EnterNotificationMessage;
        public string EnterNotificationMessage
        {
            get { return _enterNotificationMessage; }
            set
            {
                _enterNotificationMessage = value;
                NotifyPropertyChanged();
            }
        }

        public bool EnableStartSection
        {
            get { return !giveawayRunning; }
        }

        private ObservableCollection<string> _participants = new ObservableCollection<string>();
        public ObservableCollection<string> Participants
        {
            get { return _participants; }
            private set
            {
                _participants = value;
                NotifyPropertyChanged();
            }
        }

        private string _winnerName = "";
        public string WinnerName
        {
            get { return _winnerName; }
            private set
            {
                _winnerName = value;
                NotifyPropertyChanged();
            }
        }

        private string _notificationMessage = Properties.Settings.Default.NotificationMessage;
        public string NotificationMessage
        {
            get { return _notificationMessage; }
            set
            {
                _notificationMessage = value;
                NotifyPropertyChanged();
            }
        }

        public RelayCommand CmdStartGiveaway { get; private set; }
        public RelayCommand CmdClearParticipants { get; private set; }
        public RelayCommand CmdFindWinner { get; private set; }
        public RelayCommand<string> CmdNotify { get; private set; }
        #endregion

        private IPluginHost host;

        private bool _giveawayRunning = false;
        private bool giveawayRunning
        {
            get { return _giveawayRunning; }
            set
            {
                _giveawayRunning = value;
                NotifyPropertyChanged("EnableStartSection");
                NotifyPropertyChanged("EnableEnterNotificationTextBox");
            }
        }

        private GiveawayWindow window = new GiveawayWindow();
        private Random rand = new Random();

        public GiveawayPlugin()
        {

            CmdStartGiveaway = new RelayCommand(
                startGiveaway,
                () => giveawayRunning == false && EnterCommandString != "");

            CmdClearParticipants = new RelayCommand(
                () => Participants.Clear(),
                () => Participants.Count > 0);

            CmdFindWinner = new RelayCommand(
                chooseWinner,
                () => giveawayRunning == true && Participants.Count > 0);

            CmdNotify = new RelayCommand<string>(
                notifyWinner,
                (s) => giveawayRunning == false && WinnerName != "");

            window.DataContext = this;

        }

        public void Initialize(IPluginHost host)
        {
            this.host = host;
            host.IRC_PrivmsgReceived += Host_IRC_PrivmsgReceived;
        }

        private void Host_IRC_PrivmsgReceived(object sender, PrivmsgReceivedEventArgs e)
        {
            if (giveawayRunning && e.message == EnterCommandString && !Participants.Contains(e.username))
            {
                Application.Current.Dispatcher.Invoke(() => Participants.Add(e.username));

                if (NotifyWhenEntering)
                    host.sendMesssage(getFormatted(EnterNotificationMessage, e.username));
            }
        }

        public ImageSource getMenuIcon()
        {
            return BmpToImg(Properties.Resources.gift_icon);
        }

        public ImageSource getMenuIconHover()
        {
            return BmpToImg(Properties.Resources.gift_icon_hover);
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

        public void IconClicked()
        {
            window.Show();
        }

        public void End()
        {

            Properties.Settings.Default.EnterCommandString = EnterCommandString;
            Properties.Settings.Default.EnterNotificationMessage = EnterNotificationMessage;
            Properties.Settings.Default.NotificationMessage = NotificationMessage;
            Properties.Settings.Default.NotifyWhenEntering = NotifyWhenEntering;
            Properties.Settings.Default.StartNotificationMessage = StartNotificationMessage;
            Properties.Settings.Default.Save();

            window.ForceClose();
        }

        private void startGiveaway()
        {

            giveawayRunning = true;
            WinnerName = "";

            if (StartNotificationMessage != "")
                host.sendMesssage(getFormatted(StartNotificationMessage, EnterCommandString));

        }

        private void chooseWinner()
        {
            WinnerName = Participants[rand.Next(Participants.Count)];
            giveawayRunning = false;
        }

        private void notifyWinner(string s)
        {

            if (s == "Chat")
            {
                host.sendMesssage(getFormatted(NotificationMessage, WinnerName));
            } else if (s == "Whisper")
            {
                host.sendMesssage(string.Format("/w {0} {1}", WinnerName, getFormatted(NotificationMessage, WinnerName)));
            }

        }

        private string getFormatted(string s, string param)
        {
            if (s.Contains("{0}"))
                return string.Format(s, param);
            else
                return s;
        }

    }
}
