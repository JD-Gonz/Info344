using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Web;
using System.Web.Script.Services;
using System.Web.Services;
using WorkerRole1;

namespace WebRole1
{
    /// <summary>
    /// Summary description for admin
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    // [System.Web.Script.Services.ScriptService]
    [ScriptService]
    public class admin : System.Web.Services.WebService
    {
        private PerformanceCounter ramCounter;
        private PerformanceCounter cpuCounter;
        private float ram;
        private float cpu;
        private string state;

        [WebMethod]
        public string StartCrawling(string website)
        {
            if (!string.IsNullOrEmpty(website))
            {
                CloudQueue commandQueue = CreateQueue("commandqueue");
                CloudQueueMessage message = new CloudQueueMessage("Start http://" + website);
                commandQueue.AddMessage(message);
                return "Succesfully added: " + website + " to the queue";
            }
            else
                return "Please enter a valid website";
            
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
        public string GetPageTitle(string site)
        {
            if (string.IsNullOrEmpty(state))
            {
                try
                {
                    Uri uri = new UriBuilder(site.Replace("www.", "")).Uri;
                    CloudTable urlTable = CreateTable("websitetable");
                    TableQuery<UriEntity> query = new TableQuery<UriEntity>()
                        .Where(TableQuery.CombineFilters(
                        TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, uri.Host),
                        TableOperators.And,
                        TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, HttpUtility.UrlEncode(uri.AbsoluteUri)))
                        );
                    string result = "";
                    foreach (UriEntity entity in urlTable.ExecuteQuery(query))
                    {
                        result = entity.Name + " " + entity.Date;
                        break;
                    }
                    return result;
                }
                catch
                {
                    return "Provided URL could not be used";
                }
            }
            else
                return "Can not retrieve data at this time";
            
        }

        [WebMethod]
        public string Errors()
        {
            if (string.IsNullOrEmpty(state))
            {
                CloudTable urlTable = CreateTable("errortable");
                TableQuery<ErrorEntity> query = new TableQuery<ErrorEntity>()
                .Where(TableQuery.CombineFilters(
                TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, "Error"),
                TableOperators.And,
                TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, "Error"))
                );
                string result = "";
                foreach (ErrorEntity entity in urlTable.ExecuteQuery(query))
                {
                    result += entity.Description + " <Br/> "; 
                }
                return result;
            }
            else
                return "Can not retrieve data at this time"; 
        }


        [WebMethod]
        public string MachineCounters()
        {
            if (ramCounter == null)
            {
                ramCounter = new PerformanceCounter("Memory", "Available MBytes");
                cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
                ram = ramCounter.NextValue();
                cpu = cpuCounter.NextValue();
            }
            ram = ramCounter.NextValue();
            cpu = cpuCounter.NextValue();

            return ("RAM: " + (ram) + " MB | CPU: " + (cpu) + " %");
        }

        [WebMethod]
        public string numberOfURLsCrawled()
        {
            if (string.IsNullOrEmpty(state))
            {
                IEnumerable<ResultEntity> urlTable = query();
                string result = "0";
                foreach (ResultEntity entity in urlTable)
                {
                    result = "" + entity.NumOfUrls;
                }
                return result;
            }
            else
                return "Can not retrieve data at this time";

        }

        [WebMethod]
        public string LastTenURLsCrawled()
        {
            if (string.IsNullOrEmpty(state))
            {
                IEnumerable<ResultEntity> urlTable = query();
                 string lastTen = "";
                foreach (ResultEntity entity in urlTable)
                {
                    lastTen = entity.LastTen;
                }
                return lastTen.Replace(" ", " <Br/> ");
            }
            else
                return "Can not retrieve data at this time";
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
            if (string.IsNullOrEmpty(state))
            {
                IEnumerable<ResultEntity> urlTable = query();
                string result = "0";
                foreach (ResultEntity entity in urlTable)
                {
                    result = "" + entity.TableSize;
                }
                return result;
            }
             else
                 return "Can not retrieve data at this time";
        }

        [WebMethod]
        public string CrawlerStatus()
        {
            if (string.IsNullOrEmpty(state))
            {
                IEnumerable<ResultEntity> urlTable = query();
                string result = "";
                foreach (ResultEntity entity in urlTable)
                {
                    result = entity.State;
                }
                return result;
            }
            else
                return "Stopping/Clearing Crawler, This may take a few minutes";
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
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(
                CloudConfigurationManager.GetSetting("StorageConnectionString"));
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
            CloudTable table = tableClient.GetTableReference(refrence);
            table.CreateIfNotExists();
            return table;
        }
    }
}


