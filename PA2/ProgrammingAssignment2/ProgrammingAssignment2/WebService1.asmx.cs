using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using System.Configuration;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System.Data.SqlClient;
using System.IO;
using Microsoft.WindowsAzure.Storage.Table;
using System.Web.Script.Services;
using System.Web.Script.Serialization;

namespace ProgrammingAssignment2
{
    /// <summary>
    /// Summary description for WebService1
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    // [System.Web.Script.Services.ScriptService]
    public class WebService1 : System.Web.Services.WebService
    {
        public static Trie library;
        private static int maxlength = 10;
        
        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public string querySuggestions(string word)
        {
            string[] json = getSuggestions(word);
            return new JavaScriptSerializer().Serialize(json);
        }

        public string[] getSuggestions(string word)
        {
            if (library.Root == null)
                return null;
           return library.traverseTrie(word, library.Root, maxlength);    
        }


        [WebMethod]
        public void populateTrie(string filepath)
        {
            library = new Trie();
            List<string> wordlibrary = new List<string>();
            using (StreamReader sr = new StreamReader(filepath))
            {
                while (sr.EndOfStream == false)
                {
                    string line = sr.ReadLine();
                        line = library.formatLine(line);
                    wordlibrary.Add(line);
                }
            }
            library = new Trie(wordlibrary.ToArray());
        }

        [WebMethod]
        public string downloadBlob()
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(ConfigurationManager.AppSettings["StorageConnectionString"]);
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            CloudBlobContainer container = blobClient.GetContainerReference("pa2");
            if (container.Exists())
            {
                string file = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData).ToString();
                foreach (IListBlobItem item in container.ListBlobs(null, false))
                {
                    if (item.GetType() == typeof(CloudBlockBlob))
                    {
                        CloudBlockBlob blob = (CloudBlockBlob)item;
                        using (var fs = new FileStream(file + "\\blob.txt", FileMode.OpenOrCreate))
                        {
                            blob.DownloadToStream(fs);
                        }
                    }
                }
                return file;
            }
            else return "failure";
        }
    }
}
