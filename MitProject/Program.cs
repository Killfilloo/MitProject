using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Collections;
using System.Linq;

namespace MitProject
{
    class Program
    {
        static Video vid = new Video();

        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;
            string[] https = { "https://youtu.be/g0e4dyA3f24", "https://www.youtube.com/watch?v=YGXS4uEdT3Q", "https://www.youtube.com/watch?v=iFGve5MUUnE", "https://www.youtube.com/watch?v=7W0IFAGyqwo", "https://www.youtube.com/watch?v=7W0ISI3yqwo", "https://www.youtube.com/watch?v=7NOPE" };
            List<Video> videos = new List<Video>();

            for (int i = 0; i < 5; i++)
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(https[i]);
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                StreamReader sr = new StreamReader(response.GetResponseStream());
                string allLines = sr.ReadToEnd();
                int id = videos.Count;
                if (VideoExists(allLines)) { videos.Add(CreateVideo(allLines,id)); }
            }
            Console.Clear();
            /*
            Console.WriteLine(videos[0].Id);
            Console.WriteLine(videos[0].Title);
            Console.WriteLine(videos[0].Description);
            Console.WriteLine(videos[0].Views);
            Console.WriteLine(videos[0].Imgpath);
            Console.WriteLine(videos[0].Keywords);
            Console.ReadKey(true);
            */
            foreach (Video v in videos)
            {
                DisplayVideoData(videos[v.Id]);
            }
            Console.ReadKey(true);
        }

        static bool VideoExists(string s)
        {
            if (s != null)
            {
                try
                {
                    if (!s.Contains("eow-title")) { return false; }
                    else return true;
                }
                catch
                {
                    Console.WriteLine("Error encountered.");
                }
                Console.WriteLine("Video found");
            }
            else
            {
                Console.WriteLine("Video not found");
                return false;
            }
            return false;
        }

        static Video CreateVideo(string s, int id)
        {
            /* 
             CreateVideo can create a Video object based of the string s (data fetched in main) and int id (based on the list).

             OBS: The output is formatted to remove extra whitespace/html code leftovers and fix escape characters!
            */

            Video create = new Video();
            create.Id = id;
            string[] allLines = s.Split(' ');
            for (int i = 0; i < allLines.Length; i++)
            {
                if (allLines[i].Contains("thumbnailUrl"))
                {
                    create.Imgpath = allLines[i+1];
                    create.Imgpath = FormatVideoData(create, "thumbnail");
                }
                else if (allLines[i].Contains("eow-title"))
                {
                    string path = "";
                    for (int j = i; j < allLines.Length; j++) {
                        if (allLines[j].Contains("\">")) { i = j; break; }
                    }
                    for (int j = i+1; j < allLines.Length; j++) {
                        if (!allLines[j].Contains("</span")) { path += allLines[j] + " "; }
                        else { create.Title = path; break; }
                    }
                    create.Title = FormatVideoData(create,"title");
                }

                else if (allLines[i].Contains("eow-description"))
                {
                    string desc = "";
                    for (int j = i; j < allLines.Length; j++)
                    {
                        if (allLines[j].Contains(">")) { i = j; break; }
                    }
                    for (int j = i; j < allLines.Length; j++)
                    {
                        if (!allLines[j].Contains("</p>")) { desc += allLines[j] + " "; }
                        else {
                            string[] ending = allLines[j].Split('<');
                            desc += ending[0];
                            create.Description = desc;
                            break; }
                    }

                    create.Description = FormatVideoData(create, "description");
                }

                else if (allLines[i].Contains("keywords\\\":"))
                {
                    string newTags = "";
                    string[] allTags = allLines[i].Split('\\');
                    for (int j = 8; j < allTags.Length-10; j+=2)
                    {
                        newTags+= allTags[j];
                    }
                    string[] tags = newTags.Split('"');
                    for(int k = 0; k < tags.Length; k++)
                    { create.Keywords += tags[k]+" "; }

                    create.Keywords = FormatVideoData(create, "keywords");
                }

                else if (allLines[i].Contains("watch-view-count"))
                {
                    string[] allS = allLines[i].Split('"');
                    create.Views = allS[allS.Length - 1];
                    create.Views = FormatVideoData(create, "views");
                }

            }
            return create;
        }

        static string FormatVideoData(Video vid, string type) {
            if (type == "title") {
                // common issues &quot; &#39; &amp;
                //   How to skip &quot;The Fallen Protectors&quot; @ Siege of Orgrimmar - WoW
                string title = vid.Title.Substring(3,vid.Title.Length-3);
                title = title.Replace("&quot;", "\"");
                title = title.Replace("&#39;", "\'");
                title = title.Replace("&amp;", "&");
                return title;
            }
            if (type == "description") {
                string description = vid.Description.Substring(1, vid.Description.Length - 1);
                description = description.Replace("<br />", "\n");
                //description = description.Replace("<a href=", "");
                int start = 0, stop = 0;
                string link = ""; int linkloc = 0;
                string[] de = description.Split(' ');
                var d = de.ToList();

                for (int i = 0; i < d.Count; i++)
                {

                    if (d[i].Contains("<a")) { start = i; }
                    if (d[i].Contains("</a>")) { stop = i; try { d[i] = d[i].Substring(d[i].IndexOf("</a>") + 5, d[i].Length - d[i].IndexOf("</a>") - 5); } catch { } }
                    if (d[i].Contains("watch?v")&&!d[i].Contains("youtu")) { link = "https://www.youtube.com" + d[i].Substring(6,d[i].Length-7)+"\n"; linkloc = i; }
                    if (start != 0 && stop != 0)
                    {
                        for (int j = start; j < stop; j++)
                        {
                            d[j] = "";
                        }
                        d[linkloc] = link;
                        start = 0; stop = 0;
                        linkloc = 0; link = "";
                    }
                }
                description = "";
                foreach(string s in d)
                {
                    description +=s+" ";
                }
                string[] result = description.Split(' ');
                description = "";
                foreach (string s in result)
                {
                    description += s + " ";
                }
                return description;
            }
            if (type == "keywords") {
                string tags = vid.Keywords.Substring(1, vid.Keywords.Length-1);
                return tags;
            }
            if (type == "views") {
                string views = vid.Views.Substring(1, vid.Views.Length - 1);
                return views;
            }
            if (type == "thumbnail") {
                string thumbnail = vid.Imgpath.Substring(6,vid.Imgpath.Length-9);
                return thumbnail;
            }
            return "error occured";
        }

        static void DisplayVideoData(Video vid)
        {
            int x = 5, y = 3, dsc = 17;
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.White;
            Console.Clear();

            print(x, y, "Thumbnail:\n     "+vid.Imgpath);
            print(x, y + 7, "Title:\n     " + vid.Title);
            print(x, y+10, "Views:\n     " + vid.Views);

            int dsclen = vid.Description.Length;
            if(dsclen > 50){print(x, y + 13, "Description:\n     " + vid.Description.Substring(0, 50)+" ...");
                print(x, y + 15, "press tab to view more description");
            }
            else{print(x, y + 13, vid.Description);
            }

            int keylen = vid.Keywords.Length;
            if (keylen > 40) { print(x, y+dsc, "Tags:\n     " + vid.Keywords.Substring(0, 40) + " ...");
                print(x, y + dsc + 2, "press space to view more tags");
            }
            else {
                if (vid.Keywords.Length == 0) { print(x, y + dsc, "Tags:\n     None"); }
                else { print(x, y + dsc, vid.Keywords); }
            }

            ConsoleKeyInfo info = Console.ReadKey(true);
            string input = Convert.ToString(info.KeyChar).ToUpper();
            bool run = true;

            while (info.Key == ConsoleKey.Tab || input == " " || run || info.Key == ConsoleKey.Escape)
            {
                if (run == false) { 
                    info = Console.ReadKey(true);
                    input = Convert.ToString(info.KeyChar).ToUpper();
                } run = false;

                int descLines = 1;
                if (info.Key == ConsoleKey.Tab && vid.Description.Length > 0) //if tab
                {
                    Console.Clear();
                    print(x, y, "Thumbnail:\n     " + vid.Imgpath);
                    print(x, y + 7, "Title:\n     " + vid.Title);
                    print(x, y + 10, "Views:\n     " + vid.Views);
                    int descLength = 0;
                    print(x, y + 13, "Description:");
                    Console.Write("     ");
                    string[] extend_desc = vid.Description.Split(' ');
                    for (int i = 0; i < extend_desc.Length; i++)
                    {
                        descLength += extend_desc[i].Length;
                        if (descLength < 50) { Console.Write(extend_desc[i]+ " "); }
                        else
                        {
                            descLength = 0; Console.Write("\n     "+extend_desc[i]+" ");
                            descLength += extend_desc[i].Length; descLines++;
                        }
                    }
                    print(x, y + dsc + descLines-3 , "press ESCAPE");
                    if (descLines > 0) descLines--;
                    if (keylen > 40) { print(x, y + dsc+ descLines, "Tags:\n     " + vid.Keywords.Substring(0, 40) + " ...");
                        print(x, y + dsc + descLines + 1, "press space to view more tags");
                    }
                    else
                    {
                        if (vid.Keywords.Length == 0) { Console.Write("Tags:\n     None"); }
                        else { print(x, y + dsc, vid.Keywords); }
                    }
                }

                if (input == " " && vid.Keywords.Length>0) //if space
                {
                    Console.Clear();
                    print(x, y, "Thumbnail:\n     " + vid.Imgpath);
                    print(x, y + 7, "Title:\n     " + vid.Title);
                    print(x, y + 10, "Views:\n     " + vid.Views);
                    if (dsclen > 50) { print(x, y + 13, "Description:\n     " + vid.Description.Substring(0, 50) + " ...");
                        print(x, y + 15, "press tab to view more description");
                    }
                    else { print(x, y + 13, "Description:\n     " + vid.Description);
                    }
                    print(x, y + dsc, "Tags:");
                    Console.Write("     ");
                    string[] tags = vid.Keywords.Split(' ');
                    for (int i = 0; i < tags.Length; i++)
                    {
                        Console.Write(tags[i] + " ");
                        if (i % 8 == 0 && i != 0) { Console.Write("\n     "); }
                    }
                    Console.Write("                                                                             ");
                }

                if (info.Key == ConsoleKey.Escape) //if escape 
                {
                    Console.Clear();
                    print(x, y, "Thumbnail:\n     " + vid.Imgpath);
                    print(x, y + 7, "Title:\n     " + vid.Title);
                    print(x, y + 10, "Views:\n     " + vid.Views);

                    dsclen = vid.Description.Length;
                    if (dsclen > 50)
                    {
                        print(x, y + 13, "Description:\n     " + vid.Description.Substring(0, 50) + " ...");
                        print(x, y + 15, "press tab to view more description");
                    }
                    else
                    {
                        print(x, y + 13, vid.Description);
                    }

                    keylen = vid.Keywords.Length;
                    if (keylen > 40)
                    {
                        print(x, y + dsc, "Tags:\n     " + vid.Keywords.Substring(0, 40) + " ...");
                        print(x, y + dsc + 2, "press space to view more tags");
                    }
                    else
                    {
                        if (vid.Keywords.Length == 0) { print(x, y + dsc, "Tags:\n     None"); }
                        else { print(x, y + dsc, vid.Keywords); }
                    }
                }
            }
        }

        static void print(int x, int y, string s)
        {
            Console.SetCursorPosition(x, y); Console.WriteLine(s);
        }
    }

    class Video
    {
        int id;
        string title;
        string description;
        string keywords;
        string views;
        string likes;
        string imgpath;

        public Video() { }

        public Video(int id, string title, string description, string keywords, string views, string likes, string imgpath)
        {
            this.Id = id;
            this.Title = title;
            this.Description = description;
            this.Keywords = keywords;
            this.Views = views;
            this.Likes = likes;
            this.Imgpath = imgpath;
        }

        public Video(string title, string description, string keywords, string views, string likes, string imgpath)
        {
            this.Id = Id1;
            this.Title = title;
            this.Description = description;
            this.Keywords = keywords;
            this.Views = views;
            this.Likes = likes;
            this.Imgpath = imgpath;
        }

        public int Id { get => Id1; set => Id1 = value; }
        public string Title { get => title; set => title = value; }
        public string Description { get => description; set => description = value; }
        public string Keywords { get => keywords; set => keywords = value; }
        public string Views { get => views; set => views = value; }
        public string Likes { get => likes; set => likes = value; }
        public string Imgpath { get => imgpath; set => imgpath = value; }
        public int Id1 { get => id; set => id = value; }
    }
}