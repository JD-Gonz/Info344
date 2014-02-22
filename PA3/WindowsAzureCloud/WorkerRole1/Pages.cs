using HtmlAgilityPack;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace WorkerRole1
{
    class Pages
    {
        private string root;
        private string rootName;
        private HashSet<string> visited;
        private List<string> disallow;

        public Pages()
        {
            root = null;
            visited = new HashSet<string>();
            disallow = new List<string>();
        }

        public string getRoot()
        {
            return rootName;
        }

        public void setRoot (string website)
        {
            root = website;
            rootName = website.Substring(11);
        }

        public List<string> listOfLinks (string website)
        {
            List<string> hyperlinks = new List<string>();
            HtmlWeb web = new HtmlWeb();
            try
            {
                HtmlDocument doc = web.Load(website);
                foreach (HtmlNode node in doc.DocumentNode.SelectNodes("//a"))
                {
                    if (node.Attributes.Contains("href") && node.Attributes["href"].Value.ToString().Contains(rootName + "/"))
                        hyperlinks.Add(node.Attributes["href"].Value.ToString());
                }
                return isValid(hyperlinks);
            }
            catch
            {
                return new List<string>();
            }
            
        }

        public List<string> getRobots(string website)
        {
            visited.Add(website);
            string RobotsTxtFile = website + "//robots.txt";
            List<string> siteMaps = new List<string>();
            WebClient web = new WebClient();
            try
            {
                using (var stream = web.OpenRead(RobotsTxtFile))
                using (var reader = new StreamReader(stream))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        if (line.StartsWith("Sitemap:"))
                        {
                            string[] s = line.Split(' ');
                            siteMaps.Add(s[1]);
                        }
                        else if (line.StartsWith("Disallow:"))
                        {
                            line = line.Substring(9).Trim();
                            disallow.Add(line);
                        }
                    }
                    return isValid(siteMaps);
                }
            }
            catch
            {
                return new List<string>();
            }
        }

        public string getTitle(string website)
        {
            WebClient web = new WebClient();

            //insert try catch for this.


            string html = web.DownloadString(website);
            string[] separators = html.Split(new string[] { "<title>", "</title>" }, StringSplitOptions.None);
            return separators[1];
        }

        public string getDate(string website)
        {
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(website);
            HttpWebResponse res = (HttpWebResponse)req.GetResponse();
            return res.LastModified.ToString();
             
        }

        private List<string> siteMapUrls(string website)
        {
            visited.Add(website);
            WebClient web = new WebClient();
            string html = web.DownloadString(website);
            string[] separators = new string[] { "<loc>", "</loc>" };
            List<string> siteMaps = html.Split(separators, StringSplitOptions.None).Select(s =>
            {
                if (s.Contains("http") && s.Contains(rootName))
                    return s;
                else
                    return null;
            }).ToList();
            siteMaps.RemoveAll(item => item == null);
            return isValid(siteMaps); 
        }

        private List<string> isValid(List<string> hyperlinks)
        {
            List<string> validLinks = new List<string>();
            foreach (string link in hyperlinks)
            {
                bool add = true;
                foreach (string test in disallow)
                {
                    if (link.Contains(test))
                        add = false;
                }
                if (add)
                    validLinks.Add(link);
            }
            return validLinks;
        }
    }
}
