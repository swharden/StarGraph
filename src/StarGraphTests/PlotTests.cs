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
            StarGraph.StarRecord[] records = SampleData.GetAllStargazers();
            Bitmap bmp = StarGraph.Plot.MakePlot(records);
            string filename = System.IO.Path.GetFullPath("test.png");
            bmp.Save(filename, ImageFormat.Png);
            Console.WriteLine($"Saved: {filename}");
        }
    }
}
