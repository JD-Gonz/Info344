using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkerRole1
{
    public class ErrorEntity : TableEntity
    {
        public ErrorEntity(string domainName, string name)
        {
            this.PartitionKey = domainName;
            this.RowKey = name;
        }

        public ErrorEntity() { }
        public string discription { get; set; }
        public string url { get; set; }
    }
}
