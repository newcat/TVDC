namespace tvdc
{
    public class PluginClickedEventArgs
    {
        public string PluginName { get; set; }
        public PluginClickedEventArgs(string pluginName) { PluginName = pluginName; }
    }

    public class SendMessageEventArgs
    {
        public string Message { get; set; }
        public SendMessageEventArgs(string message) { Message = message; }
    }
}
