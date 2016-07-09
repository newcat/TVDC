using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace tvdc
{

    //TODO: Fix that twitchnotify messages ("xxx just subscribed") are being treated as PRIVMSG which leads to crash
    //TODO: Fix that if there is text after an emoticon it sometimes doesn't get displayed
    //TODO: Replace poll window with poll plugin
    //TODO: Add support for cheering

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, IDisposable
    {

        private MainWindowVM vm = new MainWindowVM();
        private IRCClient irc;

        //private PollWindow pollWindow = new PollWindow();

        private Timer viewerGraphTimer = new Timer(1000);
        private Timer followerTimer = new Timer(30000);

        private WebClient followerWC;
        private readonly string clientID = (string)Application.Current.Resources["client_id"];

        private string nick;
        private string oauth;
        private string channel;
        private bool debug;
        private bool showJoinLeave;
        private bool initialized = false;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = vm;
        }

        private async void init()
        {

            initialized = false;

            if (Properties.Settings.Default.nick == "")
            {
                Settings s = new Settings();
                s.btnCancel.IsEnabled = false;
                s.Owner = this;
                s.ShowDialog();
            }

            if (Properties.Settings.Default.nick == "")
            {
                Close();
                Application.Current.Shutdown();
            }

            nick = Properties.Settings.Default.nick;
            oauth = Properties.Settings.Default.oauth;
            channel = Properties.Settings.Default.channel;
            debug = Properties.Settings.Default.debug;
            showJoinLeave = Properties.Settings.Default.showJoinLeave;

            eventList.ItemsSource = vm.chatEntryList;
            viewersLB.ItemsSource = vm.viewerList;
            fnfgraph.userlist = vm.viewerList;

            vm.chatEntryList.CollectionChanged += ChatEntryList_CollectionChanged;
            vm.enableSorting = false;

            //Init emoticons
            EmoticonManager.initialize();

            //Load badges and download sub badge
            vm.chatEntryList.Add(new ChatEntry(ChatEntry.Type.IRC, "Initializing..."));
            Badges.init();
            if (!await Badges.downloadSubBadge(channel))
                vm.chatEntryList.Add(new ChatEntry(ChatEntry.Type.ERROR, "Failed to download subscriber badge."));

            //Connect to IRC
            vm.chatEntryList.Add(new ChatEntry(ChatEntry.Type.IRC, "Connecting to IRC..."));
            irc = new IRCClient("irc.twitch.tv", 6667, nick, oauth, channel);

            //Add event handlers
            irc.MessageReceived += IRC_MessageReceived;
            irc.MessageSent += IRC_MessageSent;
            irc.Notice += IRC_Notice;
            irc.Join += IRC_Join;
            irc.Part += IRC_Part;
            irc.ModeChanged += IRC_ModeChanged;
            irc.PrivmsgReceived += IRC_PrivmsgReceived;
            irc.Userstate += IRC_Userstate;
            irc.Clearchat += IRC_Clearchat;
            irc.InitCompleted += Irc_InitCompleted;

            //Connect
            irc.connect();

            //Start timer for graph & follower count
            viewerGraphTimer.Elapsed += ViewerGraphTimer_Elapsed;
            viewerGraphTimer.Start();

            followerWC = new WebClient();
            followerTimer.Elapsed += FollowerTimer_Elapsed;
            followerTimer.Start();

            initialized = true;

        }

        private async void FollowerTimer_Elapsed(object sender, ElapsedEventArgs e)
        {

            if (followerWC.IsBusy)
                return;

            string result = "";

            try
            {
                //From August, 2016 it is needed to supply a client_id with a request
                result = await followerWC.DownloadStringTaskAsync("https://api.twitch.tv/kraken/channels/" + channel + "/follows?client_id=" + clientID);
            } catch (WebException ex)
            {
                vm.chatEntryList_Add(new ChatEntry(ChatEntry.Type.ERROR, "Error while trying to update follower count: " + ex.Message));
                return;
            }

            string followerCount = Regex.Match(Regex.Match(result, "\"_total\":[0-9]*").Value, "[0-9]+").Value;

            if (followerCount != "0,\"")
                vm.followerCount = int.Parse(followerCount);

        }

        private void IRC_Clearchat(object sender, IRCClient.JoinPartEventArgs e)
        {
            
            if (e.username != null && e.username != "")
            {
                //Clear all messages of one specific user
                //Copy list so we are thread-safe
                List<ChatEntry> chatList = vm.chatEntryList_Get();
                List<ChatEntry> removeList = new List<ChatEntry>();

                foreach (ChatEntry ce in chatList)
                {
                    if (ce.username.ToLower() == e.username)
                    {
                        removeList.Add(ce);
                    }
                }

                ChatEntry toRemove;

                foreach (ChatEntry ce in removeList)
                {
                    vm.chatEntryList_GetEntryReference(ce, out toRemove);
                    toRemove.tags = new Dictionary<string, string>() { { "message_deleted", "" } };
                }


            } else
            {
                vm.chatEntryList_Clear();
            }

        }

        private void IRC_Userstate(object sender, IRCClient.UserstateEventArgs e)
        {

            User u = null;
            vm.viewerList_GetUserInstanceByName(nick, out u);

            if (u == null)
                return;

            if (e.tags.ContainsKey("color") && e.tags["color"] != null && e.tags["color"] != "")
                u.color = e.tags["color"];

            if (e.tags.ContainsKey("display-name") && e.tags["display-name"] != null && e.tags["display-name"] != "")
                u.displayName = e.tags["display-name"];

            if (e.tags.ContainsKey("badges") && e.tags["badges"] != null)
                u.setBadges(Badges.parseBadgeString(e.tags["badges"]));

        }

        private void IRC_PrivmsgReceived(object sender, IRCClient.PrivmsgReceivedEventArgs e)
        {
            string color;
            if (e.tags.ContainsKey("color") && e.tags["color"] != null && e.tags["color"] != "")
            {
                color = e.tags["color"];
            } else
            {
                color = TwitchColors.getColorByUsername(e.username);
            }

            vm.chatEntryList_Add(new ChatEntry(ChatEntry.Type.CHAT, e.message, e.username, color, e.tags));

            User u = null;
            vm.viewerList_GetUserInstanceByName(e.username, out u);

            if (u == null || !e.tags.ContainsKey("badges") || e.tags["badges"] == null)
                return;

            u.setBadges(Badges.parseBadgeString(e.tags["badges"]));
        }

        private void IRC_ModeChanged(object sender, IRCClient.ModeChangedEventArgs e)
        {
            User u;
            vm.viewerList_GetUserInstanceByName(e.username, out u);

            if (u == null)
                return;

            if (e.isMod)
            {
                u.addBadge(Badges.BadgeTypes.MODERATOR);
            } else
            {
                u.TryRemoveBadge(Badges.BadgeTypes.MODERATOR);
            }

        }

        private void IRC_Part(object sender, IRCClient.JoinPartEventArgs e)
        {
            if (vm.viewerList_ContainsName(e.username))
                vm.viewerList_TryRemoveName(e.username);

            if (showJoinLeave)
                vm.chatEntryList_Add(new ChatEntry(ChatEntry.Type.PART, "left", e.username));
        }

        private void IRC_Join(object sender, IRCClient.JoinPartEventArgs e)
        {
            if (!vm.viewerList_ContainsName(e.username))
                vm.viewerList_Add(new User(e.username));

            if (showJoinLeave && irc.initialized)
                vm.chatEntryList_Add(new ChatEntry(ChatEntry.Type.JOIN, "joined", e.username));

        }

        private void IRC_Notice(object sender, IRCClient.MsgReceivedEventArgs e)
        {
            vm.chatEntryList_Add(new ChatEntry(ChatEntry.Type.IRC, e.message));
        }

        private void IRC_MessageSent(object sender, IRCClient.MsgReceivedEventArgs e)
        {
            if (debug)
                vm.chatEntryList_Add(new ChatEntry(ChatEntry.Type.IRC, "<" + e.message));
        }

        private void IRC_MessageReceived(object sender, IRCClient.MsgReceivedEventArgs e)
        {
            if (debug)
                vm.chatEntryList_Add(new ChatEntry(ChatEntry.Type.IRC, ">" + e.message));
        }

        private void Irc_InitCompleted(object sender, EventArgs e)
        {
            vm.enableSorting = true;
            vm.viewerList_Sort();
        }

        private async void ViewerGraphTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            await ViewerGraph.addStop(vm.viewerCount);
            Application.Current.Dispatcher.Invoke(fnfgraph.update);
        }

        private void ChatEntryList_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (vm.chatEntryList.Count > 0 && cbAutoscroll.IsChecked == true)
                eventList.ScrollIntoView(vm.chatEntryList.Last());
        }

        private void cm_Timeout(object sender, RoutedEventArgs e)
        {

        }

        private void cm_Mod(object sender, RoutedEventArgs e)
        {

        }

        private void cm_Unmod(object sender, RoutedEventArgs e)
        {

        }

        private void cm_Init(object sender, RoutedEventArgs e)
        {

        }

        private void Image1_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!initialized)
                return;

            Settings s = new Settings();
            s.Owner = this;
            if (s.ShowDialog() == true)
            {
                followerTimer.Stop();
                viewerGraphTimer.Stop();
                irc.disconnect();
                vm.chatEntryList_Clear();
                vm.viewerList_Clear();
                ViewerGraph.reset();
                init();
            }
                
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            init();
        }

        private void btnChatBigger_Click(object sender, RoutedEventArgs e)
        {
            vm.chatSize += 2;
        }

        private void btnChatSmaller_Click(object sender, RoutedEventArgs e)
        {
            vm.chatSize -= 2;
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            irc.disconnect();
            viewerGraphTimer.Stop();
            followerTimer.Stop();
            Application.Current.Shutdown();
        }

        private void tbChat_TextChanged(object sender, TextChangedEventArgs e)
        {

            if (tbChat.LineCount > 1 && initialized)
            {
                if (tbChat.Text.Replace(Environment.NewLine, "").Trim() == "")
                {
                    tbChat.Text = "";
                    return;
                }
                string text = tbChat.Text.Replace(Environment.NewLine, "").Trim();
                User u = vm.viewerList_GetUserByName(nick);

                irc.send(text);

                Dictionary<string, string> tags = new Dictionary<string, string>();
                tags.Add("emotes", "");
                tags.Add("badges", Badges.badgeListToString(u.badges));

                //if (u.isMod)
                //{
                //    tags.Add("mod", "1");
                //}

                vm.chatEntryList_Add( new ChatEntry(ChatEntry.Type.CHAT, text, nick, u.color, tags));

                tbChat.Text = "";
            }

        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    irc.Dispose();
                    followerTimer.Dispose();
                    viewerGraphTimer.Dispose();
                    followerWC.Dispose();
                    vm.Dispose();
                }

                disposedValue = true;
            }
        }
        public void Dispose()
        {
            Dispose(true);
        }
        #endregion

        //public void setLoadingPanelVisible(bool visible)
        //{
        //
        //    double targetOpacity = 0;
        //    if (visible) { targetOpacity = 1; }
        //
        //    DoubleAnimation da = new DoubleAnimation(targetOpacity, TimeSpan.FromSeconds(0.5));
        //    loadingPanel.BeginAnimation(OpacityProperty, da);
        //
        //}

    }
}
