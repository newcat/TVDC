using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Timers;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using tvdc.Plugin;
using tvdc.Plugin.Models;

namespace loyaltyPlugin
{
    class LoyaltyPlugin : IPlugin
    {

        private IPluginHost host;
        private string oauth;
        private static bool working;
        private bool needAuth = true;
        private int fails = 0;

        Timer timer;
        WebClientEx wc;

        public string PluginName { get { return "LoyaltyPlugin"; } }

        public void Initialize(IPluginHost host) {

            this.host = host;

            User u = host.LoggedInAs();
            if (u != null && host.GetCurrentChannel() == u.Name)
            {

                //request oauth
                oauth = host.RequestOauth(this);
                if (oauth == null)
                    return;

                wc = new WebClientEx();

                timer = new Timer(60000);
                timer.AutoReset = true;
                timer.Elapsed += Timer_Elapsed;
                timer.Start();

            }

        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {

            if (working)
                return;

            if (!working)
            {

                working = true;

                if (fails >= 3)
                {
                    MessageBox.Show("The loyalty plugin will not be available this session: Not able to update.");
                    ((Timer)sender).Stop();
                }

                if (needAuth)
                {
                    if (authenticate())
                    {
                        needAuth = false;
                    }
                    else
                    {
                        MessageBox.Show("The loyalty plugin will not be available this session: Failed to login.");
                        ((Timer)sender).Stop();
                    }
                }

                List<User> users = host.GetChatters();
                StringBuilder sb = new StringBuilder();
                foreach (User u in users)
                {
                    sb.Append(u.Id);
                    sb.Append(',');
                }
                sb.Remove(sb.Length - 1, 1);

                var values = new NameValueCollection();
                values["viewers"] = sb.ToString();

                var response = Encoding.Default.GetString(
                    wc.UploadValues(Properties.Resources.server_base_url + "update_loyalty.php", values));

                if (response.Contains("Success"))
                {
                    fails = 0;
                }
                else if (response == "User not authenticated.")
                {
                    needAuth = true;
                }
                else
                {
                    fails++;
                }

                working = false;

            }

        }

        private bool authenticate()
        {

            var response = wc.DownloadData(
                    Properties.Resources.server_base_url + "authenticate.php?oauth=" + oauth);

            if (!Encoding.Default.GetString(response).Contains("Logged in as"))
                return false;
            else
                return true;

        }

        public ImageSource GetMenuIcon() {
            return BmpToImg(Properties.Resources.loyalty);
        }
        public ImageSource GetMenuIconHover() {
            return BmpToImg(Properties.Resources.loyalty_hover);
        }

        public void IconClicked() {
            Process.Start("https://www.twitch-viewer-display.net/tvd/login/");
        }

        public void End() {
            if (wc != null)
                wc.Dispose();

            if (timer != null)
            {
                timer.Stop();
                timer.Dispose();
            }
        }

        private BitmapImage BmpToImg(System.Drawing.Bitmap bmp)
        {
            MemoryStream ms = new MemoryStream();
            bmp.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
            BitmapImage bi = new BitmapImage();
            bi.BeginInit();
            ms.Seek(0, SeekOrigin.Begin);
            bi.StreamSource = ms;
            bi.EndInit();
            return bi;
        }

        private class WebClientEx : WebClient
        {
            private CookieContainer _cookieContainer = new CookieContainer();

            protected override WebRequest GetWebRequest(Uri address)
            {
                WebRequest request = base.GetWebRequest(address);
                if (request is HttpWebRequest)
                {
                    (request as HttpWebRequest).CookieContainer = _cookieContainer;
                }
                return request;
            }
        }

    }
}
