using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Net;
using Newtonsoft.Json.Linq;

namespace tvdc
{
    class Badges
    {

        private static bool loaded = false;

        public static bool hasSubscriberBadge { get; private set; }
        
        public static BitmapImage staff;
        public static BitmapImage admin;
        public static BitmapImage global_mod;
        public static BitmapImage moderator;
        public static BitmapImage subscriber;
        public static BitmapImage turbo;
        public static BitmapImage broadcaster;

        public static void init()
        {

            if (loaded)
                return;

            staff = convert(Properties.Resources.staff_alpha);
            admin = convert(Properties.Resources.admin_alpha);
            global_mod = convert(Properties.Resources.globalmod_alpha);
            moderator = convert(Properties.Resources.mod_alpha);
            turbo = convert(Properties.Resources.turbo_alpha);
            broadcaster = convert(Properties.Resources.broadcaster_alpha);

            loaded = true;

        }

        private static BitmapImage convert(Bitmap b)
        {

            MemoryStream ms = new MemoryStream();
            b.Save(ms, ImageFormat.Png);
            ms.Position = 0;
            BitmapImage bi = new BitmapImage();
            bi.BeginInit();
            bi.StreamSource = ms;
            bi.EndInit();
            //ms.Dispose(); -> Unfortunately it needs to stay in memory to work
            return bi;

        }

        public static async Task downloadSubBadge(string channel)
        {
            WebClient wc = new WebClient();
            wc.Headers.Add("Client-ID", Properties.Resources.client_id);

            string json = await wc.DownloadStringTaskAsync(string.Format("https://api.twitch.tv/kraken/chat/{0}/badges", channel));
            JObject mainJO = JObject.Parse(json);

            if (!mainJO["subscriber"].HasValues)
            {
                hasSubscriberBadge = false;
                return;
            }

            string subBadgeURL = (string)mainJO["subscriber"]["image"];

            byte[] imgData = await wc.DownloadDataTaskAsync(subBadgeURL);
            MemoryStream ms = new MemoryStream(imgData);
            ms.Position = 0;

            subscriber = new BitmapImage();
            subscriber.BeginInit();
            subscriber.StreamSource = ms;
            subscriber.EndInit();

            wc.Dispose();
            imgData = null;

            hasSubscriberBadge = true;
        }

    }
}
