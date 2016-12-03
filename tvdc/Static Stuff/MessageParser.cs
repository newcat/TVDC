using System;
using System.Collections;
using System.Collections.Generic;

namespace tvdc
{
    public class MessageParser
    {

        public static List<Paragraph> GetParagraphsFromMessage(string message)
        {
            string emotes = EmoticonManager.ParseEmoticons(message);
            Dictionary<string, string> tags = new Dictionary<string, string>();
            tags.Add("emotes", emotes);
            tags.Add("text", message);
            return GetParagraphsFromTags(tags);
        }

        public static List<Paragraph> GetParagraphsFromTags(Dictionary<string, string> tags)
        {

            List<Paragraph> p = new List<Paragraph>();

            if (tags.Count == 1 && tags.ContainsKey("text") && tags["text"] != null)
            {
                p.Add(new Paragraph(tags["text"]));
                return p;
            }

            bool isAction = false;
            if (tags.ContainsKey("text") && tags["text"] != null && tags["text"].StartsWith("\u0001ACTION"))
            {
                isAction = true;
                tags["text"] = tags["text"].Substring(8);
            }

            if (!tags.ContainsKey("emotes"))
            {
                p.Add(new Paragraph(tags["text"]));
                return p;
            }

            ArrayList paragraphs = new ArrayList();
            List<EmoticonPosition> positions = new List<EmoticonPosition>();
            string emoticons = tags["emotes"];

            if (emoticons.Length > 0)
            {
                int i = -1;
                while (i < emoticons.Length)
                {

                    i++;
                    string emoteIDString = "";
                    while (emoticons[i] != ':')
                    {
                        emoteIDString += emoticons[i];
                        i++;
                    }
                    int emoteID = int.Parse(emoteIDString);

                    bool exit = false;
                    while (!exit)
                    {

                        i++;
                        string startPosString = "";
                        while (emoticons[i] != '-')
                        {
                            startPosString += emoticons[i];
                            i++;
                        }
                        int startPos = int.Parse(startPosString);

                        i++;
                        string endPosString = "";
                        while (i < emoticons.Length && !(emoticons[i] == ',' || emoticons[i] == '/'))
                        {
                            endPosString += emoticons[i];
                            i++;
                        }
                        int endPos = int.Parse(endPosString);

                        positions.Add(new EmoticonPosition(emoteID, startPos, endPos));

                        if (i >= emoticons.Length || emoticons[i] == '/')
                            exit = true;

                    }

                }

                positions.Sort();

                i = 0;

                foreach (EmoticonPosition ep in positions)
                {
                    if (i != ep.startIndex)
                        paragraphs.Add(tags["text"].Substring(i, ep.startIndex - i));
                    paragraphs.Add(ep.emoteID);
                    i = ep.endIndex + 1;
                }

                if (positions[positions.Count - 1].endIndex < tags["text"].Length - 1)
                {
                    paragraphs.Add(tags["text"].Substring(positions[positions.Count - 1].endIndex + 1));
                }

            }
            else
            {
                paragraphs.Add(tags["text"]);
            }

            foreach (object pg in paragraphs)
            {
                if (pg is int)
                {
                    p.Add(new Paragraph((int)pg));
                }
                else
                {

                    List<TemporaryParagraph> localParagraphs = new List<TemporaryParagraph>();
                    string[] wordList = (pg as string).Split(' ');
                    string s = "";

                    foreach (string word in wordList)
                    {
                        if (isUrl(word))
                        {
                            if (s != " ")
                            {
                                localParagraphs.Add(new TemporaryParagraph() { type = 0, text = s });
                                s = " ";
                            }
                            localParagraphs.Add(new TemporaryParagraph() { type = 0, text = word });
                        }
                        else
                        {
                            s += word + " ";
                        }
                    }

                    localParagraphs.Add(new TemporaryParagraph() { type = 0, text = s });

                    foreach (TemporaryParagraph localPG in localParagraphs)
                    {
                        p.Add(new Paragraph(localPG.text, isAction, isUrl(localPG.text)));
                    }

                }
            }

            return p;

        }

        private static bool isUrl(string s)
        {
            Uri uriResult;
            if (s.StartsWith("www."))
                return Uri.TryCreate("http://" + s, UriKind.Absolute, out uriResult) && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
            return Uri.TryCreate(s, UriKind.Absolute, out uriResult) && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
        }

        private class TemporaryParagraph
        {
            public int type { get; set; } //0 = Text, 1 = Emoticon
            public string text { get; set; }
        }

        private class EmoticonPosition : IComparable
        {
            public int emoteID { get; private set; }
            public int startIndex { get; private set; }
            public int endIndex { get; private set; }

            public EmoticonPosition(int emoteID, int startIndex, int endIndex)
            {
                this.emoteID = emoteID;
                this.startIndex = startIndex;
                this.endIndex = endIndex;
            }

            public int CompareTo(object obj)
            {
                if (obj == null) return 1;

                EmoticonPosition otherPos = obj as EmoticonPosition;

                if (otherPos != null)
                {
                    return startIndex.CompareTo(otherPos.startIndex);
                }
                else
                {
                    throw new ArgumentException("Object is not an emoticon position");
                }

            }
        }

    }
}
