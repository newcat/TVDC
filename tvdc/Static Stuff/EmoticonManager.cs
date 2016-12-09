using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Text.RegularExpressions;
using System.Net.Http;
using System.Windows;
using System.Net;
using System.Diagnostics;

namespace tvdc
{

    //This class manages the caching and downloading of the emoticons.
    class EmoticonManager
    {

        public static readonly string TempPath = Environment.GetFolderPath(Environment.SpecialFolder.InternetCache) + "\\tvd\\emoticons\\";
        private static Dictionary<int, Emoticon> emoticons = new Dictionary<int, Emoticon>();
        private static Dictionary<string, int> availableEmoticonsForUser = new Dictionary<string, int>();

        public static bool IsCached(int id)
        {
            return File.Exists(TempPath + id.ToString() + ".png");
        }

        public static async Task<bool> Initialize()
        {

            emoticons.Clear();
            availableEmoticonsForUser.Clear();

            if (!Directory.Exists(Environment.GetFolderPath(Environment.SpecialFolder.InternetCache) + "\\tvd"))
                Directory.CreateDirectory(Environment.GetFolderPath(Environment.SpecialFolder.InternetCache) + "\\tvd");

            if (!Directory.Exists(Environment.GetFolderPath(Environment.SpecialFolder.InternetCache) + "\\tvd\\emoticons"))
                Directory.CreateDirectory(Environment.GetFolderPath(Environment.SpecialFolder.InternetCache) + "\\tvd\\emoticons");

            //Read all the Emoticons into the Dictionary
            await Task.Run(() =>
            {
                DirectoryInfo di = new DirectoryInfo(TempPath);

                foreach (FileInfo fi in di.GetFiles("*.png"))
                {
                    int eID = int.Parse(Path.GetFileNameWithoutExtension(fi.Name));
                    emoticons.Add(eID, new Emoticon(eID));
                }
            });

            Debug.WriteLine("Downloading user emoticon list");
            return await downloadUserEmoticonList();

        }

        public static Emoticon RequestEmoticon(int emoteID)
        {

            if (emoticons.ContainsKey(emoteID))
            {
                return emoticons[emoteID];
            } else
            {
                Emoticon e = new Emoticon(emoteID);
                emoticons.Add(emoteID, e);
                return e;
            }

        }

        public static void ClearCache()
        {

            emoticons.Clear();
            DirectoryInfo di = new DirectoryInfo(TempPath);
            foreach (FileInfo f in di.GetFiles())
            {
                try
                {
                    f.Delete();
                } catch (IOException)
                {
                    Debug.WriteLine("Clear Cache: Couldnt delete " + f.Name);
                }
            }

        }

        public static string ParseEmoticons(string text)
        {
            List<EmoticonPositionData> emotePositions = new List<EmoticonPositionData>();
            foreach (string s in availableEmoticonsForUser.Keys)
            {

                //TODO: Regex not perfect yet, e. g. \:-?D as s will also Match 123:D
                MatchCollection mc = Regex.Matches(text, "((?=\\W)|(^|\\b))" + s + "(?=\\W|$|\\b)");

                if (mc.Count > 0)
                {
                    Dictionary<int, int> positions = new Dictionary<int, int>();

                    foreach (Match m in mc)
                    {
                        positions.Add(m.Index, m.Index + m.Length - 1);
                    }

                    emotePositions.Add(new EmoticonPositionData()
                    {
                        id = availableEmoticonsForUser[s],
                        positions = positions
                    });
                }
                    
            }

            string returnString = "";

            for (int i = 0; i < emotePositions.Count; i++)
            {
                if (i > 0)
                    returnString += "/";

                returnString += emotePositions[i].id + ":";
                Dictionary<int, int> pos = emotePositions[i].positions;

                for (int j = 0; j < pos.Count; j++)
                {
                    if (j > 0)
                        returnString += ",";

                    returnString += pos.ElementAt(j).Key.ToString() + "-" + pos.ElementAt(j).Value.ToString();
                }
            }

            return returnString;

        }

        private class EmoticonPositionData
        {
            public int id;
            public Dictionary<int, int> positions;
        }

        private static async Task<bool> downloadUserEmoticonList()
        {

            using (WebClient wc = new WebClient())
            {

                wc.Headers.Add("Client-ID", Properties.Resources.client_id);

                try
                {
                    string json = await wc.DownloadStringTaskAsync(
                        string.Format("https://api.twitch.tv/kraken/users/{0}/emotes?oauth_token={1}",
                        AccountManager.Username, AccountManager.Oauth.Substring(6)));

                    JObject root = JObject.Parse(json);
                    JObject sets = (JObject)root["emoticon_sets"];
                    
                    foreach (JToken set in sets.Children())
                    {
                        foreach (JToken em in set.First)
                        {
                            availableEmoticonsForUser.Add(WebUtility.HtmlDecode((string)em["code"]), (int)em["id"]);
                        }
                    }
                    
                } catch (WebException)
                {
                    return false;
                } catch (InvalidOperationException)
                {
                    return false;
                }

                return true;

            }

        }

    }
}
