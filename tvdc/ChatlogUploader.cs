﻿using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace tvdc
{
    class ChatlogUploader
    {

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

        private readonly string[] sizes = { "B", "KB", "MB", "GB" };
        
        private WebClientEx wc = new WebClientEx();
        private ProgressWindow progWin;
        private bool showUploadProgress = false;
        
        public ChatlogUploader()
        {
            wc.UploadProgressChanged += Wc_UploadProgressChanged;
        }

        private void Wc_UploadProgressChanged(object sender, UploadProgressChangedEventArgs e)
        {
            if (progWin != null && showUploadProgress)
            {
                progWin.ProgressValue = e.ProgressPercentage;
                progWin.ProgressDescription = string.Format("Uploaded {0} of {1}", toHumanReadable(e.BytesSent), toHumanReadable(e.TotalBytesToSend));
            }
        }

        public void UploadLog(IEnumerable<ChatEntry> chatEntryList, bool includeJoinLeave)
        {

            progWin = new ProgressWindow();
            progWin.Title = "Uploading chatlog";
            progWin.IsIndeterminate = true;

            //build the data
            progWin.Operation = "Building chatlog";
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("v01");

            foreach (ChatEntry ce in chatEntryList)
            {
                switch (ce.EventType)
                {
                    case ChatEntry.Type.CHAT:
                        sb.AppendFormat("C|{0}|{1}|{2}|{3}|{4}", toUnix(ce.Timestamp), Badges.badgeListToString(ce.Badges),
                            ce.Color, ce.Username, ce.OriginalMessage);
                        sb.AppendLine();
                        break;
                    case ChatEntry.Type.IRC:
                        sb.AppendFormat("I|{0}||||{1}", toUnix(ce.Timestamp), ce.Text);
                        sb.AppendLine();
                        break;
                    case ChatEntry.Type.JOIN:
                        if (includeJoinLeave)
                        {
                            sb.AppendFormat("J|{0}|||{1}|", toUnix(ce.Timestamp), ce.Username);
                            sb.AppendLine();
                        }
                        break;
                    case ChatEntry.Type.PART:
                        if (includeJoinLeave)
                        {
                            sb.AppendFormat("P|{0}|||{1}|", toUnix(ce.Timestamp), ce.Username);
                            sb.AppendLine();
                        }
                        break;
                    default:
                        break;
                }
            }

            string data = sb.ToString();

            //Authenticate
            progWin.Operation = "Authenticating";

            var values = new NameValueCollection();

            try
            {
                values["oauth"] = AccountManager.OauthWithoutPrefix;
                var response = wc.UploadValues("http://localhost/tvd/authenticate.php", values);

                if (!Encoding.Default.GetString(response).Contains("Logged in as"))
                {
                    MessageBox.Show("Failed to authenticate: " + Encoding.Default.GetString(response));
                    return;
                }

                progWin.Operation = "Uploading";
                progWin.IsIndeterminate = false;
                showUploadProgress = true;

                values.Clear();
                values["data"] = data;
                values["dataType"] = "chatlog";

                response = wc.UploadValues("http://localhost/tvd/upload.php", values);

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