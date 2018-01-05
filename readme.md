# Twitch Viewer Display

## NOTE
This project is no longer supported but feel free to fork and use it (or parts of it) in your own project(s).

---

You are a streamer? You want a flexible tool to watch your chat, manage your viewers easily and have cool options like doing polls and giveaways directly in the chat?
Then this program is for you!

#### Features:
* Live chat with badge and emoticon support
* Displays when a user joins or leaves the chatroom
* Shows viewer count and graphs it over time
* Live follower count
* Displays the amount of follower vs. non-follower in your chat
* Easily mod, unmod or timeout a user with a simple right click on their name
* Huge customization possible with plugins. Currently included are plugins for polls, giveaways and loyalty
* Stay up to date with a simple auto-updater - no need for manual downloads.
* Sleek design

### Plugin documentation:
If you want to write a plugin, there's a how-to [here](Plugin-Documentation/).

### FAQ:
##### Why do Join/Leave-Events get displayed delayed?
For better performance Twitch caches all the events like joining and leaving and only sends them to the client every 10 seconds to 2 minutes, depending on the servers load.

##### Why does this program display a different viewer count than the Twitch-site?
At the moment, this program only shows the amount of users who are logged in to the chat. Also, if a stream has over 1,000 viewers, Twitch will only send the names of the mods.
