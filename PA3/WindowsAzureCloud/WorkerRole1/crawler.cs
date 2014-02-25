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

namespace WorkerRole1
{
    class Crawler
    {
        private  Uri root;
        private  HashSet<string> host;
        private  Hashtable visited;
        private  List<string> disallow;

        public Crawler()
        {
            root = null;
            host = new HashSet<string>();
            visited = new Hashtable();
            disallow = new List<string>();
        }

        public int getTableSize()
        {
           return visited.Count;
        }

        public string getLastUrls ()
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
            WebClient web = new WebClient();
            string source = web.DownloadString(website.ToString());
            return Regex.Match(source, @"\<title\b[^>]*\>\s*(?<Title>[\s\S]*?)\</title\>", RegexOptions.IgnoreCase).Groups["Title"].Value;
        }

        public DateTime getDate(Uri website)
        {
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(website);
            HttpWebResponse res = (HttpWebResponse)req.GetResponse();
            return res.LastModified;

        }

        public List<Uri> startCrawling(Uri website)
        {
            visited.Add(visited.Count + 1, website);
            Uri robots = new UriBuilder(website + "//robots.txt").Uri;
            if ((!host.Contains(website.Host)))
            {
                if (host.Count == 0)
                    root = website;
                host.Add(robots.Host);
                List<string> siteMaps = new List<string>();
                string userAgent = "*";
                WebClient web = new WebClient();

                using (var stream = web.OpenRead(robots))
                using (var reader = new StreamReader(stream))
                {
                    string lines;
                    while ((lines = reader.ReadLine()) != null && userAgent == "*")
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
            if (website.ToString().Contains("sitemaps"))
                return siteMapUrls(website);
            else
                return listOfLinks(website);
        }

        private List<Uri> listOfLinks(Uri website)
        {
            List<string> hyperlinks = new List<string>();
            HtmlWeb web = new HtmlWeb();
            HtmlDocument doc = web.Load(website.ToString());
            foreach (HtmlNode node in doc.DocumentNode.SelectNodes("//a"))
            {
                if (node.Attributes.Contains("href") && node.Attributes["href"].Value.ToString().Contains(root.Host + "/") &&
                    (node.Attributes["href"].Value.ToString().Contains(".html") || node.Attributes["href"].Value.ToString().Contains(".htm")))
                    hyperlinks.Add(node.Attributes["href"].Value);
            }
            return isValid(hyperlinks);
        }

        private List<Uri> siteMapUrls(Uri website)
        {
            List<string> siteMaps = new List<string>();
            XmlDocument web = new XmlDocument();
            web.Load(website.ToString());
            foreach (XmlNode node in web.GetElementsByTagName("loc"))
            {
                if (node.InnerText.Contains("http") && node.InnerText.Contains(root.Host))
                    siteMaps.Add(node.InnerText);
            }
            return isValid(siteMaps); 
        }

        private List<Uri> isValid(List<string> hyperlinks)
        {
            List<Uri> validLinks = new List<Uri>();
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
                    Uri validLink = new UriBuilder(link).Uri;
                    validLinks.Add(validLink);
                }
            }
            return validLinks;
        }
    }
}
