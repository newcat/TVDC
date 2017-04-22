using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace tvdc
{
    class ChatlogUploader : IDisposable
    {

        //============== THIS IS THE OLD FORMAT (DEPRECATED) ==============
        //Chatlog format:
        //Start of file: v01
        //For each chat event (fields separated by | (chr(124))):
            //1 char: Messagetype (C(hat), I(RC), J(OIN), P(ART))
            //Timestamp in unix format
            //Badge list (optional, can be empty)
            //Color in hex (optional, can be empty)
            //Username
            //Text
            //CR LF

        //============== NEW FORMAT ===================
        // see example_chatlog.clog

        private readonly string[] sizes = { "B", "KB", "MB", "GB" };
        
        private WebClientEx wc = new WebClientEx();
        private ProgressWindow progWin;
        private bool showUploadProgress = false;
        
        public ChatlogUploader()
        {
            wc.UploadProgressChanged += Wc_UploadProgressChanged;
            wc.Encoding = Encoding.UTF8;
        }

        private void Wc_UploadProgressChanged(object sender, UploadProgressChangedEventArgs e)
        {
            if (progWin != null && showUploadProgress)
            {
                progWin.ProgressValue = (int)(100 * e.BytesSent / (double)e.TotalBytesToSend);
                progWin.ProgressDescription = string.Format("Uploaded {0} of {1}", toHumanReadable(e.BytesSent), toHumanReadable(e.TotalBytesToSend));
            }
        }

        public async Task UploadLog(IEnumerable<ChatEntry> chatEntryList, IList<Graph.HistoryStop> history, bool includeJoinLeave)
        {

            if (Properties.Settings.Default.uploadChatlog == 0 ||
                (Properties.Settings.Default.uploadChatlog == 1 &&
                MessageBox.Show("Do you want to upload this chatlog?", "Upload chatlog", MessageBoxButton.YesNo) == MessageBoxResult.No)) {
                return;
            }

            progWin = new ProgressWindow();
            progWin.Title = "Uploading chatlog";
            progWin.IsIndeterminate = true;
            progWin.Show();

            //build the data
            progWin.Operation = "Building chatlog";

            //TODO: Timestamp
            long timestamp = 0;
            if (history.Count > 0)
                timestamp = toUnix(history[0].timestamp);

            JArray viewerCount = new JArray();
            JArray chatFreq = new JArray();
            foreach (Graph.HistoryStop hs in history)
            {
                viewerCount.Add(hs.count);
                chatFreq.Add(hs.freq);
            }

            JArray chat = new JArray();
            foreach (ChatEntry ce in chatEntryList)
            {
                switch (ce.EventType)
                {
                    case ChatEntry.Type.CHAT:
                        chat.Add(new JObject(
                            new JProperty("type", ce.IsAction ? "me" : "chat"),
                            new JProperty("timestamp", toUnix(ce.Timestamp)),
                            new JProperty("badges", Badges.BadgeListToString(ce.Badges)),
                            new JProperty("color", ce.Color),
                            new JProperty("username", ce.Username),
                            new JProperty("message", buildMessage(ce.Paragraphs))
                        ));
                        break;
                    case ChatEntry.Type.IRC:
                        chat.Add(new JObject(
                            new JProperty("type", "irc"),
                            new JProperty("timestamp", toUnix(ce.Timestamp)),
                            new JProperty("message", ce.Text)
                        ));
                        break;
                    case ChatEntry.Type.JOIN:
                        chat.Add(new JObject(
                            new JProperty("type", "join"),
                            new JProperty("timestamp", toUnix(ce.Timestamp)),
                            new JProperty("user", ce.Username)
                        ));
                        break;
                    case ChatEntry.Type.PART:
                        chat.Add(new JObject(
                            new JProperty("type", "part"),
                            new JProperty("timestamp", toUnix(ce.Timestamp)),
                            new JProperty("user", ce.Username)
                        ));
                        break;
                }
            }


            JObject root = new JObject(
                new JProperty("version", 2),
                new JProperty("channel", Properties.Settings.Default.channel),
                new JProperty("timestamp", timestamp), //TODO
                new JProperty("viewerCount", viewerCount),
                new JProperty("chatFreq", chatFreq),
                new JProperty("chat", chat)
            );

            string data = root.ToString();

            //Authenticate
            progWin.Operation = "Authenticating";            

            try
            {

                var response = await wc.DownloadDataTaskAsync(
                    Properties.Resources.server_base_url + "authenticate.php?oauth=" + AccountManager.OauthWithoutPrefix);

                if (!Encoding.Default.GetString(response).Contains("Logged in as"))
                {
                    MessageBox.Show("Failed to authenticate: " + Encoding.Default.GetString(response));
                    return;
                }

                progWin.Operation = "Uploading";
                progWin.IsIndeterminate = false;
                showUploadProgress = true;

                var values = new NameValueCollection();
                values["data"] = data;
                values["dataType"] = "chatlog";
                values["channel"] = Properties.Settings.Default.channel;

                response = await wc.UploadValuesTaskAsync(Properties.Resources.server_base_url + "upload.php", values);

                if (Encoding.Default.GetString(response) != "Upload completed.")
                {
                    MessageBox.Show("Upload failed: " + Encoding.Default.GetString(response));
                }

            } catch (WebException ex)
            {
                MessageBox.Show("WebException: " + ex.Message);
            } finally
            {
                wc.Dispose();
                progWin.Close();
            }

        }

        private long toUnix(DateTime dt)
        {
            return (long)dt.ToUniversalTime().Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
        }

        private string toHumanReadable(long bytes)
        {
            int order = 0;
            while (bytes >= 1024 && ++order < sizes.Length)
            {
                bytes /= 1024;
            }
            return string.Format("{0:0.##} {1}", bytes, sizes[order]);
        }

        private string buildMessage(List<Paragraph> paragraphs)
        {

            StringBuilder sb = new StringBuilder();

            foreach (Paragraph p in paragraphs)
            {

                if (p.IsImage)
                {
                    sb.Append("\u0001");
                    sb.Append(p.EmoteID);
                    sb.Append("\u0003");
                } else if (p.IsURL)
                {
                    sb.Append("\u0002");
                    sb.Append(p.Text);
                    sb.Append("\u0003");
                } else
                {
                    sb.Append(p.Text);
                }
            }

            return sb.ToString();

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

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    if (wc != null) wc.Dispose();
                }

                progWin = null;
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
        #endregion

    }
}
