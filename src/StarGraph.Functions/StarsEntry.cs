using Microsoft.WindowsAzure.Storage.Table;
using System;

namespace StarGraph.Functions
{
    public class StarsEntry : TableEntity
    {
        public string RepoUser { get; private set; }
        public string RepoName { get; private set; }
        public int StarCount { get; private set; }

        public StarsEntry(string userName, string repoName, int starCount, DateTime timestamp)
        {
            PartitionKey = "StarRecord";
            Timestamp = timestamp;
            RowKey = Guid.NewGuid().ToString();
            RepoUser = userName;
            RepoName = repoName;
            StarCount = starCount;
        }

        public override string ToString() => $"[{Timestamp}]: {RepoUser}/{RepoName} has {StarCount} stars";
    }
}
