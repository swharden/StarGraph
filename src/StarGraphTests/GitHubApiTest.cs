using NUnit.Framework;
using System;
using System.IO;
using System.Reflection;

namespace StarGraphTests
{
    public class GitHubApiTest
    {
        public string JsonPath;

        [OneTimeSetUp]
        public void Setup()
        {
            var dataFolderPath = Path.Join(TestContext.CurrentContext.TestDirectory + "../../../../data");
            dataFolderPath = Path.GetFullPath(dataFolderPath);
            JsonPath = Path.Combine(dataFolderPath, "stargazers-v3.json");
        }

        [Test]
        public void Test_StargazersJson_Exists()
        {
            Console.WriteLine(JsonPath);
            Assert.That(File.Exists(JsonPath), $"file not found: {JsonPath}");
        }
    }
}