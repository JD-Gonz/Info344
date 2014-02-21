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
        public WebData(string row, string title)
        {
            this.PartitionKey = row;
            this.RowKey = title;
        }

        public WebData() { }
        public string url { get; set; }
        public string date { get; set; }
    }
}
