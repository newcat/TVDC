using System;

namespace tvdc
{
    public class SendMessageEventArgs : EventArgs
    {
        public string message { get; private set; }

        public SendMessageEventArgs(string message) : base()
        {
            this.message = message;
        }
    }
}
