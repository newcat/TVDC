using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using tvdc.UserControls;

namespace tvdc
{

    //TODO: Fix that twitchnotify messages ("xxx just subscribed") are being treated as PRIVMSG which leads to crash
    //TODO: Fix that if there is text after an emoticon it sometimes doesn't get displayed
    //TODO: Fix that sometimes emoticons aren't replaced correctly (e. g. there still was a "t" after the getRekt emoticon)
    //TODO: Replace poll window with poll plugin

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
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

        private void init()
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

            //Download emoticons
            EmoticonManager.initialize();

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
            } catch (Exception)
            {
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
                    toRemove.text = "<message deleted>";
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

            if (e.tags.ContainsKey("user-type") && e.tags["user-type"] != null && e.tags["user-type"] != "")
                u.isMod = true;

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

            bool isMod;
            if ((e.tags.ContainsKey("user-type") && e.tags["user-type"] != "") ||
                (vm.viewerList_GetUserByName(e.username) != null && vm.viewerList_GetUserByName(e.username).isMod))
            {
                isMod = true;
            } else
            {
                isMod = false;
            }

            vm.chatEntryList_Add(new ChatEntry(ChatEntry.Type.CHAT, e.message, e.username, color, isMod, e));
        }

        private void IRC_ModeChanged(object sender, IRCClient.ModeChangedEventArgs e)
        {
            vm.viewerList_SetMod(e.username, e.isMod);
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
                vm.viewerList_Add(new User(e.username, false));

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

        private async void ViewerGraphTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            await ViewerGraph.addStop(vm.viewerCount);
            Application.Current.Dispatcher.Invoke(fnfgraph.update);
        }

        private void ChatEntryList_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (vm.chatEntryList.Count > 0)
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
                irc.disconnect();
                vm.chatEntryList_Clear();
                vm.viewerList_Clear();
                ViewerGraph.clearStops();
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
                irc.send(tbChat.Text.Replace(Environment.NewLine, "").Trim());
                vm.chatEntryList_Add(new ChatEntry(ChatEntry.Type.CHAT, tbChat.Text.Replace(Environment.NewLine, "").Trim(), nick,
                    vm.viewerList_GetUserByName(nick).color, vm.viewerList_GetUserByName(nick).isMod));
                tbChat.Text = "";
            }

        }

        //private void svg4188_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        //{
        //    if (!pollWindow.isOpen)
        //    {
        //        pollWindow.Show();
        //    } else
        //    {
        //        if (pollWindow.WindowState == WindowState.Normal)
        //        {
        //            pollWindow.WindowState = WindowState.Minimized;
        //        } else
        //        {
        //            pollWindow.WindowState = WindowState.Normal;
        //        }
        //    }
        //}

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
