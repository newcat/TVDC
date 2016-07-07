using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using System.IO;

namespace tvdc
{
    public class IRCClient : IDisposable
    {

        #region Events
        //EventArgs-Classes
        public class MsgReceivedEventArgs : EventArgs
        {
            public Dictionary<string, string> tags { get; set; }
            public string username { get; set; }
            public string message { get; set; }
        }

        public class JoinPartEventArgs : EventArgs
        {
            public string username { get; set; }
        }

        public class ModeChangedEventArgs : EventArgs
        {
            public string username { get; set; }
            public bool isMod { get; set; }
        }

        public class PrivmsgReceivedEventArgs : EventArgs
        {
            public string username { get; set; }
            public string message { get; set; }
            public Dictionary<string, string> tags { get; set; }
        }

        public class UserstateEventArgs : EventArgs
        {
            public Dictionary<string, string> tags { get; set; }
        }

        //Delegates
        public delegate void connectedHandler(object sender, EventArgs e);
        public delegate void connectionErrorHandler(object sender, EventArgs e);
        public delegate void initCompletedHandler(object sender, EventArgs e);
        public delegate void msgReceivedHandler(object sender, MsgReceivedEventArgs e);
        public delegate void joinPartHandler(object sender, JoinPartEventArgs e);
        public delegate void privmsgReceivedHandler(object sender, PrivmsgReceivedEventArgs e);
        public delegate void modeChangedHandler(object sender, ModeChangedEventArgs e);
        public delegate void userstateHandler(object sender, UserstateEventArgs e);

        //Events
        public event connectedHandler Connected;
        public event connectionErrorHandler ConnectionError;
        public event initCompletedHandler InitCompleted;
        public event msgReceivedHandler MessageReceived;
        public event msgReceivedHandler MessageSent;
        public event msgReceivedHandler Notice;
        public event joinPartHandler Join;
        public event joinPartHandler Part;
        public event privmsgReceivedHandler PrivmsgReceived;
        public event modeChangedHandler ModeChanged;
        public event userstateHandler Userstate;
        public event joinPartHandler Clearchat;
        #endregion

        private NetworkStream tcpStream;
        private StreamWriter tcpWriter;
        private StreamReader tcpReader;
        private TcpClient tcpClient;

        public bool initialized { get; private set; }
        public string ip { get; }
        public int port { get; }

        private string oauth;
        private string nick;
        private string channel;

        private Thread receiveThread;

        //IRC Constants
        private enum IRC_Commands
        {
            INVALID = 20,
            JOIN,
            PART,
            MODE,
            NOTICE,
            HOSTTARGET,
            CLEARCHAT,
            USERSTATE,
            ROOMSTATE,
            PRIVMSG,
            PING,
            GLOBALUSERSTATE,
            RPL_ENDOFMOTD = 376,
            RPL_NAMREPLY = 353,
            RPL_ENDOFNAMES = 366,
            ERR_UNKNOWNCOMMAND = 421,
        }


        public IRCClient(string ip, int port, string nick, string oauth, string channel)
        {
            this.ip = ip;
            this.port = port;
            this.nick = nick;
            this.oauth = oauth;
            this.channel = channel;

            initialized = false;

            tcpClient = new TcpClient();
            receiveThread = new Thread(new ThreadStart(receive));
        }

        public async void connect()
        {

            try
            {
                await tcpClient.ConnectAsync(ip, port);
            } catch (SocketException)
            {
                ConnectionError?.Invoke(this, new EventArgs());
                return;
            }

            if (tcpClient.Connected)
            {
                Connected?.Invoke(this, new EventArgs());

                tcpStream = tcpClient.GetStream();
                tcpWriter = new StreamWriter(tcpStream);
                tcpReader = new StreamReader(tcpStream);

                receiveThread.Start();
            } else
            {
                ConnectionError?.Invoke(this, new EventArgs());
            }

            rawSend(string.Format("PASS {0}", oauth));
            rawSend(string.Format("NICK {0}", nick));

        }

        public void send(string msg)
        {
            if (msg.StartsWith("%"))
            {
                rawSend(msg.TrimStart('%'));
            } else
            {
                rawSend(string.Format("PRIVMSG #{0} :{1}", channel, msg));
            }
        }

        private void rawSend(string msg)
        {
            if (tcpClient.Connected)
            {
                tcpWriter.WriteLine(msg);
                tcpWriter.Flush();
                MessageSent?.Invoke(this, new MsgReceivedEventArgs() { message = msg });
            }
        }

        private void rawSend(IRCMessage msg)
        {
            rawSend(msg.ToString());
        }

        private void receive()
        {
            string receivedMessage = "";

            while (true)
            {

                receivedMessage = tcpReader.ReadLine();

                if (receivedMessage == null)
                    break;

                MessageReceived?.Invoke(this, new MsgReceivedEventArgs() { message = receivedMessage });

                IRCMessage parsedMessage = IRCMessage.FromMessage(receivedMessage);

                switch (parsedMessage.command)
                {
                    case IRC_Commands.PING:
                        rawSend("PONG tmi.twitch.tv");
                        break;

                    case IRC_Commands.NOTICE: //:tmi.twitch.tv NOTICE * :Error logging in
                        if (string.Join(" ", parsedMessage.command_params[1], parsedMessage.command_params[2], parsedMessage.command_params[3]).Equals(":Error logging in")) {
                            ConnectionError?.Invoke(this, new EventArgs());
                        } else
                        {
                            string Nmessage = parsedMessage.command_params[1].Substring(1) + " ";
                            for (int i = 2; i < parsedMessage.command_params.Length; i++)
                            {
                                Nmessage += parsedMessage.command_params[i] + " ";
                            }
                            Notice?.Invoke(this, new MsgReceivedEventArgs() { message = Nmessage.TrimEnd(' '), tags = parsedMessage.tags });
                        }
                        break;

                    case IRC_Commands.RPL_NAMREPLY:
                        for (int i = 3; i < parsedMessage.command_params.Length; i++)
                        {
                            Join?.Invoke(this, new JoinPartEventArgs() { username = parsedMessage.command_params[i].TrimStart(':') });
                        }
                        break;

                    case IRC_Commands.RPL_ENDOFNAMES:
                        initialized = true;
                        InitCompleted?.Invoke(this, new EventArgs());
                        break;

                    case IRC_Commands.RPL_ENDOFMOTD:
                        rawSend("CAP REQ :twitch.tv/membership");
                        rawSend("CAP REQ :twitch.tv/commands");
                        rawSend("CAP REQ :twitch.tv/tags");
                        rawSend(string.Format("JOIN #{0}", channel));
                        break;

                    case IRC_Commands.JOIN:
                        Join?.Invoke(this, new JoinPartEventArgs() { username = parsedMessage.prefix.Split('!')[0].TrimStart(':') });
                        break;

                    case IRC_Commands.PART:
                        Part?.Invoke(this, new JoinPartEventArgs() { username = parsedMessage.prefix.Split('!')[0].TrimStart(':') });
                        break;

                    case IRC_Commands.MODE:
                        bool isMod = false;
                        if (parsedMessage.command_params[1] == "+o")
                        {
                            isMod = true;
                        }
                        ModeChanged?.Invoke(this, new ModeChangedEventArgs() { username = parsedMessage.command_params[2], isMod = isMod });
                        break;

                    case IRC_Commands.CLEARCHAT:
                        if (parsedMessage.command_params.Length == 2)
                        {
                            Clearchat?.Invoke(this, new JoinPartEventArgs() { username = parsedMessage.command_params[1].TrimStart(':') });
                        } else
                        {
                            Clearchat?.Invoke(this, new JoinPartEventArgs());
                        }
                        break;

                    case IRC_Commands.ERR_UNKNOWNCOMMAND:
                        break;

                    case IRC_Commands.GLOBALUSERSTATE:
                        //Not yet implemented from Twitch's side, probably never useful for this program
                        break;

                    case IRC_Commands.HOSTTARGET:
                        //Not needed since notification will come with NOTICE command too
                        break;

                    case IRC_Commands.INVALID:
                        //Unknonwn command -> no action
                        break;

                    case IRC_Commands.PRIVMSG:
                        string username;
                        if (parsedMessage.tags != null && parsedMessage.tags.ContainsKey("display-name") && parsedMessage.tags["display-name"] != "")
                        {
                            username = parsedMessage.tags["display-name"];
                        } else
                        {
                            username = parsedMessage.prefix.Split('!')[0].TrimStart(':');
                        }

                        string message = parsedMessage.command_params[1].Substring(1) + " ";
                        for (int i = 2; i < parsedMessage.command_params.Length; i++)
                        {
                            message += parsedMessage.command_params[i] + " ";
                        }

                        PrivmsgReceived?.Invoke(this, new PrivmsgReceivedEventArgs() { tags = parsedMessage.tags, username = username, message = message });
                        break;

                    case IRC_Commands.ROOMSTATE:
                        //Not needed for this program
                        break;

                    case IRC_Commands.USERSTATE:
                        Userstate?.Invoke(this, new UserstateEventArgs() { tags = parsedMessage.tags });
                        break;

                }
                
            }

            disconnect();

        }

        public void disconnect()
        {
            receiveThread.Abort();
            tcpClient.Close();
        }

        private class IRCMessage
        {

            public Dictionary<string, string> tags { get; set; }
            public string prefix { get; }
            public IRC_Commands command { get; }
            public string[] command_params { get; }

            public IRCMessage(string prefix, IRC_Commands command, Dictionary<string, string> tags, params string[] command_params)
            {
                this.prefix = prefix;
                this.command = command;
                this.command_params = command_params;
                this.tags = tags;
            }

            public static IRCMessage FromMessage(string msg)
            {

                Dictionary<string, string> tags = new Dictionary<string, string>();
                string prefix;
                IRC_Commands command;
                string[] command_params;

                if (msg.StartsWith("@"))
                {
                    string[] tagsArray = msg.TrimStart('@').Split(new string[1] { " :" }, StringSplitOptions.RemoveEmptyEntries)[0].TrimEnd(' ').Split(';');
                    foreach (string tag in tagsArray)
                    {
                        string[] split = tag.Split('=');
                        if (split.Length == 1)
                        {
                            tags.Add(split[0], "");
                        } else
                        {
                            tags.Add(split[0], split[1]);
                        }
                    }

                    msg = msg.Substring(msg.IndexOf(" :") + 1);
                }

                string[] parts = msg.Split(' ');

                if (parts[0].StartsWith(":"))
                {
                    //Prefix is existing
                    prefix = parts[0];
                    if (!Enum.TryParse(parts[1], out command))
                        command = IRC_Commands.INVALID;
                    command_params = new string[parts.Length - 2];
                    for (int i = 2; i < parts.Length; i++)
                    {
                        command_params[i - 2] = parts[i];
                    }
                } else
                {
                    //Prefix not existing
                    prefix = "";
                    if (!Enum.TryParse(parts[0], out command))
                        command = IRC_Commands.INVALID;
                    command_params = new string[parts.Length - 1];
                    for (int i = 1; i < parts.Length; i++)
                    {
                        command_params[i - 1] = parts[i];
                    }
                }

                return new IRCMessage(prefix, command, tags, command_params);
            }

            public override string ToString()
            {
                string ret = "";

                if (prefix != null && prefix != "")
                    ret += ":" + prefix + " ";

                if ((int)command < 100)
                {
                    ret += Enum.GetName(typeof(IRC_Commands), command) + " ";
                } else
                {
                    ret += ((int)command).ToString() + " ";
                }

                foreach (string s in command_params)
                {
                    ret += s + " ";
                }

                return ret.TrimEnd(' ');
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
                    tcpReader.Close();
                    tcpWriter.Close();
                    tcpStream.Close();
                    tcpClient.Close();
                }
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
        #endregion

    }

}
