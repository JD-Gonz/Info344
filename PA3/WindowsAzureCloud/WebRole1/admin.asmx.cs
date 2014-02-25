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
    public class admin : System.Web.Services.WebService
    {
        [WebMethod]
        public string StartCrawling(string website)
        {
            CloudQueue commandQueue = CreateQueue("commandqueue");
            CloudQueueMessage message = new CloudQueueMessage("Start http://" + website);
            commandQueue.AddMessage(message);
            return "Succesfully added: " + website + " to the queue";
        }

        [WebMethod]
        public string GetPageTitle(string site)
        {
            Uri uri = new UriBuilder(site).Uri;
            CloudTable urlTable = CreateTable("websitetable");
            TableQuery<UriEntity> query = new TableQuery<UriEntity>()
                .Where(TableQuery.CombineFilters(
                TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, uri.Host),
                TableOperators.And,
                TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, uri.AbsoluteUri))
                );
            string result = "";
            foreach (UriEntity entity in urlTable.ExecuteQuery(query))
            {
                result = entity.name;
                break;
            }
            return result;
        }

        [WebMethod]
        public string MachineCounters()
        {
            PerformanceCounter ramCounter = new PerformanceCounter("Process", "Working Set");
            PerformanceCounter cpuCounter = new PerformanceCounter("Process", "% Processor Time");
            while (true)
            {
                Thread.Sleep(500);
                double ram = ramCounter.NextValue();
                double cpu = cpuCounter.NextValue();
                return("RAM: " + (ram / 1024 / 1024) + " MB; CPU: " + (cpu) + " %");
            }
        }

        [WebMethod]
        public void numberOfURLsCrawled()
        {
        }

        [WebMethod]
        public void LastTenURLsCrawled()
        {
        }

        [WebMethod]
        public void SizeOfQueue()
        {
            CloudQueue queue = CreateQueue("websitequeue");
            queue.ApproximateMessageCount.ToString();
        }

        public void SizeOfIndex()
        {
        }

        public void Errors()
        {
        }

        public void ClearData()
        {
        }

        public void CrawlerStatus()
        {
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


