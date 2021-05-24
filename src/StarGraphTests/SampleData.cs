using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace StarGraphTests
{
    public static class SampleData
    {
        public static string GetDataFolder() =>
            Path.Join(TestContext.CurrentContext.TestDirectory + "../../../../data");

        public static string GetStargazerPageJson() =>
            File.ReadAllText(Path.Combine(GetDataFolder(), "stargazers-v3.json"));
    }
}
