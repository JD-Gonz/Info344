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
            this.PartitionKey = "Result";
            this.RowKey = "Result";
            this.State = state;
            this.LastTen = lastTenCrawled;
            this.NumOfUrls = numOfUrlsCrawled;
            this.TableSize = tableSize;
        }

        public ResultEntity() { }
        public string State { get; set; }
        public string LastTen { get; set; }
        public int NumOfUrls { get; set; }
        public int TableSize { get; set; }
    }
}
