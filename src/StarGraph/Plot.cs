using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace StarGraph
{
    public static class Plot
    {
        public static Bitmap MakePlot(StarRecord[] records)
        {
            var plt = new ScottPlot.Plot();
            double[] xs = records.Select(x=>x.DateTime.ToOADate()).ToArray();
            double[] ys = ScottPlot.DataGen.Consecutive(records.Length, offset: 1);
            plt.AddScatterStep(xs, ys);
            Bitmap bmp = plt.Render();
            return bmp;
        }
    }
}
