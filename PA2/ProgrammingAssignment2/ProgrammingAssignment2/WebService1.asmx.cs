using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Web;
using System.Configuration;
using System.Web.Services;
using System.Web.Script.Services;
using System.Web.Script.Serialization;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System.Data.SqlClient;
using Microsoft.WindowsAzure.Storage.Table;
using System.Threading;
using System.Diagnostics;


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
    [ScriptService]
    public class WebService1 : System.Web.Services.WebService
    {
        public static Trie library;
        
        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public string querySuggestions(string word)
        {
            
            string[] json = library.getSuggestions(word.ToLower());
            return new JavaScriptSerializer().Serialize(json);
           
        }


        [WebMethod]
        public float populateTrie(string filepath)
        {
            library = new Trie();
            PerformanceCounter ramCounter = new PerformanceCounter("Memory", "Available MBytes");
            using (StreamReader sr = new StreamReader(filepath))
            {
                int count = 0;
                string text = "";
                while (sr.EndOfStream == false)
                {
                    string line = sr.ReadLine().ToLower();
                    library.insertLine(line);

                    if (count == 1000)
                    {
                        count = 0;
                        if (ramCounter.NextValue() < 5000)
                            break;
                    }
                    
                }
                return ramCounter.NextValue();
            }

        }

        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
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
                
                return new JavaScriptSerializer().Serialize(file + "\\blob.txt");
            }
            else return "failure";
        }

        [WebMethod]
        public void preprocessFile()
        {
            using (StreamReader sr = new StreamReader("G:\\classes\\INFO 344\\PA2\\WebsiteTitles.txt"))
            using (StreamWriter sw = new StreamWriter("G:\\classes\\INFO 344\\PA2\\ProccessedTitlesMed.txt"))
            {
                while (sr.EndOfStream == false)
                {
                    string line = sr.ReadLine();
                    string reconstructedline = "";
                    foreach (char letter in line)
                    {
                        if (letter == '_')
                            reconstructedline += " ";
                        else if (letter != 32 && (letter < 65 || letter > 90) && (letter < 97 || letter > 122))
                        {
                            reconstructedline = null;
                            break;
                        }
                        else
                            reconstructedline += letter;
                    }
                    if (reconstructedline != null)
                        sw.WriteLine(reconstructedline);
                }
            }
        }
    }
}