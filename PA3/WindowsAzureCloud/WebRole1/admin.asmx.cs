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
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(
                ConfigurationManager.AppSettings["StorageConnectionString"]);
            CloudQueueClient queueClient = storageAccount.CreateCloudQueueClient();
            CloudQueue queue = queueClient.GetQueueReference("websitequeue");
            queue.CreateIfNotExists();

            CloudQueueMessage message = new CloudQueueMessage("http://" + website);
            queue.AddMessage(message);

            return "Succesfully added: " + website + " to the queue";
        }

        [WebMethod]
        public void GetPageTitles()
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(
                ConfigurationManager.AppSettings["StorageConnectionString"]);
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
            CloudTable table = tableClient.GetTableReference("websitetable");

        }
    }
}

/*
    Process p = *something*;
    PerformanceCounter ramCounter = new PerformanceCounter("Process", "Working Set", p.ProcessName);
    PerformanceCounter cpuCounter = new PerformanceCounter("Process", "% Processor Time", p.ProcessName);
    while (true)
    {
        Thread.Sleep(500);
        double ram = ramCounter.NextValue();
        double cpu = cpuCounter.NextValue();
        Console.WriteLine("RAM: " + (ram / 1024 / 1024) + " MB; CPU: " + (cpu) + " %");
    }
 */
