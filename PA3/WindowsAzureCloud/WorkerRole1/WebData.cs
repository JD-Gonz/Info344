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
        public WebData(string domainName, string url)
        {
            this.PartitionKey = domainName;
            this.RowKey = url;
        }

        public WebData() { }
        public string name { get; set; }
        public string date { get; set; }
    }
}
