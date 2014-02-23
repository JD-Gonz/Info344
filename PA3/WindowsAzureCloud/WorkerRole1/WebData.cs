using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkerRole1
{
    public class WebData : TableEntity
    {
        public WebData(string domainName, string name)
        {
            this.PartitionKey = domainName;
            this.RowKey = name;
        }

        public WebData() { }
        public string url { get; set; }
        public DateTime date { get; set; }
    }
}
