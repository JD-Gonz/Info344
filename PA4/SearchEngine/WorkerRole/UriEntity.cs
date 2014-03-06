using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkerRole
{
    public class UriEntity : TableEntity
    {
        public UriEntity(string name, string url, string date)
        {
            this.PartitionKey = name;
            this.RowKey = url;
            this.Date = date;
        }
        public UriEntity() { }
        public string Date { get; set; }
    }
}
