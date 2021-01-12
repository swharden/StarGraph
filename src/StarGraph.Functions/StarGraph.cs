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
    public static class StarGraph
    {
        private const string TRIGGER_DAILY = "0 0 8 * * *"; // 8AM UTC (3AM EST)
        private const string TABLE_REFERENCE = "githubStars";

        [FunctionName("StarGraph")]
        public static void RunAsync([TimerTrigger(TRIGGER_DAILY)] TimerInfo myTimer, ILogger log)
        {
            log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");

            // create the table entry
            int starCount = 123456;
            DateTime timeStamp = DateTime.UtcNow;
            string userName = "scottplotName";
            string repoName = "scottplotRepo";
            StarsEntry entry = new StarsEntry(userName, repoName, starCount, timeStamp);

            // connect to tabular storage and add the entry to the table
            string connectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage", EnvironmentVariableTarget.Process);
            CloudStorageAccount cloudStorageAccount = CloudStorageAccount.Parse(connectionString);
            CloudTableClient tableClient = cloudStorageAccount.CreateCloudTableClient();
            CloudTable table = tableClient.GetTableReference(TABLE_REFERENCE);

            // add each entry
            TableOperation operation = TableOperation.InsertOrMerge(entry);
            table.ExecuteAsync(operation);
            log.LogInformation($"Inserted: {entry}");
        }
    }
}