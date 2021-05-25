using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StarGraphTests
{
    class StarRecordManagerTests
    {
        [Test]
        public void Test_RecordManager_IgnoresDuplicateRecords()
        {
            StarGraph.StarRecord[] allRecords = SampleData.GetAllStargazers();

            var stars = new StarGraph.StarRecordManager();
            Assert.AreEqual(0, stars.Count);

            stars.TryAdd(allRecords.Take(100).ToArray());
            Assert.AreEqual(100, stars.Count);

            stars.TryAdd(allRecords.Skip(50).Take(100).ToArray());
            Assert.AreEqual(150, stars.Count);
        }
    }
}
