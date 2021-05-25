using System;
using System.IO;
using System.Text;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace StarGraph
{
    public static class UpdateStarGraph
    {
        [FunctionName("UpdateStarGraph")]
        public static void Run([TimerTrigger("0 0 * * * *")] TimerInfo myTimer, ILogger log)
        {
            log.LogInformation($"updating starplot at {DateTime.UtcNow}");

            string storageConnectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage", EnvironmentVariableTarget.Process);
            BlobContainerClient container = new(storageConnectionString, "$web");
            BlobClient recordJsonBlob = container.GetBlobClient("starRecords.json");
            if (!recordJsonBlob.Exists())
                throw new InvalidOperationException("json file not found");

            using MemoryStream streamJson = new();
            recordJsonBlob.DownloadTo(streamJson);
            string json = Encoding.UTF8.GetString(streamJson.ToArray());
            StarRecord[] records = GitHubJSON.RecordsFromJson(json);

            foreach (var record in records)
                Console.WriteLine(record);

            byte[] imageBytes = Plot.BmpToBytes(Plot.MakePlot(records));
            using var streamImage = new MemoryStream(imageBytes);
            BlobClient blobPlot = container.GetBlobClient("scottplot-stars.png");
            blobPlot.Upload(streamImage, overwrite: true);

            BlobHttpHeaders pngHeaders = new() { ContentType = "image/png", ContentLanguage = "en-us", };
            blobPlot.SetHttpHeaders(pngHeaders);
        }
    }
}
