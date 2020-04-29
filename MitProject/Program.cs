using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace MitProject
{
    class Program
    {
        static Video vid = new Video();

        static void Main(string[] args)
        {
          
            string[] https = { "https://www.youtube.com/watch?v=YGXS4uEdT3Q", "https://www.youtube.com/watch?v=iFGve5MUUnE", "https://www.youtube.com/watch?v=7W0IFAGyqwo", "https://www.youtube.com/watch?v=7W0ISI3yqwo", "https://www.youtube.com/watch?v=7NOPE" };
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
            Console.ReadKey(true);
            Console.Clear();
            Console.WriteLine(videos[0].Id);
            Console.WriteLine(videos[0].Title);
            Console.WriteLine(videos[0].Description);
            Console.WriteLine(videos[0].Views);
            Console.WriteLine(videos[0].Imgpath);
            Console.WriteLine(videos[0].Keywords);
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
             CreateVideo can create a Video object based of the string s (data fetched in main) and int id (place on the list).

             OBS: The output is not propper formatted yet!
            */

            Video create = new Video();
            create.Id = id;
            string[] allLines = s.Split(' ');
            for (int i = 0; i < allLines.Length; i++)
            {
                if (allLines[i].Contains("thumbnailUrl"))
                { create.Imgpath = allLines[i+1]; }
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
                }

                else if (allLines[i].Contains("eow-description"))
                {
                    string desc = "";
                    for (int j = i; j < allLines.Length; j++)
                    {
                        if (allLines[j].Contains(">")) { i = j; break; }
                    }
                    for (int j = i + 1; j < allLines.Length; j++)
                    {
                        if (!allLines[j].Contains("</p>")) { desc += allLines[j] + " "; }
                        else {
                            string[] ending = allLines[j].Split('<');
                            desc += ending[0];
                            create.Description = desc;
                            break; }
                    }
                    Console.WriteLine(create.Description);
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
                    Console.WriteLine(vid.Keywords);
                }

                else if (allLines[i].Contains("watch-view-count"))
                {
                    string[] allS = allLines[i].Split('"');
                    create.Views = allS[allS.Length - 1];
                    Console.WriteLine(vid.Views);
                }

            }
            return create;
        }

        static void FormatVideoData() {

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