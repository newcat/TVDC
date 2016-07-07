using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Collections.Generic;

namespace tvdc
{
    public class ChatEntry : INotifyPropertyChanged
    {

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
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

        private Dictionary<string, string> _tags;
        public Dictionary<string, string> tags
        {
            get { return _tags; }
            set
            {
                _tags = value;
                NotifyPropertyChanged();
            }
        }

        public string color { get; set; }
        public bool isMod { get; set; }
        public Guid eventID { get; }

        /// <summary>
        /// Creates a new chat entry in the chat listbox
        /// </summary>
        /// <param name="eventType">Determines the control template.</param>
        /// <param name="text">Can be an empty string if type is 'CHAT'</param>
        /// <param name="username">Only needed for types 'JOIN', 'PART' or 'CHAT'</param>
        /// <param name="color">Color of the username when type is 'CHAT'</param>
        /// <param name="tags"></param>
        public ChatEntry(Type eventType, string text, string username = "", string color = "", Dictionary<string, string> tags = null)
        {
            eventID = Guid.NewGuid();
            this.eventType = eventType;
            this.text = text;
            this.username = username;
            this.color = color;
            this.tags = tags;

            if (this.eventType == Type.CHAT)
                this.tags.Add("text", this.text);

        }

    }
}
