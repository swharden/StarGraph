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
            // load star records using the GitHub API and create a plot from them
            string gitHubToken = Environment.GetEnvironmentVariable("github-token-starchart-readuser", EnvironmentVariableTarget.Process);
            if (string.IsNullOrEmpty(gitHubToken))
                throw new InvalidOperationException("github token not found");
            StarRecord[] records = GitHubAPI.RequestAllStargazers("scottplot", "scottplot", gitHubToken);
            byte[] imageBytes = Plot.GetPlotBytes(records);

            // save the plot in blob storage
            string storageConnectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage", EnvironmentVariableTarget.Process);
            BlobContainerClient container = new(storageConnectionString, "$web");
            using var stream = new MemoryStream(imageBytes);
            BlobClient blobPlot = container.GetBlobClient("scottplot-stars.png");
            blobPlot.Upload(stream, overwrite: true);
            BlobHttpHeaders pngHeaders = new() { ContentType = "image/png", ContentLanguage = "en-us", };
            blobPlot.SetHttpHeaders(pngHeaders);
        }
    }
}
