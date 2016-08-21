using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

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
                Emoticon e = EmoticonManager.requestEmoticon(emoteID);
                if (e.isLoaded)
                {
                    return e.image;
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

        private void E_ImageDownloadFinished(object sender, EventArgs e)
        {
            Emoticon em = (Emoticon)sender;
            em.ImageDownloadFinished -= E_ImageDownloadFinished;
            NotifyPropertyChanged("ImageSource");
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
        }

    }
}
