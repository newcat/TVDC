using System.Net;
using System.Text.RegularExpressions;
using System.Timers;

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

        private async void FollowerTimer_Elapsed(object sender, ElapsedEventArgs e)
        {

            if (followerWC.IsBusy)
                return;

            string result = "";
            MainWindowVM vm = MainWindowVM.Instance;

            try
            {
                result = await followerWC.DownloadStringTaskAsync("https://api.twitch.tv/kraken/channels/" +
                    Properties.Settings.Default.channel + "/follows?client_id=" + clientID);
            }
            catch (WebException ex)
            {
                vm.chatEntryList_Add(new ChatEntry(ChatEntry.Type.ERROR, "Error while trying to update follower count: " + ex.Message));
                return;
            }

            string followerCount = Regex.Match(Regex.Match(result, "\"_total\":[0-9]*").Value, "[0-9]+").Value;

            if (followerCount != "0,\"")
                vm.FollowerCount = int.Parse(followerCount);

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
