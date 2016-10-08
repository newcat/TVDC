using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using tvdc.Plugin;
using tvdc.EventArguments;
using System.Windows.Media;
using System.Windows;

namespace tvdc
{
    public class PluginHelper : IPluginHost
    {

        private IRCClient irc;
        private List<IPlugin> plugins = new List<IPlugin>();

        public event EventHandler<EventArgs> IRC_Connected;
        public event EventHandler<EventArgs> IRC_ConnectionError;
        public event EventHandler<EventArgs> IRC_InitCompleted;
        public event EventHandler<JoinPartEventArgs> IRC_Join;
        public event EventHandler<MsgReceivedEventArgs> IRC_MessageReceived;
        public event EventHandler<ModeChangedEventArgs> IRC_ModeChanged;
        public event EventHandler<JoinPartEventArgs> IRC_Part;
        public event EventHandler<UserstateEventArgs> IRC_Userstate;
        public event EventHandler<PrivmsgReceivedEventArgs> IRC_PrivmsgReceived;

        public List<PluginInfo> PluginInfoList
        {
            get { return getPluginInfo(); }
        }

        public PluginHelper(IRCClient irc)
        {

            this.irc = irc;

            irc.Connected += Irc_Connected;
            irc.ConnectionError += Irc_ConnectionError;
            irc.InitCompleted += Irc_InitCompleted;
            irc.Join += Irc_Join;
            irc.Part += Irc_Part;
            irc.MessageReceived += Irc_MessageReceived;
            irc.ModeChanged += Irc_ModeChanged;
            irc.Userstate += Irc_Userstate;
            irc.PrivmsgReceived += Irc_PrivmsgReceived;

        }

        public void LoadPlugins()
        {

            string pluginTypeName = typeof(IPlugin).FullName;
            string[] dllFiles = Directory.GetFiles(Environment.CurrentDirectory, "*.dll");

            foreach (string dll in dllFiles)
            {
                Assembly ass = Assembly.LoadFile(dll);
                
                if (ass != null)
                {
                    Type[] assTypes;
                    try { assTypes = ass.GetTypes(); } catch (Exception) { continue; }
                    foreach (Type t in assTypes)
                    {
                        if (!(t.IsInterface || t.IsAbstract) &&
                            t.GetInterface(pluginTypeName) != null)
                        {
                            try {
                                IPlugin plugin = (IPlugin)Activator.CreateInstance(t);
                                if (!pluginExists(plugin.pluginName))
                                {
                                    plugins.Add(plugin);
                                } else
                                {
                                    MessageBox.Show("Plugin already loaded: " + plugin.pluginName);
                                }
                            } catch (Exception e)
                            {
                                MessageBox.Show("Failed to load plugin \"" + dll + "\"\n" + e.Message);
                            }
                        }
                    }
                }
            }

            foreach (IPlugin p in plugins)
            {
                p.Initialize(this);
            }

        }

        public void PluginClicked(string pluginName)
        {
            IPlugin p = getPluginByName(pluginName);
            if (p != null)
            {
                p.IconClicked();
            }
        }

        public void End()
        {

            foreach (IPlugin p in plugins)
            {
                p.End();
            }

        }

        private bool pluginExists(string pluginName)
        {
            return getPluginByName(pluginName) != null;
        }

        private IPlugin getPluginByName(string pluginName)
        {
            foreach (IPlugin p in plugins)
            {
                if (p.pluginName == pluginName)
                    return p;
            }
            return null;
        }

        private List<PluginInfo> getPluginInfo()
        {
            List<PluginInfo> piList = new List<PluginInfo>(plugins.Count);
            foreach (IPlugin p in plugins)
            {
                piList.Add(new PluginInfo() {
                    name = p.pluginName,
                    imageSource = p.getMenuIcon(),
                    imageSourceHover = p.getMenuIconHover()
                });
            }
            return piList;
        }

        private void Irc_Userstate(object sender, UserstateEventArgs e)
        {
            IRC_Userstate?.Invoke(this, e);
        }

        private void Irc_ModeChanged(object sender, ModeChangedEventArgs e)
        {
            IRC_ModeChanged?.Invoke(this, e);
        }

        private void Irc_MessageReceived(object sender, MsgReceivedEventArgs e)
        {
            IRC_MessageReceived?.Invoke(this, e);
        }

        private void Irc_Part(object sender, JoinPartEventArgs e)
        {
            IRC_Part?.Invoke(this, e);
        }

        private void Irc_Join(object sender, JoinPartEventArgs e)
        {
            IRC_Join?.Invoke(this, e);
        }

        private void Irc_InitCompleted(object sender, EventArgs e)
        {
            IRC_InitCompleted?.Invoke(this, e);
        }

        private void Irc_ConnectionError(object sender, EventArgs e)
        {
            IRC_ConnectionError?.Invoke(this, e);
        }

        private void Irc_Connected(object sender, EventArgs e)
        {
            IRC_Connected?.Invoke(this, e);
        }

        private void Irc_PrivmsgReceived(object sender, PrivmsgReceivedEventArgs e)
        {
            IRC_PrivmsgReceived?.Invoke(this, e);
        }

        public void sendMesssage(string msg)
        {
            irc.send(msg);
        }

        public class PluginInfo
        {
            public string name { get; set; }
            public ImageSource imageSource { get; set; }
            public ImageSource imageSourceHover { get; set; }
        }

    }
}
