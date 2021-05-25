using NUnit.Framework;
using System;

namespace StarGraphTests
{
    class JsonTests
    {
        [Test]
        public void Test_ToJson_DoesNotFail()
        {
            StarGraph.StarRecord[] records = SampleData.GetAllStargazers();
            string json = StarGraph.GitHubJSON.RecordsToJson(records);
            Console.WriteLine(json);
            Assert.IsNotNull(json);
            Assert.Greater(json.Length, 100);
        }

        [Test]
        public void Test_FromJson_MatchesToJson()
        {
            StarGraph.StarRecord[] records = SampleData.GetAllStargazers();
            string json = StarGraph.GitHubJSON.RecordsToJson(records);

            StarGraph.StarRecord[] records2 = StarGraph.GitHubJSON.RecordsFromJson(json);
            Assert.IsNotNull(records2);
            Assert.AreEqual(records.Length, records2.Length);

            for (int i = 0; i < records.Length; i++)
            {
                Assert.AreEqual(records[i].User, records2[i].User);
                Assert.AreEqual(records[i].DateTime, records2[i].DateTime);
            }
        }
    }
}
