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

namespace WorkerRole
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
        private PerformanceCounter ramCounter;
        private PerformanceCounter cpuCounter;
        private float ram;
        private float cpu;

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
                            DateTime siteDate;
                            if (DateTime.TryParse(crawler.getDate(website), out siteDate))
                            {
                                if ((siteDate > DateTime.Now.AddMonths(-3)))
                                {
                                    addToQueue(crawler.startCrawling(website));
                                }
                            }
                        }
                        else
                        {
                            
                            string fullTitle = preProccess(crawler.getTitle(website));
                            string[] titles = fullTitle.Split(' ');
                            foreach (string title in titles)
                            {
                                UriEntity link = new UriEntity(title, HttpUtility.UrlEncode(website.AbsoluteUri), crawler.getDate(website));
                                DateTime siteDate;
                                if (DateTime.TryParse(link.Date, out siteDate))
                                {
                                    if ((siteDate >= DateTime.Now.AddMonths(-3)))
                                    {
                                        TableOperation insert = TableOperation.InsertOrReplace(link);
                                        webTable.Execute(insert);
                                        addToQueue(crawler.startCrawling(website));
                                        sitesCrawled++;
                                    }
                                }
                            }
                        }
                    }
                    if (state != "Stopped/Data Cleared")
                    {
                        ram = ramCounter.NextValue();
                        cpu = cpuCounter.NextValue();
                        ResultEntity result = new ResultEntity(state, crawler.getLastUrls(), crawler.getTableSize(), sitesCrawled,
                                                               "RAM: " + (ram) + " MB | CPU: " + (cpu) + " %");
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
            ramCounter = new PerformanceCounter("Memory", "Available MBytes");
            cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
            ram = ramCounter.NextValue();
            cpu = cpuCounter.NextValue();
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
            ram = ramCounter.NextValue();
            cpu = cpuCounter.NextValue();
            ResultEntity result = new ResultEntity(state, crawler.getLastUrls(), crawler.getTableSize(), sitesCrawled,
                                                   "RAM: " + (ram) + " MB | CPU: " + (cpu) + " %");
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
        
        private string preProccess (string title)
        {
            string reconstructedline = "";
            foreach (char letter in title)
            {
                if (letter == '_')
                    reconstructedline += " ";
                else if (letter == 32 || letter == 46 || (letter > 65 && letter < 90) || (letter > 97 && letter < 122))
                {
                    reconstructedline += letter;
                }   
            }
            return reconstructedline;
        }
    }
}
