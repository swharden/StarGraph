using Microsoft.WindowsAzure.Storage.Table;
using System;

namespace StarGraph.Functions
{
    public class StarsEntry : TableEntity
    {
        public string RepoUser { get; set; }
        public string RepoName { get; set; }
        public int StarCount { get; set; }
        public DateTime Date { get; set; }

        public StarsEntry()
        {

        }

        public StarsEntry(string userName, string repoName, int starCount, DateTime timestamp)
        {
            PartitionKey = "StarRecord";
            Date = timestamp.Date;
            RowKey = Guid.NewGuid().ToString();
            RepoUser = userName;
            RepoName = repoName;
            StarCount = starCount;
        }

        public override string ToString() => $"[{Date}]: {RepoUser}/{RepoName} has {StarCount} stars";
    }
}
