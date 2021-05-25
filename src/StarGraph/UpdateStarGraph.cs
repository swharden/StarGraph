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
            // connect to storage 
            string storageConnectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage", EnvironmentVariableTarget.Process);
            BlobContainerClient container = new(storageConnectionString, "$web");

            // prepare the star manager
            string githubToken = Environment.GetEnvironmentVariable("github-token-starchart-readuser", EnvironmentVariableTarget.Process);
            if (string.IsNullOrEmpty(githubToken))
                throw new InvalidOperationException("github token not found");
            StarRecordManager stars = new("scottplot", "scottplot", githubToken);

            // read known stars from JSON in blob storage
            StarRecord[] knownStars = LoadStarRecords(container);
            stars.TryAdd(knownStars);
            Console.WriteLine($"loaded {stars.Count} stars from existing database");

            // hit the GitHub API to learn of new stargazers
            stars.TryAddFromWeb();
            Console.WriteLine($"now aware of {stars.Count} stars after updating from GitHub");

            // save the full list back to blob storage
            SaveStarRecords(stars.GetSortedRecords(), container);

            // plot the result and save it in blob storage
            byte[] imageBytes = Plot.GetPlotBytes(stars.GetSortedRecords());
            using var streamImage = new MemoryStream(imageBytes);
            BlobClient blobPlot = container.GetBlobClient("scottplot-stars.png");
            blobPlot.Upload(streamImage, overwrite: true);
            BlobHttpHeaders pngHeaders = new() { ContentType = "image/png", ContentLanguage = "en-us", };
            blobPlot.SetHttpHeaders(pngHeaders);
        }

        /// <summary>
        /// Returns records stored in the database.
        /// </summary>
        private static StarRecord[] LoadStarRecords(BlobContainerClient container, string filename = "starRecords.json")
        {
            BlobClient blob = container.GetBlobClient(filename);
            if (!blob.Exists())
                throw new InvalidOperationException($"file not found: {filename}");
            using MemoryStream stream = new();
            blob.DownloadTo(stream);
            string json = Encoding.UTF8.GetString(stream.ToArray());
            StarRecord[] records = IO.RecordsFromJson(json);
            return records;
        }

        /// <summary>
        /// Saves records to the database.
        /// </summary>
        private static void SaveStarRecords(StarRecord[] records, BlobContainerClient container, string filename = "starRecords.json")
        {
            BlobClient blob = container.GetBlobClient(filename);
            string json = IO.RecordsToJson(records);
            byte[] bytes = Encoding.UTF8.GetBytes(json);
            using var stream = new MemoryStream(bytes, writable: false);
            blob.Upload(stream, overwrite: true);
            stream.Close();
        }
    }
}
