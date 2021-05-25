using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace StarGraphTests
{
    public static class SampleData
    {
        public static string GetDataFolder() =>
            Path.Join(TestContext.CurrentContext.TestDirectory + "../../../../data");

        public static string GetStargazerPageJson() =>
            File.ReadAllText(Path.Combine(GetDataFolder(), "stargazers-page-0001.json"));

        public static StarGraph.StarRecord[] GetAllStargazers() =>
            Directory
            .GetFiles(GetDataFolder(), "stargazers-page-*.json")
            .Select(x => File.ReadAllText(x))
            .SelectMany(x => StarGraph.GitHubAPI.StarRecordsFromPage(x))
            .ToArray();
    }
}