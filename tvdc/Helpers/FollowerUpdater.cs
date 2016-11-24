using System.Net;
using System.Text.RegularExpressions;
using System.Timers;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using System;

namespace tvdc
{
    class FollowerUpdater
    {

        private WebClient followerWC;
        private Timer followerTimer = new Timer(30000);

        private readonly string clientID = Properties.Resources.client_id;

        public FollowerUpdater()
        {
            followerWC = new WebClient();
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

                if (root["stream"].HasValues)
                {
                    vm.OverriddenViewerCount = (int)root["stream"]["viewers"];
                    vm.OverrideViewerCount = true;
                } else
                {
                    vm.OverrideViewerCount = false;
                }
            } catch (Exception)
            {
                Debug.WriteLine("Exception at viewer count retriever.");
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

    }
}
