using HtmlAgilityPack;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;

namespace WorkerRole
{
    class Crawler
    {
        private Uri root;
        private HashSet<string> host;
        private HashSet<string> inQueue;
        private Hashtable visited;
        private List<string> disallow;

        public Crawler()
        {
            root = null;
            host = new HashSet<string>();
            visited = new Hashtable();
            inQueue = new HashSet<string>();
            disallow = new List<string>();
        }

        public int getTableSize()
        {
            return visited.Count;
        }

        public string getLastUrls()
        {
            string result = "";
            int count = Math.Min(visited.Count, 9);
            for (int i = 0; i <= count; i++)
            {
                result += visited[visited.Count - i] + " ";
            }
            return result;
        }

        public string getTitle(Uri website)
        {
            try
            {
                WebClient web = new WebClient();
                string source = web.DownloadString(website.ToString());
                return Regex.Match(source, @"\<title\b[^>]*\>\s*(?<Title>[\s\S]*?)\</title\>", RegexOptions.IgnoreCase).Groups["Title"].Value;
            }
            catch
            {
                return null;
            }
        }

        public string getDate(Uri website)
        {
            try
            {
                HttpWebRequest req = (HttpWebRequest)WebRequest.Create(website);
                HttpWebResponse res = (HttpWebResponse)req.GetResponse();
                return res.LastModified.ToString();
            }
            catch
            {
                return null;
            }

        }

        public List<string> startCrawling(Uri website)
        {
            visited.Add(visited.Count + 1, website);
            Uri robots = new UriBuilder(website + "robots.txt").Uri;
            if ((!host.Contains(website.Host)))
            {
                if (host.Count == 0)
                    root = website;
                host.Add(robots.Host);
                try
                {
                    Stream data = new WebClient().OpenRead(robots);
                    StreamReader read = new StreamReader(data);
                    List<string> siteMaps = new List<string>();
                    string userAgent = "*";
                    {
                        string lines;
                        while ((lines = read.ReadLine()) != null && userAgent == "*")
                        {
                            if (lines.StartsWith("Sitemap:"))
                            {
                                string[] line = lines.Split(' ');
                                siteMaps.Add(line[1]);
                            }
                            else if (lines.StartsWith("User-agent:"))
                            {
                                string[] line = lines.Split(' ');
                                userAgent = line[1].Trim();
                            }
                            else if (lines.StartsWith("Disallow:"))
                            {
                                string[] line = lines.Split(' ');
                                disallow.Add(line[1]);
                            }
                        }
                        return isValid(siteMaps);
                    }
                }
                catch {/*No Robots.txt file*/}
            }
            if (website.ToString().Contains("sitemaps"))
                return siteMapUrls(website);
            else
                return listOfLinks(website);
        }

        private List<string> listOfLinks(Uri website)
        {
            List<string> hyperlinks = new List<string>();
            try
            {
                HtmlWeb web = new HtmlWeb();
                HtmlDocument doc = web.Load(website.ToString());
                foreach (HtmlNode node in doc.DocumentNode.SelectNodes("//a"))
                {
                    if (node.Attributes.Contains("href"))
                    {
                        string link = node.Attributes["href"].Value.ToString();
                        if (link.StartsWith("http:///"))
                            link = link.Insert(7, website.Host);
                        if (link.Contains(root.Host + "/") && (link.Contains(".html") || link.Contains(".htm")) && !visited.ContainsValue(link) && !inQueue.Contains(link))
                            hyperlinks.Add(node.Attributes["href"].Value);
                    }
                }
                return isValid(hyperlinks);
            }
            catch
            {
                hyperlinks.Add("ERROR 408: Request Timed-Out when accessing: " + website);
                return hyperlinks;
            }
        }

        private List<string> siteMapUrls(Uri website)
        {
            List<string> siteMaps = new List<string>();
            XmlDocument web = new XmlDocument();
            web.Load(website.ToString());
            foreach (XmlNode node in web.GetElementsByTagName("loc"))
            {
                if (node.InnerText.Contains("http") && node.InnerText.Contains(root.Host + "/"))
                    siteMaps.Add(node.InnerText);
            }
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
                {
                    inQueue.Add(link);
                    validLinks.Add(link);
                }
            }
            return validLinks;
        }
    }
}
