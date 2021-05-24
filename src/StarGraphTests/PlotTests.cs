using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Text;

namespace StarGraphTests
{
    public class PlotTests
    {
        [Test]
        public void Test_Plot_LooksGood()
        {
            string json = SampleData.GetStargazerPageJson();
            StarGraph.StarRecord[] records = StarGraph.GitHubJSON.StarRecordsFromPage(json);
            Bitmap bmp = StarGraph.Plot.MakePlot(records);
            bmp.Save("test.png", ImageFormat.Png);
        }
    }
}
