using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Table;

namespace StarGraph.Functions
{
    public class StarsTable
    {
        private const string TABLE_REFERENCE = "githubStars";
        private const string TABLE_PARTITION_KEY = "StarRecord";
        private const string BLOB_REFERENCE = "$web";
        public readonly Dictionary<DateTime, int> StarsByDate = new Dictionary<DateTime, int>();

        private readonly CloudTable Table;
        private readonly CloudTableClient TableClient;
        private readonly CloudBlobClient BlobClient;
        private readonly CloudBlobContainer Blob;
        private readonly ILogger Logger;

        /// <summary>
        /// Conenct to the database from a local developer environment
        /// </summary>
        public StarsTable(string connectionString)
        {
            CloudStorageAccount cloudStorageAccount = CloudStorageAccount.Parse(connectionString);
            TableClient = cloudStorageAccount.CreateCloudTableClient();
            Table = TableClient.GetTableReference(TABLE_REFERENCE);
            BlobClient = cloudStorageAccount.CreateCloudBlobClient();
            Blob = BlobClient.GetContainerReference(BLOB_REFERENCE);
            Log($"Connected to table {TABLE_REFERENCE} and blob {BLOB_REFERENCE}");
        }

        /// <summary>
        /// Conenct to the database from a the cloud
        /// </summary>
        public StarsTable(ILogger log)
        {
            Logger = log;
            string connectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage", EnvironmentVariableTarget.Process);
            if (connectionString is null)
                throw new InvalidOperationException("null connection string");
            CloudStorageAccount cloudStorageAccount = CloudStorageAccount.Parse(connectionString);
            TableClient = cloudStorageAccount.CreateCloudTableClient();
            Table = TableClient.GetTableReference(TABLE_REFERENCE);
            BlobClient = cloudStorageAccount.CreateCloudBlobClient();
            Blob = BlobClient.GetContainerReference(BLOB_REFERENCE);
            Log($"Connected to table {TABLE_REFERENCE} and blob {BLOB_REFERENCE}");
        }

        private void Log(string message)
        {
            if (Logger is null)
                Console.WriteLine(message);
            else
                Logger.LogInformation(message);
        }

        /// <summary>
        /// Add an etry to the database
        /// </summary>
        public async Task Insert(string user, string repo, int count, DateTime timestamp)
        {
            StarsEntry entry = new StarsEntry(user, repo, count, timestamp);
            TableOperation operation = TableOperation.InsertOrMerge(entry);
            await Table.ExecuteAsync(operation);
            Log($"Inserted: {entry}");
        }

        /// <summary>
        /// Populate the StarsByDate dictionary (if it is empty)
        /// </summary>
        private async Task ReadAllRows()
        {
            if (StarsByDate.Count() > 0)
                return;

            var condition = TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, TABLE_PARTITION_KEY);
            var query = new TableQuery<StarsEntry>().Where(condition);

            TableContinuationToken token = null;
            do
            {
                TableQuerySegment<StarsEntry> resultSegment = await Table.ExecuteQuerySegmentedAsync(query, token);
                token = resultSegment.ContinuationToken;
                foreach (var result in resultSegment.Results)
                    StarsByDate[result.Date] = result.StarCount;
            }
            while (token != null);
        }

        /// <summary>
        /// Read the database to load star data and return a graph of the results
        /// </summary>
        /// <returns></returns>
        public System.Drawing.Bitmap GetGraphBitmap()
        {
            // populate table with data
            ReadAllRows().GetAwaiter().GetResult();
            var sortedStarsByDate = StarsByDate.OrderBy(x => x.Value);

            // convert data to double arrays
            List<double> dates = new List<double>();
            List<double> counts = new List<double>();
            foreach ((var k, var v) in sortedStarsByDate)
            {
                dates.Add(k.ToOADate());
                counts.Add(v);
            }

            // use ScottPlot to create the graph from the table data
            var plt = new ScottPlot.Plot(700, 400);
            plt.Title("ScottPlot Stars on GitHub");
            var scatter = plt.AddScatter(dates.ToArray(), counts.ToArray(), markerSize: 0, lineWidth: 2);
            plt.XAxis.DateTimeFormat(true);
            plt.YLabel("Total Stars");
            plt.YAxis.SetSizeLimit(min: 50);
            System.Drawing.Bitmap bmp = plt.Render();
            Log($"Generated graph with {dates.Count()} points");

            return bmp;
        }

        /// <summary>
        /// Save a Bitmap as a PNG in blob storage
        /// </summary>
        public async Task UploadImage(System.Drawing.Bitmap bmp, string fileName = "packagestats/scottplot-stars.png")
        {
            // prepare the bitmap as a memory stream
            using MemoryStream ms = new MemoryStream();
            bmp.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
            ms.Seek(0, SeekOrigin.Begin);

            // upload to the cloud
            CloudBlockBlob cloudBlockBlob = Blob.GetBlockBlobReference(fileName);
            cloudBlockBlob.Properties.ContentType = "image/png";
            await cloudBlockBlob.UploadFromStreamAsync(ms);
            Log($"Uploaded {fileName}");
        }
    }
}
