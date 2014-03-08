using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Queue;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Hosting;
using System.Web.Script.Serialization;
using System.Web.Script.Services;
using System.Web.Services;
using WorkerRole;

namespace WebRole
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
    public class WebService : System.Web.Services.WebService
    {
        public static Trie library;
        public static string file;
        private string state;
        private string line;
        private int count;

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
        public void downloadBlob()
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(ConfigurationManager.AppSettings["StorageConnectionString"]);
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            CloudBlobContainer container = blobClient.GetContainerReference("babyblob");
            if (container.Exists())
            {
                file = HostingEnvironment.ApplicationPhysicalPath + "\\data.txt";
                foreach (IListBlobItem item in container.ListBlobs(null, false))
                {
                    if (item.GetType() == typeof(CloudBlockBlob))
                    {
                        CloudBlockBlob blob = (CloudBlockBlob)item;
                        using (var fs = new FileStream(file, FileMode.OpenOrCreate))
                        {
                            blob.DownloadToStream(fs);
                        }
                    }
                }
            }
        }

        [WebMethod]
        public string populateTrie()
        {
            count = 0;
            library = new Trie();
            PerformanceCounter ram = new PerformanceCounter("Memory", "Available MBytes");
            ram.NextValue();
            using (StreamReader sr = File.OpenText(file))
            {
                while ((line = sr.ReadLine()) != null)
                {
                    if (count % 1000 == 0 && ram.NextValue() <= 1000)
                        break;
                    count++;
                    library.insertLine(line.ToLower());
                }
                
            }
            return "pupulated succesfully " + ram.NextValue();
        }

        [WebMethod]
        public int trieCount ()
        {
            return count;
        }

        [WebMethod]
        public string lastLine ()
        {
            return line;
        }

        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public string querySuggestions(string word)
        {

            string[] json = library.getSuggestions(word.ToLower());
            return new JavaScriptSerializer().Serialize(json);

        }

        [WebMethod]
        public string StartCrawling(string website)
        {
            try
            {
                CloudQueue commandQueue = CreateQueue("commandqueue");
                CloudQueueMessage message = new CloudQueueMessage("Start http://" + website);
                commandQueue.AddMessage(message);
                return "Succesfully began Crawling: " + website;
            }
            catch
            { 
                return "Please enter a valid website";
            }
        }

        [WebMethod]
        public string StopCrawling()
        {
            state = "Stopped";
            CloudQueue commandQueue = CreateQueue("commandqueue");
            CloudQueueMessage message = new CloudQueueMessage("Stop");
            commandQueue.AddMessage(message);
            return "Stopping/Clearing Crawler, This may take a few minutes";
        }

        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public string PageUrls(string site)
        {
            if (string.IsNullOrEmpty(state))
            {
                List<string> json = new List<string>();
                try
                {
                    CloudTable urlTable = CreateTable("websitetable");
                    TableQuery<UriEntity> query = new TableQuery<UriEntity>()
                        .Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, site));
                    foreach (UriEntity entity in urlTable.ExecuteQuery(query))
                    {
                        json.Add(entity.RowKey);
                    }
                    return new JavaScriptSerializer().Serialize(json.ToArray());
                }
                catch
                {
                    return new JavaScriptSerializer().Serialize(json.ToArray());
                }
            }
            else
                return "Can not retrieve data at this time";
        }

        [WebMethod]
        public string Errors()
        {
            try
            {
                CloudTable urlTable = CreateTable("errortable");
                TableQuery<ErrorEntity> query = new TableQuery<ErrorEntity>()
                .Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, "Error"));
                string result = "";
                    foreach (ErrorEntity entity in urlTable.ExecuteQuery(query))
                    {
                        result += entity.Description + " <Br/> ";
                    }
                    return result;
            }
            catch
            { 
                return "Can not retrieve data at this time";
            }
        }

        [WebMethod]
        public string MachineCounters()
        {
            try
            {
                IEnumerable<ResultEntity> urlTable = query();
                string counters = "";
                foreach (ResultEntity entity in urlTable)
                {
                    counters = entity.Counters;
                }
                return counters;
            }
            catch
            {
                return "Can not retrieve data at this time";
            }
        }

        [WebMethod]
        public string numberOfURLsCrawled()
        {
           try
            {
                IEnumerable<ResultEntity> urlTable = query();
                string result = "0";
                foreach (ResultEntity entity in urlTable)
                {
                    result = "" + entity.NumOfUrls;
                }
                return result;
            }
            catch
           {
                return "Can not retrieve data at this time"; 
           }
                

        }

        [WebMethod]
        public string LastTenURLsCrawled()
        {
           try
            {
                IEnumerable<ResultEntity> urlTable = query();
                 string lastTen = "";
                foreach (ResultEntity entity in urlTable)
                {
                    lastTen = entity.LastTen;
                }
                return lastTen.Replace(" ", " <Br/> ");
            }
            catch
           {
               return "Can not retrieve data at this time";
           }
        }

        [WebMethod]
        public string SizeOfQueue()
        {
            CloudQueue queue = CreateQueue("websitequeue");
            queue.FetchAttributes();
            return queue.ApproximateMessageCount.ToString();
        }

        [WebMethod]
        public string SizeOfTable()
        {
            try
            {
                IEnumerable<ResultEntity> urlTable = query();
                string result = "0";
                foreach (ResultEntity entity in urlTable)
                {
                    result = "" + entity.TableSize;
                }
                return result;
            }
             catch
            {
                return "Can not retrieve data at this time";
            }
        }

        [WebMethod]
        public string CrawlerStatus()
        {
            try
            {
                IEnumerable<ResultEntity> urlTable = query();
                string result = "";
                foreach (ResultEntity entity in urlTable)
                {
                    result = entity.State;
                }
                return result;
            }
            catch
            {
                return "Stopped/Cleared";
            }
        }

        private IEnumerable<ResultEntity> query()
        {
            CloudTable urlTable = CreateTable("resulttable");
            TableQuery<ResultEntity> query = new TableQuery<ResultEntity>()
                .Where(TableQuery.CombineFilters(
                TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, "Result"),
                TableOperators.And,
                TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, "Result"))
                );
            return urlTable.ExecuteQuery(query);
        }

        private CloudQueue CreateQueue(string refrence)
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(
                CloudConfigurationManager.GetSetting("StorageConnectionString"));
            CloudQueueClient queueClient = storageAccount.CreateCloudQueueClient();
            CloudQueue queue = queueClient.GetQueueReference(refrence);
            queue.CreateIfNotExists();
            return queue;
        }

        private CloudTable CreateTable(string refrence)
        {
            try
            {
                CloudStorageAccount storageAccount = CloudStorageAccount.Parse(
                CloudConfigurationManager.GetSetting("StorageConnectionString"));
                CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
                CloudTable table = tableClient.GetTableReference(refrence);
                table.CreateIfNotExists();
                return table;
            }
            catch
            {
                CloudStorageAccount storageAccount = CloudStorageAccount.Parse(
                CloudConfigurationManager.GetSetting("StorageConnectionString"));
                CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
                CloudTable table = tableClient.GetTableReference(refrence);
                return table;
            }
            
        }
    }
}