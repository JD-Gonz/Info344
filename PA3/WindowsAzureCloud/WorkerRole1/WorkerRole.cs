using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Diagnostics;
using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using Microsoft.WindowsAzure.Storage.Table;
using System.Web;

namespace WorkerRole1
{
    public class WorkerRole : RoleEntryPoint
    {
        private Crawler crawler;
        private string state;
        private int sitesCrawled;

        public override void Run()
        {
            // This is a sample worker implementation. Replace with your logic.
            Trace.TraceInformation("WorkerRole1 entry point called", "Information");

            CloudQueue webQueue = CreateQueue("websitequeue");
            CloudQueue commandQueue = CreateQueue("commandqueue");
            CloudTable webTable = CreateTable("websitetable");
            CloudTable resultsTable = CreateTable("resulttable");

            while (true)
            {
                Thread.Sleep(500);
                Trace.TraceInformation("Working", "Information");

                CloudQueueMessage command = commandQueue.GetMessage();
                if (command == null && state.Equals("Crawling"))
                {
                    state = "Crawling";
                    CloudQueueMessage url = webQueue.GetMessage();
                    if (url != null) 
                    {
                        webQueue.DeleteMessage(url);
                        Uri website = new UriBuilder(url.AsString).Uri;
                        if (website.ToString().Contains("sitemaps"))
                        {
                            addToQueue(crawler.startCrawling(website));
                        }
                        else
                        {
                            UriEntity link = new UriEntity(website.Host, HttpUtility.UrlEncode(website.AbsoluteUri),
                                                           crawler.getTitle(website), crawler.getDate(website));
                            addToQueue(crawler.startCrawling(website));
                            TableOperation insert = TableOperation.InsertOrReplace(link);
                            webTable.Execute(insert);
                            sitesCrawled++;
                        }
                        ResultEntity result = new ResultEntity("results", state, crawler.getLastUrls(), crawler.getTableSize(), sitesCrawled);
                        TableOperation update = TableOperation.InsertOrReplace(result);
                        resultsTable.Execute(update);
                    }
                }
                else if (command != null)
                {

                    commandQueue.DeleteMessage(command);
                    string process = command.AsString;
                    if (process.StartsWith("Start"))
                    {
                        string[] pros = process.Split(' ');
                        startCrawling(pros[1]);
                    }
                    else if (process.StartsWith("Stop"))
                        stopCrawling();
                    else if (process.StartsWith("Clear"))
                        clearAll();
                }
                else if (state.Equals("Stopping"))
                {
                    state = "Stopped";
                    ResultEntity result = new ResultEntity("results", state, crawler.getLastUrls(), crawler.getTableSize(), sitesCrawled);
                    TableOperation update = TableOperation.InsertOrReplace(result);
                    resultsTable.Execute(update);
                }
            }
        }

        public override bool OnStart()
        {
            // Set the maximum number of concurrent connections 
            ServicePointManager.DefaultConnectionLimit = 12;

            // For information on handling configuration changes
            // see the MSDN topic at http://go.microsoft.com/fwlink/?LinkId=166357.
            state = "Loading/Idle";
            crawler = new Crawler();
            return base.OnStart();
        }

        private void startCrawling(string website)
        {
            Uri link = new UriBuilder(website).Uri;
            CloudQueue queue = CreateQueue("websitequeue");
            CloudTable resultsTable = CreateTable("resulttable");
            state = "Crawling";
            ResultEntity result = new ResultEntity("results", state, crawler.getLastUrls(), crawler.getTableSize(), sitesCrawled);
            TableOperation update = TableOperation.InsertOrReplace(result);
            resultsTable.Execute(update);
            CloudQueueMessage message = new CloudQueueMessage(link.ToString());
            queue.AddMessage(message);   
        }
    
        private void stopCrawling()
        {
            CloudTable resultsTable = CreateTable("resulttable");
            CloudQueue queue = CreateQueue("websitequeue");
            state = "Stopping";
            ResultEntity result = new ResultEntity("results", state, crawler.getLastUrls(), crawler.getTableSize(), sitesCrawled);
            TableOperation update = TableOperation.InsertOrReplace(result);
            resultsTable.Execute(update);
        }

        private void clearAll()
        { 
            CloudQueue webQueue = CreateQueue("websitequeue");
            CloudTable webTable = CreateTable("websitetable");
            CloudTable resultsTable = CreateTable("resulttable");
            CloudTable errorTable = CreateTable("errortable");
            webQueue.Clear();
            webTable.DeleteIfExists();
            resultsTable.DeleteIfExists();
            errorTable.DeleteIfExists();
            state = "Data Cleared";

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

        private void addToQueue(List<Uri> newLinks)
        {
            CloudQueue queue = CreateQueue("websitequeue");

            foreach (Uri link in newLinks)
            {
                CloudQueueMessage message = new CloudQueueMessage(link.ToString());
                queue.AddMessage(message);
            }
        }
    }
}
