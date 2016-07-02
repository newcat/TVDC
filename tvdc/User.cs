using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Net;
using System.Web.Script.Serialization;
using System.Windows;

namespace tvdc
{
    public class User : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
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

        private bool _isMod;
        public bool isMod
        {
            get { return _isMod; }
            set
            {
                _isMod = value;
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

        public User(string name, bool isMod)
        {
            lock (MainWindowVM.viewerListLock)
            {
                _name = name;
                _isMod = isMod;
                updating = true;
                color = TwitchColors.getColorByUsername(name);
            }

            if (!name.Equals("404"))
                getDisplayName();
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

    }
}
