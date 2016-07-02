using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Web.Script.Serialization;
using System.Collections;
using System.Text.RegularExpressions;
using System.Net.Http;
using System.Windows;

namespace tvdc
{

    //This class manages the caching and downloading of the emoticons.
    class EmoticonManager
    {

        public static string tempPath = Environment.GetFolderPath(Environment.SpecialFolder.InternetCache) + "\\tvd\\emoticons\\";
        private static Dictionary<int, Emoticon> emoticons = new Dictionary<int, Emoticon>();

        public static bool isCached(int id)
        {
            return File.Exists(tempPath + id.ToString() + ".png");
        }

        public static void initialize()
        {

            emoticons.Clear();

            if (!Directory.Exists(Environment.GetFolderPath(Environment.SpecialFolder.InternetCache) + "\\tvd"))
                Directory.CreateDirectory(Environment.GetFolderPath(Environment.SpecialFolder.InternetCache) + "\\tvd");

            if (!Directory.Exists(Environment.GetFolderPath(Environment.SpecialFolder.InternetCache) + "\\tvd\\emoticons"))
                Directory.CreateDirectory(Environment.GetFolderPath(Environment.SpecialFolder.InternetCache) + "\\tvd\\emoticons");

            //Read all the Emoticons into the Dictionary
            DirectoryInfo di = new DirectoryInfo(tempPath);

            foreach (FileInfo fi in di.GetFiles("*.png"))
            {
                int eID = int.Parse(Path.GetFileNameWithoutExtension(fi.Name));
                emoticons.Add(eID, new Emoticon(eID));
            }

        }

        public static Emoticon requestEmoticon(int emoteID)
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

        public static void clearCache()
        {

            emoticons.Clear();
            DirectoryInfo di = new DirectoryInfo(tempPath);
            foreach (FileInfo f in di.GetFiles())
            {
                f.Delete();
            }

        }

    }
}
