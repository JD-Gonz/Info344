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
        private string domain;

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
                Thread.Sleep(5000);
                Trace.TraceInformation("Working", "Information");

                CloudQueueMessage command = commandQueue.GetMessage();
                if (command == null && state.Equals("Crawling"))
                {
                    state = "Crawling";
                    CloudQueueMessage url = webQueue.GetMessage();
                    Uri website;
                    if (url != null && Uri.TryCreate(url.AsString, UriKind.Absolute, out website) && website.Scheme == Uri.UriSchemeHttp)
                    {
                        webQueue.DeleteMessage(url);
                        if (website.ToString().Contains("sitemaps"))
                        {
                            addToQueue(crawler.startCrawling(website));
                        }
                        else
                        {
                            Urls link = new Urls(website.Host, HttpUtility.UrlEncode(website.AbsoluteUri));
                            link.name = crawler.getTitle(website);
                            link.localPath = website.PathAndQuery;
                            link.date = crawler.getDate(website);
                            addToQueue(crawler.startCrawling(website));
                            TableOperation insert = TableOperation.InsertOrReplace(link);
                            webTable.Execute(insert);
                            sitesCrawled++;
                        }
                        Results result = new Results("results", state);
                        result.lastTenCrawled = crawler.getLastUrls();
                        result.numOfUrlsCrawled = crawler.getTableSize();
                        result.queueSize = webQueue.ApproximateMessageCount.ToString();
                        result.tableSize = sitesCrawled;
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
                    if (process.StartsWith("Stop"))
                        stopCrawling();
                    if (process.StartsWith("Clear"))
                        clearAll();
                }
                else if (state.Equals("Stopping"))
                {
                    state = "Stopped";
                    Results result = new Results("results", state);
                    result.lastTenCrawled = crawler.getLastUrls();
                    result.numOfUrlsCrawled = crawler.getTableSize();
                    result.queueSize = webQueue.ApproximateMessageCount.ToString();
                    result.tableSize = sitesCrawled;
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

        private void startCrawling(string website)
        {
            Uri validLink;
            if (Uri.TryCreate(website, UriKind.Absolute, out validLink) && validLink.Scheme == Uri.UriSchemeHttp)
            {
                
                CloudQueue queue = CreateQueue("websitequeue");
                CloudTable resultsTable = CreateTable("resulttable");
                state = "Crawling";
                Results result = new Results("results", state);
                result.lastTenCrawled = crawler.getLastUrls();
                result.numOfUrlsCrawled = crawler.getTableSize();
                result.queueSize = queue.ApproximateMessageCount.ToString();
                result.tableSize = sitesCrawled;
                TableOperation update = TableOperation.InsertOrReplace(result);
                resultsTable.Execute(update);
                addToQueue(crawler.startCrawling(validLink));
            }
            else
            {
                CloudTable errorTable = CreateTable("errortable");
                ErrorEntity error = new ErrorEntity("error", "Invalid URL Was Provided");
                error.discription = "the provided URL was Invalid Please Provide A Real URL";
                error.url = website;
                TableOperation update = TableOperation.InsertOrReplace(error);
                errorTable.Execute(update);

            }
                
                
            
        }
    
        private void stopCrawling()
        {
            CloudTable resultsTable = CreateTable("resulttable");
            CloudQueue queue = CreateQueue("websitequeue");
            state = "Stopping";
            Results result = new Results("results", state);
            result.lastTenCrawled = crawler.getLastUrls();
            result.numOfUrlsCrawled = crawler.getTableSize();
            result.queueSize = queue.ApproximateMessageCount.ToString();
            result.tableSize = sitesCrawled;
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
    }
}
