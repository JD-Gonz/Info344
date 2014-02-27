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
        private CloudQueue webQueue;
        private CloudQueue commandQueue;
        private CloudTable webTable;
        private CloudTable resultsTable;
        private CloudTable errorTable;

        public override void Run()
        {
            // This is a sample worker implementation. Replace with your logic.
            Trace.TraceInformation("WorkerRole1 entry point called", "Information");


            
            while (true)
            {
                Thread.Sleep(500);
                Trace.TraceInformation("Working", "Information");
                CloudQueueMessage command = commandQueue.GetMessage();
                if (command == null)
                {
                    CloudQueueMessage url = webQueue.GetMessage();
                    if (url != null) 
                    {
                        state = "Crawling";
                        webQueue.DeleteMessage(url);
                        Uri website = new UriBuilder(url.AsString.Replace("www.", "")).Uri;
                        if (website.ToString().Contains("sitemaps"))
                        {
                            addToQueue(crawler.startCrawling(website));
                        }
                        else
                        {
                            UriEntity link = new UriEntity(website.Host, HttpUtility.UrlEncode(website.AbsoluteUri),
                                                           crawler.getTitle(website), crawler.getDate(website));

                                TableOperation insert = TableOperation.InsertOrReplace(link);
                                webTable.Execute(insert);
                                sitesCrawled++;

                            if (link.Name == null && link.Date == null)
                            {
                                ErrorEntity result = new ErrorEntity(website.AbsolutePath, "ERROR 408: Request Timed-Out uptaining an attribute from: " + website);
                                TableOperation update = TableOperation.InsertOrReplace(result);
                                errorTable.Execute(update);
                            }
                            addToQueue(crawler.startCrawling(website));
                        }
                    }
                    if (state != "Stopped/Data Cleared")
                    {
                        ResultEntity result = new ResultEntity(state, crawler.getLastUrls(), crawler.getTableSize(), sitesCrawled);
                        TableOperation update = TableOperation.InsertOrReplace(result);
                        resultsTable.Execute(update);
                    }
                }
                else if (command != null)
                {
                    state = "Idle";
                    commandQueue.DeleteMessage(command);
                    string process = command.AsString;
                    if (process.StartsWith("Start"))
                    {
                        string[] pros = process.Split(' ');
                        startCrawling(pros[1]);
                    }
                    else if (process.StartsWith("Stop"))
                        stopCrawling();
                }
                else if (state.Equals("Stopping"))
                {
                    state = "Stopped";
                }
            }
        }

        public override bool OnStart()
        {
            // Set the maximum number of concurrent connections 
            ServicePointManager.DefaultConnectionLimit = 12;

            // For information on handling configuration changes
            // see the MSDN topic at http://go.microsoft.com/fwlink/?LinkId=166357.

            state = "Loading";
            crawler = new Crawler();
            webQueue = CreateQueue("websitequeue");
            commandQueue = CreateQueue("commandqueue");
            webTable = CreateTable("websitetable");
            resultsTable = CreateTable("resulttable");
            errorTable = CreateTable("errortable");
            return base.OnStart();
        }

        private void startCrawling(string website)
        {
            state = "Idle";
            crawler = new Crawler();
            webQueue = CreateQueue("websitequeue");
            commandQueue = CreateQueue("commandqueue");
            webTable = CreateTable("websitetable");
            resultsTable = CreateTable("resulttable");
            errorTable = CreateTable("errortable");
            Uri link = new UriBuilder(website).Uri;
            ResultEntity result = new ResultEntity(state, crawler.getLastUrls(), crawler.getTableSize(), sitesCrawled);
            TableOperation update = TableOperation.InsertOrReplace(result);
            resultsTable.Execute(update);
            CloudQueueMessage message = new CloudQueueMessage(link.ToString());
            webQueue.AddMessage(message);   
        }
    
        private void stopCrawling()
        {
            webQueue.Clear();
            crawler = null;
            commandQueue.Clear();
            webTable.DeleteIfExists();
            resultsTable.DeleteIfExists();
            errorTable.DeleteIfExists();
            state = "Stopped/Data Cleared";
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

        private void addToQueue(List<string> newLinks)
        {
            foreach (string website in newLinks)
            {
                if (website.StartsWith("ERROR"))
                {
                    ErrorEntity result = new ErrorEntity(website, website);
                    TableOperation update = TableOperation.InsertOrReplace(result);
                    errorTable.Execute(update);
                }
                else
                {
                    Uri link = new UriBuilder(website).Uri;
                    CloudQueueMessage message = new CloudQueueMessage(link.ToString());
                    webQueue.AddMessage(message);
                }
            }
        }
    }
}