using System;
using System.Text.Json;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace StarGraph.Dev
{
    class Program
    {
        static void Main()
        {
            string secretsFilePath = Path.GetFullPath("../../../../StarGraph.Functions/local.settings.json");
            string connectionString = JsonDocument.Parse(File.ReadAllText(secretsFilePath))
                                                  .RootElement
                                                  .GetProperty("Values")
                                                  .GetProperty("AzureWebJobsStorage")
                                                  .GetString();
            var table = new Functions.StarsTable(connectionString);

            //FillTableWithDataFromLog(table);
        }

        private static void FillTableWithDataFromLog(Functions.StarsTable table)
        {
            Dictionary<string, DateTime> gazerDates = GitHubAPI.GetGazerDatesFromLog("../../../data/gazerDates.csv");
            DateTime created = DateTime.Parse("2018-01-04T03:34:45Z").Date;

            int totalDays = (DateTime.UtcNow.Date - created).Days + 1;
            DateTime[] days = Enumerable.Range(0, totalDays).Select(i => created.AddDays(i)).ToArray();

            int[] newStarsByDay = new int[totalDays];
            foreach ((_, DateTime dt) in gazerDates)
                newStarsByDay[(dt - created).Days] += 1;

            int[] totalStarsByDay = new int[totalDays];
            totalStarsByDay[0] = newStarsByDay[0];
            for (int i = 1; i < totalDays; i++)
                totalStarsByDay[i] = totalStarsByDay[i - 1] + newStarsByDay[i];

            for (int i = 0; i < totalDays; i++)
            {
                DateTime day = days[i];
                int stars = totalStarsByDay[i];
                table.Insert("scottplot", "scottplot", stars, day);
            }
        }
    }
}
