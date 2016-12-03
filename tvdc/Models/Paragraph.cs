using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace tvdc
{
    public class Paragraph : INotifyPropertyChanged
    {

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private bool _isAction = false;
        public bool IsAction
        {
            get { return _isAction; }
            set
            {
                _isAction = value;
                NotifyPropertyChanged();
            }
        }

        private bool _isImage = false;
        public bool IsImage
        {
            get { return _isImage; }
            set
            {
                _isImage = value;
                NotifyPropertyChanged();
            }
        }

        private bool _isURL = false;
        public bool IsURL
        {
            get { return _isURL; }
            set
            {
                _isURL = value;
                NotifyPropertyChanged();
            }
        }

        public string ImageSource
        {
            get
            {
                Emoticon e = EmoticonManager.RequestEmoticon(emoteID);
                if (e.IsLoaded)
                {
                    return e.Image;
                } else
                {
                    if (!isSubscribed)
                    {
                        e.ImageDownloadFinished += E_ImageDownloadFinished;
                        isSubscribed = true;
                    }
                    return "";
                }
            }
        }

        public int ImageWidth
        {
            get
            {
                Emoticon e = EmoticonManager.RequestEmoticon(emoteID);
                if (e.IsLoaded)
                    return e.Width;
                return 0;
            }
        }

        public int ImageHeight
        {
            get
            {
                Emoticon e = EmoticonManager.RequestEmoticon(emoteID);
                if (e.IsLoaded)
                    return e.Height;
                return 0;
            }
        }

        private void E_ImageDownloadFinished(object sender, EventArgs e)
        {
            Emoticon em = (Emoticon)sender;
            em.ImageDownloadFinished -= E_ImageDownloadFinished;
            NotifyPropertyChanged("ImageSource");
            NotifyPropertyChanged("ImageWidth");
            NotifyPropertyChanged("ImageHeight");
        }

        private string _text = "";
        public string Text
        {
            get { return _text; }
            set
            {
                _text = value;
                NotifyPropertyChanged();
            }
        }

        public RelayCommand cmdUrlClicked { get; private set; }

        private int emoteID;
        private bool isSubscribed = false;

        public Paragraph(int emoteID)
        {
            this.emoteID = emoteID;
            IsImage = true;
        }

        public Paragraph(string text, bool isAction = false, bool isURL = false)
        {
            Text = text;
            IsAction = isAction;
            IsURL = isURL;

            cmdUrlClicked = new RelayCommand(() => { Process.Start(Text); }, () => { return IsURL; });
        }

    }
}
