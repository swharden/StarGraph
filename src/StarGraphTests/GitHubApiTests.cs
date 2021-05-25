using Microsoft.Extensions.Configuration;
using NUnit.Framework;
using System;
using System.IO;
using System.Reflection;

namespace StarGraphTests
{
    public class GitHubApiTests
    {
        [Test]
        public void Test_StargazersJson_OneCanBeParsed()
        {
            string json = SampleData.GetStargazerPageJson();
            StarGraph.StarRecord[] records = StarGraph.GitHubJSON.StarRecordsFromPage(json);
            Assert.IsNotEmpty(records);
            Assert.AreEqual(30, records.Length);
            foreach (var record in records)
                Console.WriteLine(record);
        }

        [Test]
        public void Test_StargazersJson_AllCanBeParsed()
        {
            StarGraph.StarRecord[] records = SampleData.GetAllStargazers();
            Assert.AreEqual(839, records.Length);
        }

        [Test]
        [Ignore("Live HTTP tests disabled")]
        public void Test_Http_ReturnsJson()
        {
            (string json, int pages, int remaining) = StarGraph.GitHubAPI.RequestStargazerJson("scottplot", "scottplot");
            Console.WriteLine($"Requests remaining: {remaining}");
            Console.WriteLine($"Stargazer page count: {pages}");

            Assert.IsNotNull(json);
            Assert.That(json.Contains("starred_at"));

            Assert.GreaterOrEqual(pages, 1);

            StarGraph.StarRecord[] records = StarGraph.GitHubJSON.StarRecordsFromPage(json);
            Assert.IsNotEmpty(records);
            Assert.AreEqual(30, records.Length);
            foreach (var record in records)
                Console.WriteLine(record);
        }

        [Test]
        [Ignore("Live HTTP tests disabled")]
        public void Test_Http_DownloadPages()
        {
            // TODO: use a GitHub auth token to avoid rate limiting.
            (_, int pageCount, int remaining) = StarGraph.GitHubAPI.RequestStargazerJson("scottplot", "scottplot");
            Console.WriteLine($"{pageCount} pages and {remaining} remaining requests");
            if (remaining <= pageCount)
                throw new InvalidOperationException("not enough remaining requests to proceed");

            for (int i = 0; i < pageCount; i++)
            {
                int page = i + 1;
                string filename = $"stargazers-page-{page:0000}.json";
                Console.WriteLine(filename);

                (string json, _, _) = StarGraph.GitHubAPI.RequestStargazerJson("scottplot", "scottplot", page);
                File.WriteAllText(filename, json);
            }
        }

        [Test]
        [Ignore("Live HTTP tests disabled")]
        public void Test_Http_GitHubToken()
        {
            var appConfig = new ConfigurationBuilder().AddUserSecrets<GitHubApiTests>().Build();
            string token = appConfig["github-token-starchart-readuser"];
            if (string.IsNullOrWhiteSpace(token))
                throw new InvalidOperationException("token not found");

            (_, _, int remaining) = StarGraph.GitHubAPI.RequestStargazerJson("scottplot", "scottplot", token: token);
            Console.WriteLine($"Remaining API requests: {remaining}");
            Assert.That(remaining > 100, "access token was wasn't accepted");
        }
    }
}