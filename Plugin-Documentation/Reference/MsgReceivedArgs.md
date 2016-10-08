# MsgReceivedArgs Class

### Properties
Name|Type|Description
----|----|-----------
tags|Dictionary<string, string>|Tags which were sent as message prefix by Twitch. Learn more about here: https://github.com/justintv/Twitch-API/blob/master/IRC.md#tags
username|string|Name of the user who sent the message.
message|string|Message or additional information about the NOTICE.

### CAREFUL!
This is NOT the type of event args you get when subscribed to the [IRC_PrivmsgReceived-Event](https://github.com/newcat/TVDC/blob/master/Plugin-Documentation/Reference/IPluginHost.md#events).
Although the [PrivmsgReceivedEventArgs](PrivmsgReceivedEventArgs.md) got the same properties, the tags might be completely different, because the [IRC_MsgReceived-Event](https://github.com/newcat/TVDC/blob/master/Plugin-Documentation/Reference/IPluginHost.md#events) is also fired on `NOTICE`.
