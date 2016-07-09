using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Net;
using System.Web.Script.Serialization;
using System.Windows;
using System.Linq;

namespace tvdc
{
    public class User : INotifyPropertyChanged, IComparable<User>, IEquatable<User>
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private string _color;
        public string color
        {
            get { return _color; }
            set
            {
                _color = value;
                NotifyPropertyChanged();
            }
        }

        private string _name;
        public string name
        {
            get { return _name; }
            set
            {
                _name = value;
                NotifyPropertyChanged();
            }
        }

        private string _displayName;
        public string displayName
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

        private bool _isFollower;
        public bool isFollower
        {
            get { return _isFollower; }
            set
            {
                _isFollower = value;
                NotifyPropertyChanged();
            }
        }

        public bool updating { get; private set; }

        private List<Badges.BadgeTypes> _badges;
        public List<Badges.BadgeTypes> badges
        {
            get { return _badges; }
            set
            {
                _badges = value;
                NotifyPropertyChanged();
            }
        }
        public int badgeLevel { get; private set; }

        public delegate void BadgeChangedHandler(object sender, EventArgs e);
        public event BadgeChangedHandler BadgeChanged;
        public bool subscribedToBadgeChangedEvent { get; set; }

        public User(string name, List<Badges.BadgeTypes> badges = null)
        {
            lock (MainWindowVM.viewerListLock)
            {
                _name = name;
                updating = true;
                color = TwitchColors.getColorByUsername(name);

                if (badges != null)
                {
                    this.badges = badges;
                    badgeLevel = getBadgeLevel();
                } else
                {
                    this.badges = new List<Badges.BadgeTypes>();
                    badgeLevel = 0;
                }
            }

            if (!name.Equals("404"))
                getDisplayName();
        }

        public void addBadge(Badges.BadgeTypes badge)
        {
            lock (MainWindowVM.viewerListLock)
            {
                if (!badges.Contains(badge))
                {
                    badges.Add(badge);
                    badgeLevel = getBadgeLevel();
                    BadgeChanged?.Invoke(this, new EventArgs());
                }
            }
        }

        public void setBadges(List<Badges.BadgeTypes> badges)
        {
            lock (MainWindowVM.viewerListLock)
            {
                if (!equalBadges(badges))
                {
                    this.badges = badges;
                    badgeLevel = getBadgeLevel();
                    BadgeChanged?.Invoke(this, new EventArgs());
                }
            }
        }

        private bool equalBadges(List<Badges.BadgeTypes> badges)
        {
            if (this.badges.Count != badges.Count)
                return false;

            foreach (Badges.BadgeTypes b in this.badges)
            {
                if (!badges.Contains(b))
                    return false;
            }

            return true;
        }

        public void TryRemoveBadge(Badges.BadgeTypes b)
        {
            lock (MainWindowVM.viewerListLock)
            {
                bool success = badges.Remove(b);

                if (success)
                {
                    badgeLevel = getBadgeLevel();
                    BadgeChanged?.Invoke(this, new EventArgs());
                }
            }
        }

        private async void getDisplayName()
        {

            WebClient wr = new WebClient();
            string json = "";

            wr.Headers.Add("Client-ID", (string)Application.Current.Resources["client_id"]);

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
                displayName = dict["display_name"].ToString();
            }

            try {
                json = await wr.DownloadStringTaskAsync(string.Format("https://api.twitch.tv/kraken/users/{0}/follows/channels/{1}", name, Properties.Settings.Default.channel));
            }
            catch (WebException ex)
            {
                if (ex.Message.Contains("404"))
                {
                    lock (MainWindowVM.viewerListLock)
                    {
                        isFollower = false;
                        updating = false;
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
                    isFollower = true;
                    updating = false;
                }
            } else
            {
                lock (MainWindowVM.viewerListLock)
                {
                    isFollower = false;
                    updating = false;
                }
            }

            wr.Dispose();

        }

        private int getBadgeLevel()
        {

            int badgeLevel = 0;

            if (badges.Contains(Badges.BadgeTypes.SUBSCRIBER))
                badgeLevel = 1;

            if (badges.Contains(Badges.BadgeTypes.TURBO))
                badgeLevel = 2;

            if (badges.Contains(Badges.BadgeTypes.MODERATOR))
                badgeLevel = 3;

            if (badges.Contains(Badges.BadgeTypes.BROADCASTER))
                badgeLevel = 4;

            if (badges.Contains(Badges.BadgeTypes.GlOBAL_MOD))
                badgeLevel = 5;

            if (badges.Contains(Badges.BadgeTypes.ADMIN))
                badgeLevel = 6;

            if (badges.Contains(Badges.BadgeTypes.STAFF))
                badgeLevel = 7;

            return badgeLevel;

        }

        public int CompareTo(User other)
        {
  
            if (badgeLevel < other.badgeLevel)
            {
                return 1;
            } else if (badgeLevel == other.badgeLevel)
            {
                return name.CompareTo(other.name);
            } else
            {
                return -1;
            }

        }

        public bool Equals(User other)
        {
            return name == other.name;
        }
    }
}
