using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkerRole1
{
    public class ResultEntity : TableEntity
    {
        public ResultEntity(string state, string lastTenCrawled, int numOfUrlsCrawled, int tableSize)
        {
            this.PartitionKey = "results";
            this.RowKey = "update";
            this.state = state;
            this.lastTenCrawled = lastTenCrawled;
            this.numOfUrlsCrawled = numOfUrlsCrawled;
            this.tableSize = tableSize;
        }

        public ResultEntity() { }
        public string state { get; set; }
        public string lastTenCrawled { get; set; }
        public int numOfUrlsCrawled { get; set; }
        public int tableSize { get; set; }
    }
}
