using System;
using System.Collections.Generic;
using System.Drawing;

namespace tvdc
{
    public interface IPlugin
    {

        /// <summary>
        /// The name will be displayed when hovering above the plugin icon.
        /// </summary>
        string pluginName { get; }

        /// <summary>
        /// The icon that will be displayed in the bar with all the plugin icons (right of the settings icon).
        /// </summary>
        /// <returns>The image to be drawn (Size 28x28)</returns>
        Image getMenuIcon();

        /// <summary>
        /// The icon that will be displayed if the user hovers over the menu icon.
        /// </summary>
        /// <returns>The image to be drawn (Size 28x28)</returns>
        Image getMenuIconHover();

        void IconClicked();

        void IRC_Connected();
        void IRC_ConnectionError();
        void IRC_InitCompleted();
        void IRC_MessageReceived(string username, string message, Dictionary<string, string> tags);
        void IRC_Join(string username);
        void IRC_Part(string username);
        void IRC_ModeChanged(string username, bool isMod);
        void IRC_Userstate(Dictionary<string, string> tags);

        /// <summary>
        /// Raise this event with the string you want to send. If the string starts with an '%', the message
        /// will be sent to the IRC "as-is" (raw), else it will be sent as a PRIVMSG (like you would type
        /// in the normal Twitch-Chatbox)
        /// </summary>
        event EventHandler<SendMessageEventArgs> sendMessage;


    }
}
