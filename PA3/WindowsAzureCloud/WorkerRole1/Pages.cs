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
        private Hashtable visited;
        private List<string> disallow;

        public Pages()
        {
            root = null;
            visited = new Hashtable();
            disallow = new List<string>();
        }

        public void setRoot (string website)
        {
            root = website;
            rootName = website.Substring(11);
        }

        public List<string> listOfLinks (string website)
        {
            WebClient web = new WebClient();
            string html = web.DownloadString(website);
            string[] separators = new string[] { "<a ", "/a>" };
            List<string> hyperlinks = html.Split(separators, StringSplitOptions.None).Select(s =>
            {
                if (s.Contains("href") && s.Contains(rootName))
                    return s;
                else
                    return null;
            }).ToList();
            hyperlinks.RemoveAll(item => item == null);
            return isValid(hyperlinks); 
        }

        private List<string> isValid (List<string> hyperlinks)
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

        public void getRobots(string website)
        {

            visited.Add(website, true);
            string RobotsTxtFile = root + "/robots.txt";
            WebClient web = new WebClient();
            using (var stream = web.OpenRead(RobotsTxtFile))
            using (var reader = new StreamReader(stream))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    if (line.StartsWith("Disallow:"))
                    {
                        line = line.Substring(9).Trim();
                        disallow.Add(line);
                    }
                }
            }
        }

        public string getTitle(string website)
        {
            WebClient web = new WebClient();
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
    }
}
