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
            CloudQueue commandQueue = CreateQueue("commandqueue");
            CloudQueueMessage message = new CloudQueueMessage("Start http://" + website);
            commandQueue.AddMessage(message);
            return "Succesfully added: " + website + " to the queue";
        }

        [WebMethod]
        public string StopCrawling()
        {
            state = "Stopped";
            CloudQueue commandQueue = CreateQueue("commandqueue");
            CloudQueueMessage message = new CloudQueueMessage("Stop");
            commandQueue.AddMessage(message);
            return "Stopping Crawler";
        }

        [WebMethod]
        public string ClearData()
        {
            state = "Stopped";
            CloudQueue commandQueue = CreateQueue("commandqueue");
            CloudQueueMessage message = new CloudQueueMessage("Clear");
            commandQueue.AddMessage(message);
            return "Clearing Data";
        }

        [WebMethod]
        public string GetPageTitle(string site)
        {
            try
            {
                Uri uri = new UriBuilder(site).Uri;
            }
            catch
            {
                // the given string could not be converted to a proper URL!!!
            }
            if (state == null)
            {
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
                    result = entity.name + " " + entity.date;
                    break;
                }
                return result;
            }
            else
            {
                return "Data Cleared or Crawler was Stopped";
            }
            
        }

        [WebMethod]
        public string Errors()
        {
            if (state == null)
            {
                CloudTable urlTable = CreateTable("errortable");
                TableQuery<ErrorEntity> query = new TableQuery<ErrorEntity>()
                    .Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, "Error"));
                string result = "";
                foreach (ErrorEntity entity in urlTable.ExecuteQuery(query))
                {
                    result += "Error: " + entity.url + " encountered " + entity.description + "||"; 
                }
                return result;
            }
            else
                return "Data Cleared or Crawler was Stopped"; 
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
        public int numberOfURLsCrawled()
        {
            IEnumerable<ResultEntity> urlTable = query();
            int result = 0;
            foreach (ResultEntity entity in urlTable)
            {
                result = entity.numOfUrlsCrawled;
            }
            return result;
        }

        [WebMethod]
        public string LastTenURLsCrawled()
        {
            IEnumerable<ResultEntity> urlTable = query();
             string lastTen = "";
            foreach (ResultEntity entity in urlTable)
            {
                lastTen = entity.lastTenCrawled;
            }
            return lastTen;
        }

        [WebMethod]
        public string SizeOfQueue()
        {
            CloudQueue queue = CreateQueue("websitequeue");
            queue.FetchAttributes();
            return queue.ApproximateMessageCount.ToString();
        }

        [WebMethod]
        public int SizeOfTable()
        {
            IEnumerable<ResultEntity> urlTable = query();
            int result = 0;
            foreach (ResultEntity entity in urlTable)
            {
                result = entity.tableSize;
            }
            return result;
        }

        [WebMethod]
        public string CrawlerStatus()
        {
            IEnumerable<ResultEntity> urlTable = query();
            string result = "";
            foreach (ResultEntity entity in urlTable)
            {
                result = entity.state;
            }
            return result;
        }

        private IEnumerable<ResultEntity> query()
        {
            if (state == null)
            {
                CloudTable urlTable = CreateTable("resulttable");
                TableQuery<ResultEntity> query = new TableQuery<ResultEntity>()
                    .Where(TableQuery.CombineFilters(
                    TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, "results"),
                    TableOperators.And,
                    TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, "update"))
                    );
                return urlTable.ExecuteQuery(query);
            }
            else
                return null;
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


