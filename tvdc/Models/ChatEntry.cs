using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

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

        private Type _eventType = Type.ERROR;
        public Type EventType
        {
            get { return _eventType; }
            set
            {
                _eventType = value;
                NotifyPropertyChanged();
            }
        }

        //Username for JOIN, PART and CHAT entries
        private string _username = "";
        public string Username
        {
            get { return _username; }
            set
            {
                _username = value;
                NotifyPropertyChanged();
            }
        }

        //Text property is only used for non-chat entries
        //Chat entries will use the paragraphs-property
        private string _text;
        public string Text
        {
            get { return _text; }
            set
            {
                _text = value;
                NotifyPropertyChanged();
            }
        }

        //Paragraph can be text, url or emoticon
        private List<Paragraph> _paragraphs;
        public List<Paragraph> Paragraphs
        {
            get { return _paragraphs; }
            set
            {
                _paragraphs = value;
                NotifyPropertyChanged();
            }
        }

        //Color for the username and for /me-Messages
        private string _color = "";
        public string Color
        {
            get { return _color; }
            set
            {
                _color = value;
                NotifyPropertyChanged();
            }
        }

        //Badges to display in front of the username
        private List<Badges.BadgeTypes> _badges;
        public List<Badges.BadgeTypes> Badges
        {
            get { return _badges; }
            set
            {
                _badges = value;
                NotifyPropertyChanged();
            }
        }

        private string _originalMessage = "";
        public string OriginalMessage
        {
            get { return _originalMessage; }
        }

        private DateTime _timestamp = DateTime.Now;
        public DateTime Timestamp
        {
            get { return _timestamp; }
        }

        //needed for making the "Now hosting xxx" clickable
        public bool IsHostingMessage
        {
            get { return Text.StartsWith("Now hosting"); }
        }

        //needed for making the "Now hosting xxx" clickable
        public string HostingChannelName
        {
            get { return Text.Substring(12).TrimEnd('.'); }
        }


        //Constructor for IRC, Join/Part and error entries
        public ChatEntry(Type eventType, string text, string username = "")
        {
            EventType = eventType;
            Text = text;
            Username = username;
        }

        //Constructor for chat entries
        public ChatEntry(string username, string color, List<Paragraph> paragraphs, List<Badges.BadgeTypes> badges, string originalMessage)
        {
            EventType = Type.CHAT;
            Username = username;
            Color = color;
            Paragraphs = paragraphs;
            Badges = badges;
            _originalMessage = originalMessage;
        }

    }
}
