using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using System.Net;
using System.Timers;
using System.Windows;
using System.IO;
using System.Threading.Tasks;
using tvdc.EventArguments;
using System.Text.RegularExpressions;
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

        public int ViewerCount
        {
            get { return viewerList.Count; }
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

        public RelayCommand<string> cmdSendChat { get; private set; }

        public event EventHandler<PluginClickedEventArgs> PluginClicked;
        public event EventHandler<SendMessageEventArgs> SendMessage;
        public event EventHandler DoInit;

        private static readonly MainWindowVM _instance = new MainWindowVM();
        public static MainWindowVM Instance
        {
            get { return _instance; }
        }

        private MainWindowVM()
        {

            viewerList = new ObservableCollection<User>();
            chatEntryList = new ObservableCollection<ChatEntry>();

            viewerList.CollectionChanged += ViewerList_CollectionChanged;
            enableSorting = false;

            cmdSendChat = new RelayCommand<string>(sendMessage, (s) => { return Initialized; });

        }

        public void shutdown()
        {
            
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
                    if (!u.subscribedToBadgeChangedEvent)
                    {
                        invoke(() =>
                        {
                            u.BadgeChanged += U_BadgeChanged;
                            u.subscribedToBadgeChangedEvent = true;
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
                    if (u.name == name || u.name == name.ToLower() || u.displayName == name)
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
                    if (u.name == name || u.name == name.ToLower() || u.displayName == name)
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
                    if (u.name == name || u.name == name.ToLower() || u.displayName == name)
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
                    if (u.name == name || u.name == name.ToLower() || u.displayName == name)
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
