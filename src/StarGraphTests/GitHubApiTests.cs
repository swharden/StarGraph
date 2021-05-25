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
            StarGraph.StarRecord[] records = StarGraph.GitHubAPI.StarRecordsFromPageJSON(json);
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
            (StarGraph.StarRecord[] records, int pages, int remaining) = StarGraph.GitHubAPI.RequestStargazersPage("scottplot", "scottplot");
            Console.WriteLine($"Requests remaining: {remaining}");
            Console.WriteLine($"Stargazer page count: {pages}");
            Assert.IsNotEmpty(records);
            Assert.AreEqual(30, records.Length);
            foreach (var record in records)
                Console.WriteLine(record);
        }

        [Test]
        [Ignore("Live HTTP tests disabled")]
        public void Test_Http_GitHubToken()
        {
            (_, _, int remaining) = StarGraph.GitHubAPI.RequestStargazersPage("scottplot", "scottplot", token: Authentication.GetGitHubAccessToken());
            Console.WriteLine($"Remaining API requests: {remaining}");
            Assert.That(remaining > 100, "access token was wasn't accepted");
        }
    }
}