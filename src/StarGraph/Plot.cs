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
            double[] xs = records.Select(x => x.DateTime.ToOADate()).ToArray();
            double[] ys = ScottPlot.DataGen.Consecutive(records.Length, offset: 1);
            string day = DateTime.Now.ToString("yyyy-MM-dd");
            string msg = "Recent Stargazers:\n" + string.Join("\n", records.TakeLast(12).Select(x => " - " + x.User));

            var plt = new ScottPlot.Plot(600, 350);
            plt.AddScatterLines(xs, ys, lineWidth: 2);

            var annotation = plt.AddAnnotation(msg, 10, 10);
            annotation.BackgroundColor = ColorTranslator.FromHtml("#F6F6F6");

            plt.Title($"ScottPlot has {records.Length} GitHub Stars");
            plt.YLabel("Total Stars");
            plt.YAxis.SetSizeLimit(min: 50);
            plt.XAxis.DateTimeFormat(true);

            Bitmap bmp = plt.Render();
            return bmp;
        }
    }
}
