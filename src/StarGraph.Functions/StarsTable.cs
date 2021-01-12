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
        private readonly CloudTableClient TableClient;
        private readonly ILogger Logger;

        private void Log(string message)
        {
            if (Logger is null)
                Console.WriteLine(message);
            else
                Logger.LogInformation(message);
        }

        public StarsTable(ILogger log)
        {
            Logger = log;
            string connectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage", EnvironmentVariableTarget.Process);
            CloudStorageAccount cloudStorageAccount = CloudStorageAccount.Parse(connectionString);
            TableClient = cloudStorageAccount.CreateCloudTableClient();
            Table = TableClient.GetTableReference(TABLE_REFERENCE);
            Log($"Connected to table {TABLE_REFERENCE}");
        }

        // this is just for development
        public StarsTable(string connectionString)
        {
            CloudStorageAccount cloudStorageAccount = CloudStorageAccount.Parse(connectionString);
            CloudTableClient tableClient = cloudStorageAccount.CreateCloudTableClient();
            Table = tableClient.GetTableReference(TABLE_REFERENCE);
        }

        public void Insert(string user, string repo, int count, DateTime timestamp)
        {
            StarsEntry entry = new StarsEntry(user, repo, count, timestamp);
            TableOperation operation = TableOperation.InsertOrMerge(entry);
            Table.ExecuteAsync(operation);
            Log($"Inserted: {entry}");
        }
    }
}
