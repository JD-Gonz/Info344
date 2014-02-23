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

namespace WorkerRole1
{
    public class WorkerRole : RoleEntryPoint
    {
        private Crawler crawler;
        public override void Run()
        {
            // This is a sample worker implementation. Replace with your logic.
            Trace.TraceInformation("WorkerRole1 entry point called", "Information");


            
            crawler = new Crawler();
            while (true)
            {
                Thread.Sleep(10000);
                Trace.TraceInformation("Working", "Information");

                CloudStorageAccount storageAccount = CloudStorageAccount.Parse(
                    CloudConfigurationManager.GetSetting("StorageConnectionString"));
                CloudQueueClient queueClient = storageAccount.CreateCloudQueueClient();
                CloudQueue queue = queueClient.GetQueueReference("websitequeue");
                queue.CreateIfNotExists();
                CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
                CloudTable table = tableClient.GetTableReference("websitetable");
                table.CreateIfNotExists();
                while (true)
                {
                    CloudQueueMessage url = queue.GetMessage(TimeSpan.FromMinutes(5));
                    Uri website;
                    if (url != null && Uri.TryCreate(url.AsString, UriKind.Absolute, out website) && website.Scheme == Uri.UriSchemeHttp)
                    {
                        queue.DeleteMessage(url);
                        if (website.ToString().Contains("sitemaps"))
                        {
                            addToQueue(crawler.startCrawling(website));
                        }
                        else
                        {
                            WebData link = new WebData(website.Host, crawler.getTitle(website));
                            link.url = website.PathAndQuery;
                            link.date = crawler.getDate(website);
                            addToQueue(crawler.startCrawling(website));
                            TableOperation insert = TableOperation.InsertOrReplace(link);
                            table.Execute(insert);
                        }
                    }
                }
            }
        }

        public override bool OnStart()
        {
            // Set the maximum number of concurrent connections 
            ServicePointManager.DefaultConnectionLimit = 12;

            // For information on handling configuration changes
            // see the MSDN topic at http://go.microsoft.com/fwlink/?LinkId=166357.

            return base.OnStart();
        }

        public void addToQueue(List<Uri> newLinks)
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(
                CloudConfigurationManager.GetSetting("StorageConnectionString"));
            CloudQueueClient queueClient = storageAccount.CreateCloudQueueClient();
            CloudQueue queue = queueClient.GetQueueReference("websitequeue");
            queue.CreateIfNotExists();
            
            foreach (Uri link in newLinks)
            {
                CloudQueueMessage message = new CloudQueueMessage(link.ToString());
                queue.AddMessage(message);
            }
        }
    }
}
