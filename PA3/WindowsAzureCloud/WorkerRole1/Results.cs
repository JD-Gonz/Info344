using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkerRole1
{
    public class Results : TableEntity
    {
        public Results(string domainName, string name)
        {
            this.PartitionKey = domainName;
            this.RowKey = name;
        }

        public Results() { }
        public string lastTenCrawled { get; set; }
        public int numOfUrlsCrawled { get; set; }
        public string queueSize { get; set; }
        public int tableSize { get; set; }
    }
}
