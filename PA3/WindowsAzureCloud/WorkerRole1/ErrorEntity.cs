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
        public ErrorEntity(string name, string description, string url)
        {
            this.PartitionKey = "Error";
            this.RowKey = name;
            this.description = description;
            this.url = url;
        }

        public ErrorEntity() { }
        public string description { get; set; }
        public string url { get; set; }
    }
}
