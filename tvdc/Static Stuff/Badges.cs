using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Net;
using Newtonsoft.Json.Linq;

namespace tvdc
{
    public class Badges
    {

        private static bool loaded = false;

        public static bool hasSubscriberBadge { get; private set; }
        
        public static BitmapImage staff { get; private set; }
        public static BitmapImage admin { get; private set; }
        public static BitmapImage global_mod { get; private set; }
        public static BitmapImage moderator { get; private set; }
        public static BitmapImage turbo { get; private set; }
        public static BitmapImage broadcaster { get; private set; }
        public static BitmapImage subscriber { get; private set; }

        public enum BadgeTypes
        {
            SUBSCRIBER, TURBO, MODERATOR, BROADCASTER, GLOBAL_MOD, ADMIN, STAFF
        }

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

        public static async Task<bool> downloadSubBadge(string channel)
        {
            WebClient wc = new WebClient();
            wc.Headers.Add("Client-ID", Properties.Resources.client_id);

            string json;

            try
            {
                json = await wc.DownloadStringTaskAsync(string.Format("https://api.twitch.tv/kraken/chat/{0}/badges", channel));
            } catch (WebException)
            {
                hasSubscriberBadge = false;
                return false;
            }

            JObject mainJO = JObject.Parse(json);

            if (!mainJO["subscriber"].HasValues)
            {
                hasSubscriberBadge = false;
                return true;
            }

            string subBadgeURL = (string)mainJO["subscriber"]["image"];

            byte[] imgData;

            try
            {
                imgData = await wc.DownloadDataTaskAsync(subBadgeURL);
            } catch (WebException)
            {
                hasSubscriberBadge = false;
                return false;
            }

            MemoryStream ms = new MemoryStream(imgData);
            ms.Position = 0;

            subscriber = new BitmapImage();
            subscriber.BeginInit();
            subscriber.StreamSource = ms;
            subscriber.EndInit();

            wc.Dispose();
            imgData = null;

            hasSubscriberBadge = true;
            return true;
        }

        public static string badgeListToString(List<BadgeTypes> badges)
        {
            string returnString = "";

            foreach (BadgeTypes b in badges)
            {
                returnString += Enum.GetName(typeof(BadgeTypes), b).ToLower();
            }

            return returnString;
        }

        public static List<BadgeTypes> parseBadgeString(string bs)
        {

            List<BadgeTypes> returnList = new List<BadgeTypes>();

            if (bs.Contains("moderator"))
                returnList.Add(BadgeTypes.MODERATOR);

            if (bs.Contains("subscriber"))
                returnList.Add(BadgeTypes.SUBSCRIBER);

            if (bs.Contains("turbo"))
                returnList.Add(BadgeTypes.TURBO);

            if (bs.Contains("staff"))
                returnList.Add(BadgeTypes.STAFF);

            if (bs.Contains("admin"))
                returnList.Add(BadgeTypes.ADMIN);

            if (bs.Contains("broadcaster"))
                returnList.Add(BadgeTypes.BROADCASTER);

            if (bs.Contains("global_mod"))
                returnList.Add(BadgeTypes.GLOBAL_MOD);

            return returnList;

        }

    }
}
