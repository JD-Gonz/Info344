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
        public UriEntity(string name, string url, string link, string date)
        {
            this.PartitionKey = name;
            this.RowKey = url;
            this.Date = date;
            this.Link = link;
        }
        public UriEntity() { }
        public string Link { get; set; }
        public string Date { get; set; }
    }
}
