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
using Microsoft.WindowsAzure.Storage.Auth;
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
        public static string file;

        [WebMethod]
        public void preprocessFile(string input, string output)
        {
            using (StreamReader sr = new StreamReader(input))
            using (StreamWriter sw = new StreamWriter(output))
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

        [WebMethod]
        public string downloadBlob()
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(ConfigurationManager.AppSettings["StorageConnectionString"]);
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            CloudBlobContainer container = blobClient.GetContainerReference("pa2");
            if (container.Exists())
            {
                int count = 0;
                file = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData).ToString();
                foreach (IListBlobItem item in container.ListBlobs(null, false))
                {
                    count++;
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
            return file;
        }

        [WebMethod]
        public string populateTrie(string fileName)
        {
            library = new Trie();
            int count = 0;
            string message = "";
            Process proc = Process.GetCurrentProcess();
            long benchmark = proc.PeakPagedMemorySize64 - 20000000;
            long peak = proc.PeakPagedMemorySize64;
            try 
            {
                using (StreamReader sr = new StreamReader(fileName))
                {
                    while (sr.EndOfStream == false)
                    {
                        string line = sr.ReadLine().ToLower();
                        library.insertLine(line);   
                        count ++;

                        if (count == 1000 && proc.PrivateMemorySize64 == benchmark)
                        {
                            message = "count: " + count + " priateMemory:  " + proc.PrivateMemorySize64 + " benchmark: " + benchmark + " out of " + peak;
                            break;
                        }
                        else if (count == 1000)
                            count = 0;
                    }
                }
                return message;
            }
            catch (OutOfMemoryException e)
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();
                return "count: " + count + " priateMemory:  " + proc.PrivateMemorySize64 + " benchmark: " + benchmark + " out of " + peak;
            }

        }

        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public string querySuggestions(string word)
        {

            string[] json = library.getSuggestions(word.ToLower());
            return new JavaScriptSerializer().Serialize(json);

        }  
    }
}