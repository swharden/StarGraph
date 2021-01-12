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

        [FunctionName("StarGraph")]
        public static void RunAsync([TimerTrigger(TRIGGER_DAILY)] TimerInfo myTimer, ILogger log)
        {
            log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");

            var stars = new StarsTable(log);
        }
    }
}