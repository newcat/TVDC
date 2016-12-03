using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Net;
using System.Web.Script.Serialization;
using System.Windows;

namespace tvdc
{
    public class User : INotifyPropertyChanged, IComparable<User>, IEquatable<User>
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private string _id;
        public string Id
        {
            get { return _id; }
            private set
            {
                _id = value;
                NotifyPropertyChanged();
            }
        }

        private string _color;
        public string Color
        {
            get { return _color; }
            set
            {
                _color = value;
                NotifyPropertyChanged();
            }
        }

        private string _name;
        public string Name
        {
            get { return _name; }
            private set
            {
                _name = value;
                NotifyPropertyChanged();
            }
        }

        private string _displayName;
        public string DisplayName
        {
            get
            {
                if (_displayName == null || _displayName == "")
                {
                    return _name;
                } else
                {
                    return _displayName;
                }
            }
            set
            {
                _displayName = value;
                NotifyPropertyChanged();
            }
        }

        private string _bio;
        public string Bio
        {
            get { return _bio; }
            private set
            {
                _bio = value;
                NotifyPropertyChanged();
            }
        }

        private bool _isFollower;
        public bool IsFollower
        {
            get { return _isFollower; }
            set
            {
                _isFollower = value;
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
        public int BadgeLevel { get; private set; }

        public bool Updating { get; private set; }

        public delegate void BadgeChangedHandler(object sender, EventArgs e);
        public event BadgeChangedHandler BadgeChanged;
        public bool SubscribedToBadgeChangedEvent { get; set; }

        public User(string name, List<Badges.BadgeTypes> badges = null)
        {
            lock (MainWindowVM.viewerListLock)
            {
                _name = name;
                Updating = true;
                Color = TwitchColors.GetColorByUsername(name);

                if (badges != null)
                {
                    Badges = badges;
                    BadgeLevel = getBadgeLevel();
                } else
                {
                    Badges = new List<Badges.BadgeTypes>();
                    BadgeLevel = 0;
                }
            }

            if (!name.Equals("404"))
                retrieveUserData();
        }

        public void AddBadge(Badges.BadgeTypes badge)
        {
            lock (MainWindowVM.viewerListLock)
            {
                if (!Badges.Contains(badge))
                {
                    Badges.Add(badge);
                    BadgeLevel = getBadgeLevel();
                    BadgeChanged?.Invoke(this, new EventArgs());
                }
            }
        }

        public void SetBadges(List<Badges.BadgeTypes> badges)
        {
            lock (MainWindowVM.viewerListLock)
            {
                if (!equalBadges(badges))
                {
                    Badges = badges;
                    BadgeLevel = getBadgeLevel();
                    BadgeChanged?.Invoke(this, new EventArgs());
                }
            }
        }

        private bool equalBadges(List<Badges.BadgeTypes> badges)
        {
            if (Badges.Count != badges.Count)
                return false;

            foreach (Badges.BadgeTypes b in Badges)
            {
                if (!badges.Contains(b))
                    return false;
            }

            return true;
        }

        public bool TryRemoveBadge(Badges.BadgeTypes b)
        {
            bool success;
            lock (MainWindowVM.viewerListLock)
            {
                success = Badges.Remove(b);

                if (success)
                {
                    BadgeLevel = getBadgeLevel();
                    BadgeChanged?.Invoke(this, new EventArgs());
                }
            }
            return success;
        }

        private async void retrieveUserData()
        {

            WebClient wr = new WebClient();
            string json = "";

            //TODO: Update to Twitch API Version 5
            wr.Headers.Add("Client-ID", Properties.Resources.client_id);

            try
            {
                json = await wr.DownloadStringTaskAsync("https://api.twitch.tv/kraken/users/" + _name);
            }
            catch (Exception)
            {
                return;
            }

            JavaScriptSerializer jsonSerializer = new JavaScriptSerializer();
            Dictionary<string, object> dict = jsonSerializer.Deserialize<Dictionary<string, object>>(json);

            lock (MainWindowVM.viewerListLock)
            {
                DisplayName = dict["display_name"].ToString();
                Id = dict["_id"].ToString();
                Bio = dict["bio"] != null ? dict["bio"].ToString() : "";
            }

            try {
                json = await wr.DownloadStringTaskAsync(
                    string.Format("https://api.twitch.tv/kraken/users/{0}/follows/channels/{1}",
                        Name, Properties.Settings.Default.channel));
            }
            catch (WebException ex)
            {
                if (ex.Message.Contains("404"))
                {
                    lock (MainWindowVM.viewerListLock)
                    {
                        IsFollower = false;
                        Updating = false;
                    }
                }
                return;
            }
            catch (Exception)
            {
                return;
            }

            dict = jsonSerializer.Deserialize<Dictionary<string, object>>(json);

            if (dict.ContainsKey("channel"))
            {
                lock (MainWindowVM.viewerListLock)
                {
                    IsFollower = true;
                    Updating = false;
                }
            } else
            {
                lock (MainWindowVM.viewerListLock)
                {
                    IsFollower = false;
                    Updating = false;
                }
            }

            wr.Dispose();

        }

        private int getBadgeLevel()
        {

            int badgeLevel = 0;

            if (Badges.Contains(tvdc.Badges.BadgeTypes.TURBO))
                badgeLevel = 1;

            if (Badges.Contains(tvdc.Badges.BadgeTypes.PREMIUM))
                badgeLevel = 1;

            if (Badges.Contains(tvdc.Badges.BadgeTypes.SUBSCRIBER))
                badgeLevel = 2;

            if (Badges.Contains(tvdc.Badges.BadgeTypes.MODERATOR))
                badgeLevel = 3;

            if (Badges.Contains(tvdc.Badges.BadgeTypes.BROADCASTER))
                badgeLevel = 4;

            if (Badges.Contains(tvdc.Badges.BadgeTypes.GLOBAL_MOD))
                badgeLevel = 5;

            if (Badges.Contains(tvdc.Badges.BadgeTypes.ADMIN))
                badgeLevel = 6;

            if (Badges.Contains(tvdc.Badges.BadgeTypes.STAFF))
                badgeLevel = 7;

            return badgeLevel;

        }

        public int CompareTo(User other)
        {
  
            if (BadgeLevel < other.BadgeLevel)
            {
                return 1;
            } else if (BadgeLevel == other.BadgeLevel)
            {
                return Name.CompareTo(other.Name);
            } else
            {
                return -1;
            }

        }

        public bool Equals(User other)
        {
            return Name == other.Name;
        }
    }
}
