using System;
using System.Collections.Generic;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Table;

namespace StarGraph.Functions
{
    public class StarsTable
    {
        private const string TABLE_REFERENCE = "githubStars";
        private readonly CloudTable Table;
        private readonly ILogger Log;

        public StarsTable(ILogger log)
        {
            Log = log;
            string connectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage", EnvironmentVariableTarget.Process);
            CloudStorageAccount cloudStorageAccount = CloudStorageAccount.Parse(connectionString);
            CloudTableClient tableClient = cloudStorageAccount.CreateCloudTableClient();
            Table = tableClient.GetTableReference(TABLE_REFERENCE);
        }

        public void Insert(string user, string repo, int count, DateTime timestamp)
        {
            StarsEntry entry = new StarsEntry(user, repo, count, timestamp);
            TableOperation operation = TableOperation.InsertOrMerge(entry);
            Table.ExecuteAsync(operation);
            Log.LogInformation($"Inserted: {entry}");
        }
    }
}
