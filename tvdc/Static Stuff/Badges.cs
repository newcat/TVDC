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

        public static bool HasSubscriberBadge { get; private set; }
        
        public static BitmapImage Staff { get; private set; }
        public static BitmapImage Admin { get; private set; }
        public static BitmapImage Global_mod { get; private set; }
        public static BitmapImage Moderator { get; private set; }
        public static BitmapImage Turbo { get; private set; }
        public static BitmapImage Broadcaster { get; private set; }
        public static BitmapImage Subscriber { get; private set; }
        public static BitmapImage Premium { get; private set; }

        public enum BadgeTypes
        {
            SUBSCRIBER, TURBO, MODERATOR, BROADCASTER, GLOBAL_MOD, ADMIN, STAFF, PREMIUM
        }

        public static void Init()
        {

            if (loaded)
                return;

            Staff = convert(Properties.Resources.staff_alpha);
            Admin = convert(Properties.Resources.admin_alpha);
            Global_mod = convert(Properties.Resources.globalmod_alpha);
            Moderator = convert(Properties.Resources.mod_alpha);
            Turbo = convert(Properties.Resources.turbo_alpha);
            Broadcaster = convert(Properties.Resources.broadcaster_alpha);
            Premium = convert(Properties.Resources.premium);

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

        public static async Task<bool> DownloadSubBadge(string channel)
        {
            WebClient wc = new WebClient();
            wc.Encoding = System.Text.Encoding.UTF8;
            wc.Headers.Add("Client-ID", Properties.Resources.client_id);

            string json;

            try
            {
                json = await wc.DownloadStringTaskAsync(string.Format("https://api.twitch.tv/kraken/chat/{0}/badges", channel));
            } catch (WebException)
            {
                HasSubscriberBadge = false;
                return false;
            }

            JObject mainJO = JObject.Parse(json);

            if (!mainJO["subscriber"].HasValues)
            {
                HasSubscriberBadge = false;
                return true;
            }

            string subBadgeURL = (string)mainJO["subscriber"]["image"];

            byte[] imgData;

            try
            {
                imgData = await wc.DownloadDataTaskAsync(subBadgeURL);
            } catch (WebException)
            {
                HasSubscriberBadge = false;
                return false;
            }

            MemoryStream ms = new MemoryStream(imgData);
            ms.Position = 0;

            Subscriber = new BitmapImage();
            Subscriber.BeginInit();
            Subscriber.StreamSource = ms;
            Subscriber.EndInit();

            wc.Dispose();
            imgData = null;

            HasSubscriberBadge = true;
            return true;
        }

        public static string BadgeListToString(List<BadgeTypes> badges)
        {
            string returnString = "";

            foreach (BadgeTypes b in badges)
            {
                returnString += Enum.GetName(typeof(BadgeTypes), b).ToLower() + ",";
            }

            return returnString.TrimEnd(',');
        }

        public static List<BadgeTypes> ParseBadgeString(string bs)
        {

            List<BadgeTypes> returnList = new List<BadgeTypes>();

            if (bs.Contains("moderator"))
                returnList.Add(BadgeTypes.MODERATOR);

            if (bs.Contains("subscriber"))
                returnList.Add(BadgeTypes.SUBSCRIBER);

            if (bs.Contains("premium"))
                returnList.Add(BadgeTypes.PREMIUM);

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
