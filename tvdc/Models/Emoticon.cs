using System;
using System.Net;
using System.Drawing;
using System.IO;

namespace tvdc
{
    class Emoticon : IDisposable
    {

        public int Id { get; private set; }
        public bool IsLoaded { get; private set; }
        public int Width { get; private set; }
        public int Height { get; private set; }
        public string Image { get; private set; }

        public delegate void imageDownloadFinishedHandler(object sender, EventArgs e);
        public event imageDownloadFinishedHandler ImageDownloadFinished;

        private const string baseURL = "http://static-cdn.jtvnw.net/emoticons/v1/{0}/1.0";

        WebClient wc;

        public Emoticon(int id)
        {
            Id = id;

            if (!EmoticonManager.IsCached(id))
            {
                wc = new WebClient();
                wc.Encoding = System.Text.Encoding.UTF8;
                wc.DownloadFileCompleted += Wc_DownloadFileCompleted;
                wc.DownloadFileAsync(new Uri(string.Format(baseURL, id.ToString())), EmoticonManager.TempPath + id.ToString() + ".png");
            } else
            {
                IsLoaded = true;
                Image = EmoticonManager.TempPath + id.ToString() + ".png";
                loadDimensions();
            }
        }

        private void Wc_DownloadFileCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            wc.Dispose();
            IsLoaded = true;
            Image = EmoticonManager.TempPath + Id.ToString() + ".png";
            loadDimensions();
            ImageDownloadFinished?.Invoke(this, EventArgs.Empty);
        }

        private void loadDimensions()
        {
            try
            {
                Image img = System.Drawing.Image.FromFile(Image);
                Width = img.Width;
                Height = img.Height;
                img.Dispose();
            } catch (OutOfMemoryException)
            {
                //This shouldnt happen in normal use, since the img gets
                //disposed right away.
                //It only occurs if the image is broken, so in that case just delete the image
                //from cache and continue.
                File.Delete(Image);
            }
        }

        public void Dispose()
        {
            ((IDisposable)wc).Dispose();
        }
    }
}
