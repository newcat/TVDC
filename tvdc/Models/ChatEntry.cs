using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace tvdc
{
    public class ChatEntry : INotifyPropertyChanged
    {

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        public enum Type
        {
            ERROR, IRC, JOIN, PART, CHAT
        }

        public Type eventType { get; set; }
        public string username { get; set; }

        private string _text;
        public string text
        {
            get { return _text; }
            set
            {
                _text = value;
                NotifyPropertyChanged();
            }
        }

        private IRCClient.PrivmsgReceivedEventArgs _data;
        public IRCClient.PrivmsgReceivedEventArgs data
        {
            get { return _data; }
            set
            {
                _data = value;
                NotifyPropertyChanged();
            }
        }

        public string color { get; set; }
        public bool isMod { get; set; }
        public Guid eventID { get; }

        public ChatEntry(Type eventType, string text, string username = "", string color = "", bool isMod = false, IRCClient.PrivmsgReceivedEventArgs data = null)
        {
            eventID = Guid.NewGuid();
            this.eventType = eventType;
            this.text = text;
            this.username = username;
            this.color = color;
            this.isMod = isMod;
            this.data = data;
        }

    }
}
