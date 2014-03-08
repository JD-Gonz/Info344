using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkerRole
{
    public class ResultEntity : TableEntity
    {
        public ResultEntity(string state, string lastTenCrawled, int numOfUrlsCrawled, int tableSize, string counters)
        {
            this.PartitionKey = "Result";
            this.RowKey = "Result";
            this.State = state;
            this.LastTen = lastTenCrawled;
            this.NumOfUrls = numOfUrlsCrawled;
            this.TableSize = tableSize;
            this.Counters = counters;
        }

        public ResultEntity() { }
        public string State { get; set; }
        public string LastTen { get; set; }
        public string Counters { get; set; }
        public int NumOfUrls { get; set; }
        public int TableSize { get; set; }
    }
}
