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
            this.Name = name;
            this.Date = date;
        }
        public UriEntity() { }
        public string Name { get; set; }
        public DateTime Date { get; set; }
    }
}
