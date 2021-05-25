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

            var plt = new ScottPlot.Plot(600, 350);
            plt.AddScatterLines(xs, ys, lineWidth: 2);

            string msg = "Recent Stargazers:\n" + string.Join("\n", records.TakeLast(12).Select(x => " - " + x.User));
            var starsAnnotation = plt.AddAnnotation(msg, 10, 10);
            starsAnnotation.BackgroundColor = ColorTranslator.FromHtml("#F6F6DD");

            string day = DateTime.UtcNow.ToString("yyyy-MM-dd");
            string time = DateTime.UtcNow.ToString("H:mm");
            var dateAnnotation = plt.AddAnnotation($"updated {day} at {time} UTC", -3, -1);
            dateAnnotation.Border = false;
            dateAnnotation.Shadow = false;
            dateAnnotation.Background = false;
            dateAnnotation.Font.Color = Color.FromArgb(150, Color.Black);
            dateAnnotation.Font.Name = ScottPlot.Drawing.InstalledFont.Monospace();
            dateAnnotation.Font.Size = 10;

            plt.Title($"ScottPlot has {records.Length} GitHub Stars");
            plt.YLabel("Total Stars");
            plt.XAxis.DateTimeFormat(true);

            Bitmap bmp = plt.Render();
            return bmp;
        }

        public static byte[] GetPlotBytes(StarRecord[] records)
        {
            Bitmap bmp = MakePlot(records);
            return (byte[])new ImageConverter().ConvertTo(bmp, typeof(byte[]));
        }
    }
}
