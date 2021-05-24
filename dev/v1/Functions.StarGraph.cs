using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System;

#pragma warning disable IDE0060 // don't warn about myTimer being unused

namespace StarGraph.Functions
{
    public static class StarGraph
    {
        private const string TRIGGER_DAILY = "0 0 8 * * *"; // 8AM UTC (3AM EST)

        /// <summary>
        /// Logs the current star count and makes a new graph of all star counts over time
        /// </summary>
        [FunctionName("StarGraph")]
        public static void RunAsync([TimerTrigger(TRIGGER_DAILY)] TimerInfo myTimer, ILogger log)
        {
            log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");

            var db = new StarsTable(log);
            string userName = "scottplot";
            string repoName = "scottplot";

            // add the latest star count to the database
            string gitHubToken = Environment.GetEnvironmentVariable("GitHubToken", EnvironmentVariableTarget.Process);
            (var _, string json) = GitHubAPI.RequestRepo(userName, repoName, gitHubToken);
            int stars = GitHubAPI.TotalStars(json);
            db.Insert(userName, repoName, stars, DateTime.UtcNow).GetAwaiter().GetResult();

            // plot historical star count as an image and upload it
            var bmp = db.GetGraphBitmap();
            db.UploadImage(bmp).GetAwaiter().GetResult();
        }
    }
}