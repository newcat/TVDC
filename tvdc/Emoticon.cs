using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Drawing;

namespace tvdc
{
    class Emoticon
    {

        public int id { get; private set; }
        public bool isLoaded { get; private set; }
        public int width { get; private set; }
        public int height { get; private set; }
        public string image { get; private set; }

        public delegate void imageDownloadFinishedHandler(object sender, EventArgs e);
        public event imageDownloadFinishedHandler ImageDownloadFinished;

        private const string baseURL = "http://static-cdn.jtvnw.net/emoticons/v1/{0}/1.0";

        WebClient wc;

        public Emoticon(int id)
        {
            this.id = id;

            if (!EmoticonManager.isCached(id))
            {
                wc = new WebClient();
                wc.DownloadFileCompleted += Wc_DownloadFileCompleted;
                wc.DownloadFileAsync(new Uri(string.Format(baseURL, id.ToString())), EmoticonManager.tempPath + id.ToString() + ".png");
            } else
            {
                isLoaded = true;
                image = EmoticonManager.tempPath + id.ToString() + ".png";
                loadDimensions();
            }
        }

        private void Wc_DownloadFileCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            wc.Dispose();
            isLoaded = true;
            image = EmoticonManager.tempPath + id.ToString() + ".png";
            loadDimensions();
            if (ImageDownloadFinished != null)
                ImageDownloadFinished(this, EventArgs.Empty);
        }

        private void loadDimensions()
        {
            Image img = Image.FromFile(image);
            width = img.Width;
            height = img.Height;
            img.Dispose();
        }

    }
}
