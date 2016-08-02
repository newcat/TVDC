using System;
using System.Collections.Generic;

namespace tvdc.EventArguments
{

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

}