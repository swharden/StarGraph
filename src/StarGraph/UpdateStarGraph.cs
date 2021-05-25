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
            // retrieve secrets
            string storageConnectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage", EnvironmentVariableTarget.Process);
            string githubToken = Environment.GetEnvironmentVariable("github-token-starchart-readuser", EnvironmentVariableTarget.Process);

            StarRecordManager stars = new("scottplot", "scottplot", githubToken);

            // read known stars from JSON in blob storage
            BlobContainerClient container = new(storageConnectionString, "$web");
            BlobClient recordJsonBlob = container.GetBlobClient("starRecords.json");
            if (!recordJsonBlob.Exists())
                throw new InvalidOperationException("json file not found");

            using MemoryStream streamJson = new();
            recordJsonBlob.DownloadTo(streamJson);
            string json = Encoding.UTF8.GetString(streamJson.ToArray());
            stars.TryAddFromJson(json);
            Console.WriteLine($"loaded {stars.Count} stars from stored JSON");

            // hit the GitHub API to learn of new stargazers


            // save the full list back to blob storage

            // plot the result and save it in blob storage
            byte[] imageBytes = Plot.GetPlotBytes(stars.GetSortedRecords());
            using var streamImage = new MemoryStream(imageBytes);
            BlobClient blobPlot = container.GetBlobClient("scottplot-stars.png");
            blobPlot.Upload(streamImage, overwrite: true);
            BlobHttpHeaders pngHeaders = new() { ContentType = "image/png", ContentLanguage = "en-us", };
            blobPlot.SetHttpHeaders(pngHeaders);
        }
    }
}
