using System;
using tvdc.EventArguments;

namespace tvdc.Plugin
{
    public interface IPluginHost
    {

        /// <summary>
        /// Occurs on successfull connection to the Twitch IRC.
        /// </summary>
        event EventHandler<EventArgs> IRC_Connected;

        /// <summary>
        /// Occurs when connecting to the Twitch IRC failed.
        /// </summary>
        event EventHandler<EventArgs> IRC_ConnectionError;

        /// <summary>
        /// Occurs when initialization is done, the user is in the right chat room and the
        /// IRC is ready for sending messages.
        /// </summary>
        event EventHandler<EventArgs> IRC_InitCompleted;

        /// <summary>
        /// Occurs every time a message is received. NOTICE: If you only want to listen to normal
        /// chat messages, use the <see cref="IRC_PrivmsgReceived"/> event instead.
        /// </summary>
        event EventHandler<MsgReceivedEventArgs> IRC_MessageReceived;

        /// <summary>
        /// Occurs every time a user joins the channel.
        /// NOTICE: These events get buffered by Twitch. Expect a lot of JOIN-Events in a very short
        /// time period; especially on application startup, when Twitch sends the list of users.
        /// You can use the <see cref="IRC_InitCompleted"/> event to see, if a join event comes from the
        /// list of users, because then those JOIN-Events will occur before the <see cref="IRC_InitCompleted"/> event. 
        /// </summary>
        event EventHandler<JoinPartEventArgs> IRC_Join;

        /// <summary>
        /// Occurs every time a user leaves the channel. This event also gets buffered by Twitch.
        /// </summary>
        event EventHandler<JoinPartEventArgs> IRC_Part;

        /// <summary>
        /// Occurs every time a user gets modded or unmodded, or if a moderator joins/leaves the channel.
        /// </summary>
        event EventHandler<ModeChangedEventArgs> IRC_ModeChanged;

        /// <summary>
        /// Will occur after every PRIVMSG sent by the client. It contains info about the user using the client.
        /// </summary>
        event EventHandler<UserstateEventArgs> IRC_Userstate;

        /// <summary>
        /// Occurs when a PRIVMSG (a normal Twitch chat message) is received.
        /// </summary>
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
