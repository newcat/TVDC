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

        private Guid _eventID = Guid.NewGuid();
        public Guid EventID { get; }


        public ChatEntry(Type eventType, string text, string username = "")
        {
            EventType = eventType;
            Text = text;
            Username = username;
        }

        public ChatEntry(string username, string color, List<Paragraph> paragraphs, List<Badges.BadgeTypes> badges)
        {
            EventType = Type.CHAT;
            Username = username;
            Color = color;
            Paragraphs = paragraphs;
            Badges = badges;
        }

    }
}
