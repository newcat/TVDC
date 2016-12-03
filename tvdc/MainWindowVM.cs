using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace tvdc
{
    public class MainWindowVM : INotifyPropertyChanged
    {

        //Support for INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #region Binding Properties
        private bool _initialized = false;
        public bool Initialized
        {
            get { return _initialized; }
            set
            {
                _initialized = value;
                NotifyPropertyChanged();
            }
        }

        private bool _overrideViewerCount = true;
        public bool OverrideViewerCount
        {
            get { return _overrideViewerCount; }
            set
            {
                _overrideViewerCount = value;
                NotifyPropertyChanged("ViewerCount");
            }
        }

        private int _overriddenViewerCount = 0;
        public int OverriddenViewerCount
        {
            get { return _overriddenViewerCount; }
            set
            {
                _overriddenViewerCount = value;
                NotifyPropertyChanged("ViewerCount");
            }
        }

        public int ViewerCount
        {
            get { return OverrideViewerCount ? OverriddenViewerCount : viewerList.Count; }
        }

        private int _followerCount = 0;
        public int FollowerCount
        {
            get { return _followerCount; }
            set
            {
                _followerCount = value;
                NotifyPropertyChanged();
            }
        }

        private int _chatSize = 12;
        public int ChatSize
        {
            get { return _chatSize; }
            set
            {
                if (value >= 9)
                {
                    _chatSize = value;
                    NotifyPropertyChanged();
                }
            }
        }

        private List<PluginHelper.PluginInfo> _pluginInfos = new List<PluginHelper.PluginInfo>();
        public List<PluginHelper.PluginInfo> PluginInfos
        {
            get { return _pluginInfos; }
            set
            {
                _pluginInfos = value;
                NotifyPropertyChanged();
            }
        }

        private ObservableCollection<string> _channelFavorites = new ObservableCollection<string>();
        public ObservableCollection<string> ChannelFavorites
        {
            get { return _channelFavorites; }
            set
            {
                _channelFavorites = value;
                NotifyPropertyChanged();
            }
        }

        private string _selectedChannel;
        public string SelectedChannel
        {
            get { return _selectedChannel; }
            set
            {
                _selectedChannel = value;
                NotifyPropertyChanged();
            }
        }

        public RelayCommand<string> cmdSendChat { get; private set; }
        public RelayCommand CmdRemoveChannelEntry { get; private set; }
        public RelayCommand<string> CmdAddChannelEntry { get; private set; }
        public RelayCommand<string> CmdMod { get; private set; }
        public RelayCommand<string> CmdUnmod { get; private set; }
        public RelayCommand<string> CmdTimeout { get; private set; }
        public RelayCommand<string> CmdSwitchChannel { get; private set; }
        #endregion

        public event EventHandler<PluginClickedEventArgs> PluginClicked;
        public event EventHandler<SendMessageEventArgs> SendMessage;
        public event EventHandler DoInit;

        #region Singleton and Constructor
        private static MainWindowVM _instance;
        public static MainWindowVM Instance
        {
            get {
                if (_instance == null)
                    _instance = new MainWindowVM();
                return _instance;
            }
        }

        private MainWindowVM()
        {

            viewerList = new ObservableCollection<User>();
            chatEntryList = new ObservableCollection<ChatEntry>();

            viewerList.CollectionChanged += ViewerList_CollectionChanged;
            enableSorting = false;

            string[] arr = new string[Properties.Settings.Default.favoriteChannels.Count];
            Properties.Settings.Default.favoriteChannels.CopyTo(arr, 0);
            List<string> l = new List<string>(arr);
            ChannelFavorites = new ObservableCollection<string>(l);

            cmdSendChat = new RelayCommand<string>(sendMessage, (s) => { return Initialized; });
            CmdAddChannelEntry = new RelayCommand<string>(
                (s) => 
                {
                    if (s != "" && !ChannelFavorites.Contains(s))
                    {
                        ChannelFavorites.Add(s);
                        Properties.Settings.Default.favoriteChannels.Add(s);
                        Properties.Settings.Default.Save();
                    }
                },
                (s) => { return s != "" && !ChannelFavorites.Contains(s); });
            CmdRemoveChannelEntry = new RelayCommand(
                () =>
                {
                    if (SelectedChannel != "")
                    {
                        Properties.Settings.Default.favoriteChannels.Remove(SelectedChannel);
                        Properties.Settings.Default.Save();
                        ChannelFavorites.Remove(SelectedChannel);
                    }
                },
                () => SelectedChannel != null);
            CmdMod = new RelayCommand<string>((s) => sendMessage("/mod " + s));
            CmdUnmod = new RelayCommand<string>((s) => sendMessage("/unmod " + s));
            CmdTimeout = new RelayCommand<string>((s) => sendMessage("/timeout " + s));
            CmdSwitchChannel = new RelayCommand<string>(
                (s) => {
                    if (s != null && s != "" && s != Properties.Settings.Default.channel)
                    {
                        Properties.Settings.Default.channel = s.ToLower();
                        Properties.Settings.Default.Save();
                        InvokeInit();
                    }
                },
                (s) => (s != null && s != "" && s != Properties.Settings.Default.channel));

            PropertyChanged += MainWindowVM_PropertyChanged;
        }
        #endregion

        private void MainWindowVM_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "SelectedChannel" && SelectedChannel != null && SelectedChannel != "" &&
                SelectedChannel != Properties.Settings.Default.channel)
            {
                Properties.Settings.Default.channel = SelectedChannel;
                Properties.Settings.Default.Save();
                InvokeInit();
            }
        }

        public async Task Shutdown()
        {
            ChatlogUploader clu = new ChatlogUploader();
            await clu.UploadLog(chatEntryList, false);

            Application.Current.Shutdown();
        }

        #region Chat&Viewer List Stuff

        public ObservableCollection<User> viewerList { get; private set; }
        public ObservableCollection<ChatEntry> chatEntryList { get; private set; }

        public bool enableSorting { get; set; }

        public static object viewerListLock = new object();
        public static object chatListLock = new object();

        private void ViewerList_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("viewerCount"));

            if (e.NewItems != null)
            {
                foreach (User u in e.NewItems)
                {
                    if (!u.SubscribedToBadgeChangedEvent)
                    {
                        invoke(() =>
                        {
                            u.BadgeChanged += U_BadgeChanged;
                            u.SubscribedToBadgeChangedEvent = true;
                        });
                    }
                }
            }

        }

        private void U_BadgeChanged(object sender, EventArgs e)
        {
            lock (viewerListLock)
            {
                invoke(() =>
                {
                    if (enableSorting)
                        viewerList.Sort();
                });
            }
        }

        public void chatEntryList_Add(ChatEntry ce)
        {
            lock (chatListLock) { invoke(() => { chatEntryList.Add(ce); }); }
        }

        public void chatEntryList_Remove(ChatEntry ce)
        {
            lock (chatListLock) { invoke(() => { chatEntryList.Remove(ce); }); }
        }

        public void chatEntryList_Clear()
        {
            lock (chatListLock) { invoke(() => { chatEntryList.Clear(); }); }
        }

        public void chatEntryList_GetEntryReference(ChatEntry target, out ChatEntry reference)
        {
            lock (chatListLock)
            {
                reference = chatEntryList[chatEntryList.IndexOf(target)];
            }
        }

        public List<ChatEntry> chatEntryList_Get()
        {
            List<ChatEntry> cl = new List<ChatEntry>();

            lock (chatListLock)
            {
                foreach (ChatEntry ce in chatEntryList)
                {
                    cl.Add(ce);
                }
            }

            return cl;
        }

        public void viewerList_Add(User u)
        {
            lock (viewerListLock)
            {
                invoke(() =>
                {
                    if (enableSorting)
                    {
                        viewerList.AddSorted(u);
                    }
                    else
                    {
                        viewerList.Add(u);
                    }
                });
            }
        }

        public void viewerList_Remove(User u)
        {
            lock (viewerListLock) { invoke(() => { viewerList.Remove(u); }); }
        }

        public void viewerList_Clear()
        {
            lock (viewerListLock) { invoke(() => { viewerList.Clear(); }); }
        }

        public bool viewerList_ContainsName(string name)
        {
            lock (viewerListLock)
            {
                foreach (User u in viewerList)
                {
                    if (u.Name == name || u.Name == name.ToLower() || u.DisplayName == name)
                        return true;
                }
                return false;
            }
        }

        public bool viewerList_TryRemoveName(string name)
        {
            lock (viewerListLock)
            {
                foreach (User u in viewerList)
                {
                    if (u.Name == name || u.Name == name.ToLower() || u.DisplayName == name)
                    {
                        invoke(() => { viewerList.Remove(u); });
                        return true;
                    }
                }
                return false;
            }
        }

        public User viewerList_GetUserByName(string name)
        {
            lock (viewerListLock)
            {
                foreach (User u in viewerList)
                {
                    if (u.Name == name || u.Name == name.ToLower() || u.DisplayName == name)
                    {
                        return u;
                    }
                }
                return null;
            }
        }

        public void viewerList_GetUserInstanceByName(string name, out User user)
        {
            lock (viewerListLock)
            {
                foreach (User u in viewerList)
                {
                    if (u.Name == name || u.Name == name.ToLower() || u.DisplayName == name)
                    {
                        user = u;
                        return;
                    }
                }
                user = null;
            }
        }

        public void viewerList_Sort()
        {
            lock (viewerListLock) { invoke(() => { viewerList.Sort(); }); }
        }

        #endregion

        public void pluginClicked(string pluginName)
        {
            PluginClicked?.Invoke(this, new PluginClickedEventArgs(pluginName));
        }

        public void invoke(Action a)
        {
            Application.Current.Dispatcher.Invoke(a);
        }

        private void sendMessage(string msg)
        {
            SendMessage?.Invoke(this, new SendMessageEventArgs(msg));
        }

        public void InvokeInit()
        {
            DoInit?.Invoke(this, new EventArgs());
        }

    }
}
