using System;

namespace tvdc
{
    public class PluginClickedEventArgs : EventArgs
    {
        public string PluginName { get; set; }
        public PluginClickedEventArgs(string pluginName) { PluginName = pluginName; }
    }

    public class SendMessageEventArgs : EventArgs
    {
        public string Message { get; set; }
        public SendMessageEventArgs(string message) { Message = message; }
    }
}
