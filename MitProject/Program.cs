using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Collections;
using System.Linq;
using System.Threading;
using System.Diagnostics;
using HtmlAgilityPack;
using VideoLibrary;
using YoutubeExtractor;

namespace MitProject
{
    class Program
    {
        static Video vid = new Video();

        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;
            List<string> https = new List<string>(); // https is a list of url strings.
            List<string> hist = new List<string>(); // hist is a list of url strings.
            List<Video> videos = new List<Video>(); // vidoes is the list of video objects, containing all of the information fetched.
            List<string> menu = new List<string> { "\n\n", "  1. Add Url(s)", "  2. Remove Url(s)", "  3. Display Video Data", "  4. Load Url(s) from urls.dat", "  5. 10 last urls","ESC. Exit" }; //menu categories
            using (StreamWriter w = File.AppendText("urls.dat")); //Creates the urls.dat file to read from, if file do not exist.
            using (StreamWriter w = File.AppendText("hist.dat")); //Creates the hist.dat file to read from, if file do not exist.

            while (true)
            {
                Console.Clear();
                foreach(string s in menu){ Console.WriteLine("     " + s); } // print each element in the string-list menu
                if(https.Count == 1){ print(7, 1, https.Count + " url loaded"); } // print 1 url loaded
                else{ print(7, 1, https.Count + " urls loaded"); } // or print 0 || 2+ urls loaded

                ConsoleKeyInfo info = Console.ReadKey(true);

                int.TryParse((info.KeyChar).ToString(), out int input); // trying to parse the ConsoleKeyInfo to a int

                if (info.Key == ConsoleKey.Escape) { SaveHist(hist); break; } // if escape is pressed (exit) the history is saved and the main loop breaks;
                else if (input == 1) // if input is 1 it goes to UserUrl which asks for url inputs.
                {
                    string[] ms = UserUrl();
                    if (ms[0] != "")
                    {
                        foreach (string s in ms)
                        {
                            try
                            {
                                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(s); // an youtube api webrequest is created
                                HttpWebResponse response = (HttpWebResponse)request.GetResponse(); // the response is stored in response.
                                StreamReader sr = new StreamReader(response.GetResponseStream()); 
                                string allLines = sr.ReadToEnd(); // when all of the stream have ended, its added to a string allLines
                                int id = videos.Count;
                                if (VideoExists(allLines)) { videos.Add(CreateVideo(allLines, id)); https.Add(s); hist.Add(s); } // if the url exists, the video is created, its added to the history, and to the loaded url list
                            }
                            catch
                            {
                                print(5, 7, "Error. Invalid url.\n     Press any key to return."); // If url doesnt exists
                                Console.ReadKey(true); // pause, true indicates no output to console.
                            }
                        }
                    }
                }
                else if (input == 2) { string[] ms = UserUrl(); if (ms[0] != "") { foreach (string s in ms) { try { videos.RemoveAt(https.IndexOf(s)); https.Remove(s); } catch { Console.WriteLine("URL not found. Press any key to return"); Console.ReadKey(true); } } } }
                // The above is the function to remove a entry. Both from the videos (Video object), the url in https
                else if (input == 3) // if the input is 3 its displaying video data.
                {
                    for (int i = 0; i < videos.Count;) // for loop so the index can be better controlled
                    {
                        ConsoleKey response = DisplayVideoData(videos[i],i,videos.Count); // displays the video with index i

                        if (response == ConsoleKey.Escape)
                        { break; } //Escape, back to menu
                        if (response == ConsoleKey.LeftArrow)
                        {
                            if (i >= 1)
                            {
                                i--;
                            }
                        } //Left arrow, < backwards, i--
                        if (response == ConsoleKey.RightArrow)
                        { if (i != videos.Count) i++; } //Right Arrow, > forwards, i++
                        if (response == ConsoleKey.End)
                        {
                            OpenURL(https[i]);
                        } // end key, opens url

                    }
                }  // display video data
                else if (input == 4) // input 4 loads urls from the FileUrl metode. Which points to the urls.dat file
                {
                    string[] ms = FileUrl();
                    if (ms[0] != "")
                    {
                        foreach (string s in ms) { https.Add(s); hist.Add(s); } // for each entry, add it to the urls and history lists
                    }
                    for (int i = 0; i < ms.Length; i++)
                    {
                        HttpWebRequest request = (HttpWebRequest)WebRequest.Create(https[i]);
                        HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                        StreamReader sr = new StreamReader(response.GetResponseStream());
                        string allLines = sr.ReadToEnd();
                        int id = videos.Count;
                        print(5, 1, "                              ");
                        if (VideoExists(allLines)) { videos.Add(CreateVideo(allLines, id)); print(7, 1, i + " of " + ms.Length + " urls loaded"); } // displays the progress x of y urls loaded at the top.
                    }
                    // save hist
                }

                else if (input == 5) { DisplayHist(hist); } // if input is 5, the history is displayed

                else // Error message and a pause if any other key is pressed.
                {
                    print(5, 9, "Error. Please press one of the keys shown above.\n     Press any key to return.");
                    Console.ReadKey(true);
                }
            }
        }

        static string[] UserUrl() // method to load urls by user input.
        {
            Console.Clear();
            Console.WriteLine("     Please enter your YouTube url(s) in the following way:\n     https://www.youtube.com/watch?v=dQw4w9WgXcQ (single url)\n     url , url (multiple urls, split by commas)\n     Press enter after entering your url(s) or before, if you want to exit.");
            Console.Write("\n     "); string s = Console.ReadLine();
            string[] ms = s.Split(',');
            return ms;
        }

        static string[] FileUrl() // method to load urls by file.
        {
            string[] ms = File.ReadAllLines("urls.dat");
            return ms;
        }

        static void SaveHist(List<string> ls) // saves the hist list to the hist.dat file
        {
            string[] ms = File.ReadAllLines("hist.dat");
            TextWriter tw = new StreamWriter("hist.dat");

            ls.Reverse(); // reverse order, 
            foreach (string s in ms) ls.Add(s); // adds the new urls to the back of the list.
            if (ls.Count > 10) { ls.RemoveRange(10,ls.Count-10); } // removes any urls past 10.

            foreach (string s in ls) { tw.WriteLine(s); } // writes the urls to file (saves)

            tw.Close(); // closes the connection with file
        }
        static void DisplayHist(List<string> ls) { // method to load and display urls from history.
            SaveHist(ls);
            string[] ms = File.ReadAllLines("hist.dat");
            Console.Clear();
            Console.SetCursorPosition(0, 3); Console.WriteLine("     History of last "+ms.Length+" urls \n");
            foreach (string s in ms)
            {
                Console.WriteLine("     "+s);
            }
            Console.ReadKey(true);
        }

        static bool VideoExists(string s) // checks if the Video contains "eow-title" amongst its long string, if it doesnt, url isnt valid.
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
                    create.Imgpath = allLines[i + 1];
                    create.Imgpath = FormatVideoData(create, "thumbnail");
                }
                else if (allLines[i].Contains("eow-title"))
                {
                    string path = "";
                    for (int j = i; j < allLines.Length; j++)
                    {
                        if (allLines[j].Contains("\">")) { i = j; break; }
                    }
                    for (int j = i + 1; j < allLines.Length; j++)
                    {
                        if (!allLines[j].Contains("</span")) { path += allLines[j] + " "; }
                        else { create.Title = path; break; }
                    }
                    create.Title = FormatVideoData(create, "title");
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
                        else
                        {
                            string[] ending = allLines[j].Split('<');
                            desc += ending[0];
                            create.Description = desc;
                            break;
                        }
                    }

                    create.Description = FormatVideoData(create, "description");
                }

                else if (allLines[i].Contains("keywords\\\":"))
                {
                    string newTags = "";
                    string[] allTags = allLines[i].Split('\\');
                    for (int j = 8; j < allTags.Length - 10; j += 2)
                    {
                        newTags += allTags[j];
                    }
                    string[] tags = newTags.Split('"');
                    for (int k = 0; k < tags.Length; k++)
                    { create.Keywords += tags[k] + " "; }

                    create.Keywords = FormatVideoData(create, "keywords");
                    if (create.Keywords == null) { create.Keywords = " "; }
                }

                else if (allLines[i].Contains("watch-view-count"))
                {
                    string[] allS = allLines[i].Split('"');
                    create.Views = allS[allS.Length - 1];
                    create.Views = FormatVideoData(create, "views");
                }

            }
            if (create.Keywords == null) { create.Keywords = "None"; }
            if (create.Description == null) { create.Description = "None"; }
            if (string.IsNullOrWhiteSpace(create.Description)) { create.Description = "None"; }
            return create;
        }

        static string FormatVideoData(Video vid, string type)
        {
            if (type == "title")
            {
                // common issues &quot; &#39; &amp;
                //   How to skip &quot;The Fallen Protectors&quot; @ Siege of Orgrimmar - WoW
                string title = vid.Title.Substring(3, vid.Title.Length - 3);
                title = title.Replace("&quot;", "\"");
                title = title.Replace("&#39;", "\'");
                title = title.Replace("&amp;", "&");
                return title;
            }
            if (type == "description")
            {
                string description = vid.Description.Substring(1, vid.Description.Length - 1);

                /*
                borrowed code from Stackoverflow:
                https://stackoverflow.com/questions/18153998/how-do-i-remove-all-html-tags-from-a-string-without-knowing-which-tags-are-in-it
                comments by ssilas777
                and Thierry_S
                */

                HtmlDocument htmlDoc = new HtmlDocument();
                htmlDoc.LoadHtml(description);

                string result = WebUtility.HtmlDecode(description);
                result = htmlDoc.DocumentNode.InnerText;
                description = result;

                /*
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
                */
                return description;
            }
            if (type == "keywords")
            {
                string tags = vid.Keywords.Substring(1, vid.Keywords.Length - 1);
                if (tags == "") { tags = "None"; }
                return tags;
            }
            if (type == "views")
            {
                string views = vid.Views.Substring(1, vid.Views.Length - 1);
                return views;
            }
            if (type == "thumbnail")
            {
                string thumbnail = vid.Imgpath.Substring(6, vid.Imgpath.Length - 9);
                return thumbnail;
            }
            return "error occured";
        }

        static ConsoleKey DisplayVideoData(Video vid, int id, int tot)
        {
            id += 1;
            int x = 5, y = 6, dsc = 17;
            int keylen = 0;
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.White;
            Console.Clear();
            controls();
            print(x, 1, "Displaying video "); color(id.ToString()); print(x + 18 + id.ToString().Length, 1, "of " + tot);
            print(x, 3, "press "); color("END"); print(x + 9, 3, " to view the video in browser");

            print(x, y, "Thumbnail:\n     " + vid.Imgpath + "\n     press "); color("DELETE"); print(x + 12, y + 2, " to view the thumbnail");
            print(x, y + 7, "Title:\n     " + vid.Title);
            print(x, y + 10, "Views:\n     " + vid.Views);

            int dsclen = vid.Description.Length;
            if (dsclen > 50)
            {
                print(x, y + 13, "Description:\n     " + vid.Description.Substring(0, 50) + " ...");
                print(x, y + 15, "press "); color("UP ARROW"); print(x + 14, y + 15, " to view more description");
            }
            else
            {
                print(x, y + 13, "Description:\n     " + vid.Description);
            }

            try { keylen = Convert.ToInt32(vid.Keywords.Length); } catch { }

            if (keylen > 40)
            {
                print(x, y + dsc, "Tags:\n     " + vid.Keywords.Substring(0, 40) + " ...");
                print(x, y + dsc + 2, "press "); color("DOWN ARROW"); print(x + 16, y + dsc + 2, " to view more tags");
            }
            else
            {
                if (keylen == 0) { print(x, y + dsc, "Tags:\n     None"); }
                else { print(x, y + dsc, "Tags:\n     " + vid.Keywords); }
            }

            ConsoleKeyInfo info = Console.ReadKey(true);
            bool run = true;

            while (info.Key != ConsoleKey.LeftArrow && info.Key != ConsoleKey.RightArrow && info.Key != ConsoleKey.Escape && info.Key != ConsoleKey.End || run)
            {
                if (run == false)
                {
                    info = Console.ReadKey(true);
                }
                run = false;

                if (info.Key == ConsoleKey.Delete)
                {
                    DisplayThumbnail(vid.Imgpath);
                }

                int descLines = 1;
                if (info.Key == ConsoleKey.UpArrow && vid.Description.Length > 0) //extend decription, up arrow
                {
                    Console.Clear();
                    controls();
                    print(x, 1, "Displaying video "); color(id.ToString()); print(x + 18+ id.ToString().Length, 3, " of "+tot);
                    print(x, 3, "press "); color("END"); print(x + 9, 3, " to view the video in browser");
                    print(x, y, "Thumbnail:\n     " + vid.Imgpath + "\n     press "); color("DELETE"); print(x + 12, y + 2, " to view the thumbnail");
                    print(x, y + 7, "Title:\n     " + vid.Title);
                    print(x, y + 10, "Views:\n     " + vid.Views);
                    int descLength = 0;
                    print(x, y + 13, "Description:");
                    Console.Write("\n     ");
                    string[] extend_desc = vid.Description.Split(' ');
                    for (int i = 0; i < extend_desc.Length; i++)
                    {
                        descLength += extend_desc[i].Length;
                        if (descLength < 50) { Console.Write(extend_desc[i] + " "); }
                        else
                        {
                            descLength = 0; Console.Write("\n     " + extend_desc[i] + " ");
                            descLength += extend_desc[i].Length; descLines++;
                        }
                    }
                    print(x, y + dsc + descLines - 3, "press "); color("ENTER"); print(x + 11, y + dsc + descLines - 3, " to view less");
                    if (descLines > 0) descLines--;
                    if (keylen > 40)
                    {
                        print(x, y + dsc + descLines, "Tags: \n     " + vid.Keywords.Substring(0, 40) + " ...");
                        print(x, y + dsc + descLines + 2, "press "); color("DOWN ARROW"); print(x + 16, y + dsc + descLines + 2, " to view more tags");
                    }
                    else
                    {
                        print(x, y + dsc + descLines, "Tags:\n     " + vid.Keywords);
                    }
                }

                if (info.Key == ConsoleKey.DownArrow && vid.Keywords.Length > 0) //extend tags, down arrow
                {
                    Console.Clear();
                    controls();
                    print(x, 1, "Displaying video "); color(id.ToString()); print(x + 18 + id.ToString().Length, 3, " of " + tot);
                    print(x, 3, "press "); color("END"); print(x + 9, 3, " to view the video in browser");
                    print(x, y, "Thumbnail:\n     " + vid.Imgpath + "\n     press "); color("DELETE"); print(x + 12, y + 2, " to view the thumbnail");
                    print(x, y + 7, "Title:\n     " + vid.Title);
                    print(x, y + 10, "Views:\n     " + vid.Views);
                    if (dsclen > 50)
                    {
                        print(x, y + 13, "Description:\n     " + vid.Description.Substring(0, 50) + " ...");
                        print(x, y + 15, "press "); color("UP ARROW"); print(x + 14, y + 15, " to view more description");
                    }
                    else
                    {
                        print(x, y + 13, "Description:\n     " + vid.Description);
                    }
                    print(x, y + dsc, "Tags:");
                    Console.Write("\n     ");
                    string[] tags = vid.Keywords.Split(' ');
                    int taglen = 0;
                    for (int i = 0; i < tags.Length; i++)
                    {
                        Console.Write(tags[i] + " ");
                        if (i % 8 == 0 && i != 0) { Console.Write("\n     "); taglen++; }
                    }
                    print(5, 22 + taglen, "press "); color("ENTER"); print(16, 22 + taglen, " to view less");
                }

                if (info.Key == ConsoleKey.Enter) //minimize, enter
                {
                    Console.Clear();
                    controls();
                    print(x, 1, "Displaying video "); color(id.ToString()); print(x + 18 + id.ToString().Length, 3, " of " + tot);
                    print(x, 3, "press "); color("END"); print(x + 9, 3, " to view the video in browser");
                    print(x, y, "Thumbnail:\n     " + vid.Imgpath + "\n     press "); color("DELETE"); print(x + 12, y + 2, " to view the thumbnail");
                    print(x, y + 7, "Title:\n     " + vid.Title);
                    print(x, y + 10, "Views:\n     " + vid.Views);

                    dsclen = vid.Description.Length;
                    if (dsclen > 50)
                    {
                        print(x, y + 13, "Description:\n     " + vid.Description.Substring(0, 50) + " ...");
                        print(x, y + 15, "press "); color("UP ARROW"); print(x + 14, y + 15, " to view more description");
                    }
                    else
                    {
                        print(x, y + 13, "Description:\n     " + vid.Description);
                    }

                    try { keylen = Convert.ToInt32(vid.Keywords.Length); } catch { }

                    if (keylen > 40)
                    {
                        print(x, y + dsc, "Tags:\n     " + vid.Keywords.Substring(0, 40) + " ...");
                        print(x, y + dsc + 2, "press "); color("DOWN ARROW"); print(x + 16, y + dsc + 2, " to view more tags");
                    }
                    else
                    {
                        if (keylen == 0) { print(x, y + dsc, "Tags:\n     None"); }
                        else { print(x, y + dsc, "Tags:\n     " + vid.Keywords); }
                    }
                }
                if (info.Key == ConsoleKey.LeftArrow) //< back
                { return ConsoleKey.LeftArrow; }
                if (info.Key == ConsoleKey.RightArrow) //> forward
                { return ConsoleKey.RightArrow; }
                if (info.Key == ConsoleKey.Escape) //Escape, back to menu
                { return ConsoleKey.Escape; }
                if (info.Key == ConsoleKey.End) // End, open in browser
                { return ConsoleKey.End; }
            }
            return ConsoleKey.C;
        }
        static void controls()
        {
            Console.Clear();
            int i = 2;
            print(75, 1+i, "Controls"); print(90, 1 + i, "Functions");

            print(75, 3 + i, "DOWN ARROW"); print(90, 3 + i, "Show more tags");
            print(75, 4 + i, "UP ARROW"); print(90, 4 + i, "Show more description");
            print(75, 5 + i, "LEFT ARROW"); print(90, 5 + i, "Show previous video");
            print(75, 6 + i, "RIGHT ARROW"); print(90, 6 + i, "Show next video");
            print(75, 7 + i, "DELETE"); print(90, 7 + i, "Show thumbnail");
            print(75, 8 + i, "END"); print(90, 8 + i, "Open in browser");
            print(75, 9 + i, "ENTER"); print(90, 9 + i, "Minimize");
            print(75, 10 + i, "ESCAPE"); print(90, 10 + i, "Exit / return to menu");
        }

        static void print(int x, int y, string s)
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.SetCursorPosition(x, y); Console.Write(s);
        }

        static void color(string s)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write(s);
        }

        static void DisplayThumbnail(string url)
        {
            using (WebClient client = new WebClient())
            {
                string imgname = url.Substring(url.Length - 25, 25).Replace("/", "-");
                client.DownloadFile(new Uri(url), imgname);
                Thread.Sleep(3000);
                Process.Start(imgname);
            }
        }

        public static void OpenURL(string url)
        {
            Process.Start(url);
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
}