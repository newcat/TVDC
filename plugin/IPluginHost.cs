using System;
using tvdc.EventArguments;

namespace tvdc.Plugin
{
    public interface IPluginHost
    {

        event EventHandler<EventArgs> IRC_Connected;
        event EventHandler<EventArgs> IRC_ConnectionError;
        event EventHandler<EventArgs> IRC_InitCompleted;
        event EventHandler<MsgReceivedEventArgs> IRC_MessageReceived;
        event EventHandler<JoinPartEventArgs> IRC_Join;
        event EventHandler<JoinPartEventArgs> IRC_Part;
        event EventHandler<ModeChangedEventArgs> IRC_ModeChanged;
        event EventHandler<UserstateEventArgs> IRC_Userstate;
        event EventHandler<PrivmsgReceivedEventArgs> IRC_PrivmsgReceived;

        /// <summary>
        /// Sends a message to the IRC. If the message starts with an '%', the message
        /// will be sent to the IRC "as-is" (raw), else it will be sent as a PRIVMSG (like you would type
        /// in the normal Twitch-Chatbox)
        /// </summary>
        /// <param name="msg">The message to send</param>
        void sendMesssage(string msg);

    }
}
