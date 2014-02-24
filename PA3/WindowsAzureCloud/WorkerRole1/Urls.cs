using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkerRole1
{
    public class Urls : TableEntity
    {
        public Urls(string domainName, string url)
        {
            this.PartitionKey = domainName;
            this.RowKey = url;
        }

        public Urls() { }
        public string localPath { get; set; }
        public string name { get; set; }
        public DateTime date { get; set; }
    }
}
