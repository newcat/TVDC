using System.Net;
using System.Text.RegularExpressions;
using System.Timers;

using Newtonsoft.Json.Linq;
using System.Diagnostics;
using System;

namespace tvdc
{
    class FollowerUpdater : IDisposable
    {

        private WebClient followerWC;
        private Timer followerTimer = new Timer(30000);
        private int failedTries = 0;

        private readonly string clientID = Properties.Resources.client_id;

        public FollowerUpdater()
        {
            followerWC = new WebClient();
            followerWC.Encoding = System.Text.Encoding.UTF8;
            followerTimer.Elapsed += FollowerTimer_Elapsed;
            followerTimer.Start();
        }

        public void RunOnce()
        {
            FollowerTimer_Elapsed(null, null);
        }

        private async void FollowerTimer_Elapsed(object sender, ElapsedEventArgs e)
        {

            if (followerWC.IsBusy)
                return;

            string resultFollower = "";
            string resultViewer = "";
            MainWindowVM vm = MainWindowVM.Instance;

            try
            {
                resultFollower = await followerWC.DownloadStringTaskAsync("https://api.twitch.tv/kraken/channels/" +
                    Properties.Settings.Default.channel + "/follows?client_id=" + clientID);

                resultViewer = await followerWC.DownloadStringTaskAsync(string.Format(
                    "https://api.twitch.tv/kraken/streams/{0}?client_id={1}", Properties.Settings.Default.channel, clientID));

            }
            catch (WebException ex)
            {
                vm.chatEntryList_Add(new ChatEntry(ChatEntry.Type.ERROR, "Error while trying to update follower count: " + ex.Message));
                return;
            }

            string followerCount = Regex.Match(Regex.Match(resultFollower, "\"_total\":[0-9]*").Value, "[0-9]+").Value;

            if (followerCount != "0,\"")
                vm.FollowerCount = int.Parse(followerCount);

            try
            {
                JObject root = JObject.Parse(resultViewer);

                if (root["stream"].HasValues && (int)root["stream"]["viewers"] > vm.ViewerCount)
                {
                    vm.OverriddenViewerCount = (int)root["stream"]["viewers"];
                    vm.OverrideViewerCount = true;
                    failedTries = 0;
                } else
                {
                    if (vm.OverrideViewerCount && failedTries >= 3)
                    {
                        vm.OverrideViewerCount = false;
                    } else
                    {
                        failedTries++;
                    }
                }
            } catch (Exception)
            {
                Debug.WriteLine("Exception at viewer count retriever.");
                vm.OverrideViewerCount = false;
            }

        }

        public void Start()
        {
            followerTimer.Start();
        }

        public void Stop()
        {
            followerTimer.Stop();
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {

                    if (followerTimer != null)
                        followerTimer.Dispose();

                    if (followerWC != null)
                        followerWC.Dispose();

                }

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
