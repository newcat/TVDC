# IPluginHost Interface

**Namespace:** tvdc.Plugin

### Methods
Name|Return Type|Description
----|-----------
sendMessage(string)|void|Sends a message to the IRC. If the message starts with an '%', the message will be sent to the IRC "as-is" (raw), else it will be sent as a PRIVMSG (like you would type in the normal Twitch-Chatbox).

### Events
Name|Event Argument|Description
----|--------------|-----------
IRC_Connected|EventArgs|Occurs on successfull connection to the Twitch IRC.
IRC_ConnectionError|EventArgs|Occurs when connecting to the Twitch IRC failed.
IRC_InitCompleted|EventArgs|Occurs when initialization is done, the user is in the right chat room and the IRC is ready for sending messages.
IRC_MessageReceived|[MsgReceivedEventArgs](MsgReceivedEventArgs.md)|Occurs every time a message is received. NOTICE: If you only want to listen to normal chat messages, use the `IRC_PrivmsgReceived` event instead.
IRC_Join|[JoinPartEventArgs](JoinPartEventArgs.md)|Occurs every time a user joins the channel. NOTICE: These events get buffered by Twitch. Expect a lot of JOIN-Events in a very short time period; especially on application startup, when Twitch sends the list of users. You can use the `IRC_InitCompleted` event to see, if a join event comes from the list of users, because then those JOIN-Events will occur before the `IRC_InitCompleted` event.
IRC_Part|[JoinPartEventArgs](JoinPartEventArgs.md)|Occurs every time a user leaves the channel. This event also gets buffered by Twitch.
IRC_ModeChanged|[ModeChangedEventArgs](ModeChangedEventArgs.md)|Occurs every time a user gets modded or unmodded, or if a moderator joins/leaves the channel.
IRC_Userstate|[UserstateEventArgs](UserstateEventArgs.md)|Will occur after every PRIVMSG sent by the client. It contains info about the user using the client.
IRC_PrivmsgReceived|[PrivmsgReceivedEventArgs](PrivmsgReceivedEventArgs.md)|Occurs when a PRIVMSG (a normal Twitch chat message) is received.
