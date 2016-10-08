using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using tvdc.EventArguments;

namespace tvdc
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {

        private MainWindowVM mainVM;
        private PluginHelper pluginHelper;
        private FollowerUpdater followerUpdater;
        private IRCClient irc;

        private string channel;
        private bool debug;
        private bool showJoinLeave;

        private async void Application_Startup(object sender, StartupEventArgs e)
        {

            ShutdownMode = ShutdownMode.OnExplicitShutdown;

            tvdc.Properties.Settings.Default.Reload();
            if (tvdc.Properties.Settings.Default.upgradeRequired)
            {

                try
                {
                    tvdc.Properties.Settings.Default.GetPreviousVersion("nick");

                    tvdc.Properties.Settings.Default.Reset();
                    tvdc.Properties.Settings.Default.upgradeRequired = false;
                    tvdc.Properties.Settings.Default.Save();

                } catch (SettingsPropertyNotFoundException)
                {
                    //If setting could not be found it means we will also have
                    //a valid oauth code in previous settings
                    tvdc.Properties.Settings.Default.Upgrade();
                    tvdc.Properties.Settings.Default.upgradeRequired = false;
                    tvdc.Properties.Settings.Default.Save();
                }

            }

            if (! await AccountManager.Login())
            {
                MessageBox.Show("Error while logging in. Please try again.", "TVD");
                Shutdown();
            }

            if (tvdc.Properties.Settings.Default.channel == "")
            {
                Settings s = new Settings();
                s.btnCancel.IsEnabled = false;
                s.ShowDialog();
            }

            tvdc.Properties.Settings.Default.Reload();

            if (tvdc.Properties.Settings.Default.channel == "")
            {
                Shutdown();
            }

            mainVM = MainWindowVM.Instance;
            MainWindow m = new MainWindow(mainVM);
            MainWindow = m;
            m.DataContext = mainVM;
            m.Show();

            mainVM.PluginClicked += MainVM_PluginClicked;
            mainVM.SendMessage += MainVM_SendMessage;
            mainVM.DoInit += MainVM_DoInit;

            await init();
        }

        private async void MainVM_DoInit(object sender, EventArgs e)
        {
            await init();
        }

        private void MainVM_SendMessage(object sender, SendMessageEventArgs e)
        {
            if (irc != null)
                sendChatMessage(e.Message);
        }

        private void MainVM_PluginClicked(object sender, PluginClickedEventArgs e)
        {
            if (pluginHelper != null)
                pluginHelper.PluginClicked(e.PluginName);
        }

        public async Task init()
        {

            mainVM.Initialized = false;

            if (followerUpdater != null)
                followerUpdater.Stop();

            if (irc != null && irc.IsConnected)
                irc.disconnect();

            if (pluginHelper != null)
                pluginHelper.End();

            mainVM.chatEntryList_Clear();
            mainVM.viewerList_Clear();
            mainVM.FollowerCount = 0;

            channel = tvdc.Properties.Settings.Default.channel;
            debug = tvdc.Properties.Settings.Default.debug;
            showJoinLeave = tvdc.Properties.Settings.Default.showJoinLeave;

            mainVM.enableSorting = false;

            mainVM.chatEntryList_Add(new ChatEntry(ChatEntry.Type.IRC, "Initializing..."));

            //Init emoticons
            if (!await EmoticonManager.initialize())
            {
                mainVM.chatEntryList_Add(new ChatEntry(ChatEntry.Type.ERROR, "Failed to download emoticon list."));
            }

            //Load badges and download sub badge
            Badges.init();
            if (!await Badges.downloadSubBadge(channel))
                mainVM.chatEntryList_Add(new ChatEntry(ChatEntry.Type.ERROR, "Failed to download subscriber badge."));

            //Connect to IRC
            irc = new IRCClient("irc.twitch.tv", 6667, AccountManager.Username, AccountManager.Oauth, channel);

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

            //Load plugins
            mainVM.chatEntryList_Add(new ChatEntry(ChatEntry.Type.IRC, "Loading plugins..."));
            pluginHelper = new PluginHelper(irc);
            pluginHelper.LoadPlugins();
            mainVM.PluginInfos = pluginHelper.PluginInfoList;

            if (debug)
            {
                foreach (PluginHelper.PluginInfo pi in pluginHelper.PluginInfoList)
                {
                    mainVM.chatEntryList_Add(new ChatEntry(ChatEntry.Type.IRC, "Plugin loaded: " + pi.name));
                }
            }

            //Connect
            mainVM.chatEntryList_Add(new ChatEntry(ChatEntry.Type.IRC, "Connecting to IRC..."));
            irc.connect();

            if (followerUpdater == null)
                followerUpdater = new FollowerUpdater();
            followerUpdater.Start();

            mainVM.Initialized = true;
            ((MainWindow)MainWindow).initCompleted();

            UpdateWindow uw = new UpdateWindow(true);
            await uw.SearchForUpdates();

        }

        #region IRC

        private void IRC_Clearchat(object sender, JoinPartEventArgs e)
        {

            if (e.username != null && e.username != "")
            {
                //Clear all messages of one specific user
                //Copy list so we are thread-safe
                List<ChatEntry> chatList = mainVM.chatEntryList_Get();
                List<ChatEntry> removeList = new List<ChatEntry>();

                foreach (ChatEntry ce in chatList)
                {
                    if (ce.Username.ToLower() == e.username)
                    {
                        removeList.Add(ce);
                    }
                }

                ChatEntry toRemove;

                List<Paragraph> p = new List<Paragraph>();
                p.Add(new Paragraph("<message deleted>", false, false));

                mainVM.invoke(() =>
                {
                    foreach (ChatEntry ce in removeList)
                    {
                        mainVM.chatEntryList_GetEntryReference(ce, out toRemove);
                        toRemove.Paragraphs = p;
                    }
                });


            }
            else
            {
                mainVM.chatEntryList_Clear();
            }

        }

        private void IRC_Userstate(object sender, UserstateEventArgs e)
        {

            User u = null;
            mainVM.viewerList_GetUserInstanceByName(AccountManager.Username, out u);

            if (u == null)
                return;

            if (e.tags.ContainsKey("color") && e.tags["color"] != null && e.tags["color"] != "")
                mainVM.invoke(() => { u.Color = e.tags["color"]; });

            if (e.tags.ContainsKey("display-name") && e.tags["display-name"] != null && e.tags["display-name"] != "")
                mainVM.invoke(() => { u.DisplayName = e.tags["display-name"]; });

            if (e.tags.ContainsKey("badges") && e.tags["badges"] != null)
                mainVM.invoke(() => { u.setBadges(Badges.parseBadgeString(e.tags["badges"])); });

        }

        private void IRC_PrivmsgReceived(object sender, PrivmsgReceivedEventArgs e)
        {
            string color;
            if (e.tags.ContainsKey("color") && e.tags["color"] != null && e.tags["color"] != "")
            {
                color = e.tags["color"];
            }
            else
            {
                color = TwitchColors.getColorByUsername(e.username);
            }

            User u = null;
            mainVM.viewerList_GetUserInstanceByName(e.username, out u);

            List<Badges.BadgeTypes> badges = new List<Badges.BadgeTypes>();

            if (e.tags.ContainsKey("badges") && e.tags["badges"] != null)
            {
                badges = Badges.parseBadgeString(e.tags["badges"]);
                if (u != null)
                    mainVM.invoke(() => { u.setBadges(badges); });
            }

            e.tags.Add("text", e.message);
            mainVM.chatEntryList_Add(new ChatEntry(e.username, color, MessageParser.GetParagraphsFromTags(e.tags), badges));
        }

        private void IRC_ModeChanged(object sender, ModeChangedEventArgs e)
        {
            User u;
            mainVM.viewerList_GetUserInstanceByName(e.username, out u);

            if (u == null)
                return;

            if (e.isMod)
            {
                mainVM.invoke(() => { u.addBadge(Badges.BadgeTypes.MODERATOR); });
            }
            else
            {
                mainVM.invoke(() => { u.TryRemoveBadge(Badges.BadgeTypes.MODERATOR); });
            }

        }

        private void IRC_Part(object sender, JoinPartEventArgs e)
        {
            if (mainVM.viewerList_ContainsName(e.username))
                mainVM.viewerList_TryRemoveName(e.username);

            if (showJoinLeave)
                mainVM.chatEntryList_Add(new ChatEntry(ChatEntry.Type.PART, "left", e.username));
        }

        private void IRC_Join(object sender, JoinPartEventArgs e)
        {
            if (!mainVM.viewerList_ContainsName(e.username))
                mainVM.viewerList_Add(new User(e.username));

            if (showJoinLeave && irc.initialized)
                mainVM.chatEntryList_Add(new ChatEntry(ChatEntry.Type.JOIN, "joined", e.username));

        }

        private void IRC_Notice(object sender, MsgReceivedEventArgs e)
        {
            mainVM.chatEntryList_Add(new ChatEntry(ChatEntry.Type.IRC, e.message));
        }

        private void IRC_MessageSent(object sender, MsgReceivedEventArgs e)
        {
            if (debug)
                mainVM.chatEntryList_Add(new ChatEntry(ChatEntry.Type.IRC, "<" + e.message));
        }

        private void IRC_MessageReceived(object sender, MsgReceivedEventArgs e)
        {
            if (debug)
                mainVM.chatEntryList_Add(new ChatEntry(ChatEntry.Type.IRC, ">" + e.message));
        }

        private void Irc_InitCompleted(object sender, EventArgs e)
        {
            mainVM.enableSorting = true;
            mainVM.viewerList_Sort();
        }

        public void sendChatMessage(string message)
        {

            irc.send(message);

            string name = AccountManager.Username;
            string color = TwitchColors.getColorByUsername(name);
            List<Badges.BadgeTypes> badges = new List<Badges.BadgeTypes>();

            User u = mainVM.viewerList_GetUserByName(name);

            if (u != null)
            {
                name = u.DisplayName;
                color = u.Color;
                badges = u.Badges;
            }

            mainVM.chatEntryList_Add(new ChatEntry(name, color, MessageParser.GetParagraphsFromMessage(message), badges));

        }

        #endregion

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            pluginHelper.End();
            if (irc != null)
                irc.disconnect();
            if (followerUpdater != null)
                followerUpdater.Stop();
        }
    }
}
