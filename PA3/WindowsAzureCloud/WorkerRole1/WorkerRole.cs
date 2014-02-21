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
        public override void Run()
        {
            // This is a sample worker implementation. Replace with your logic.
            Trace.TraceInformation("WorkerRole1 entry point called", "Information");

            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(
                CloudConfigurationManager.GetSetting("StorageConnectionString"));
            CloudQueueClient queueClient = storageAccount.CreateCloudQueueClient();
            CloudQueue queue = queueClient.GetQueueReference("websitequeue");
            queue.CreateIfNotExists();

            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
            CloudTable table = tableClient.GetTableReference("websitetable");
            table.CreateIfNotExists();
            
            Pages pages = new Pages();
            int count = 0;
            while (true)
            {
                Thread.Sleep(10000);
                Trace.TraceInformation("Working", "Information");

                CloudQueueMessage url = queue.GetMessage(TimeSpan.FromMinutes(5));
                if (url != null)
                {
                    
                    queue.DeleteMessage(url);
                    string website = url.AsString;
                    if (count < 1)
                    {
                        pages.setRoot(website);
                        count++;
                    }
                    pages.getRobots(website);
                    // Create a new customer entity.
                    WebData link = new WebData("row", pages.getTitle(website));
                    link.url = website;
                    link.date = pages.getDate(website);
                    List<string> newLinks = pages.listOfLinks(website);

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
    }
}
