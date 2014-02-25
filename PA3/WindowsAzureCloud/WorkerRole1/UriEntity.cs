using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkerRole1
{
    public class UriEntity : TableEntity
    {
        public UriEntity(string domainName, string url, string name, DateTime date)
        {
            this.PartitionKey = domainName;
            this.RowKey = url;
            this.name = name;
            this.date = date;
        }
        public UriEntity() { }
        public string name { get; set; }
        public DateTime date { get; set; }
    }
}
