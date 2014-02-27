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
        public ErrorEntity(string description)
        {
            this.PartitionKey = "Error";
            this.RowKey = "Error";
            this.Description = description;
        }

        public ErrorEntity() { }
        public string Description { get; set; }
    }
}
