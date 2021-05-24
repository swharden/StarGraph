using NUnit.Framework;
using System;
using System.IO;
using System.Reflection;

namespace StarGraphTests
{
    public class GitHubApiTests
    {
        [Test]
        public void Test_StargazersJson_CanBeParsed()
        {
            string json = SampleData.GetStargazerPageJson();
            StarGraph.StarRecord[] records = StarGraph.GitHubJSON.StarRecordsFromPage(json);
            Assert.IsNotEmpty(records);
            Assert.AreEqual(30, records.Length);
            foreach (var record in records)
                Console.WriteLine(record);
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
    }
}