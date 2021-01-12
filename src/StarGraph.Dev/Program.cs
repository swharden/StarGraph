using System;
using System.Text.Json;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StarGraph.Dev
{
    class Program
    {
        const string SecretsFilePath = "../../../../StarGraph.Functions/local.settings.json";

        static void Main()
        {
            //ViewCurrentStars();
            //AddCurrentStarsToTable().GetAwaiter().GetResult();
            //FillTableWithDataFromLog().GetAwaiter().GetResult();
            Console.WriteLine("DONE");
        }

        private static string GetConnectionString()
        {
            string s = JsonDocument.Parse(File.ReadAllText(SecretsFilePath))
                                   .RootElement
                                   .GetProperty("Values")
                                   .GetProperty("AzureWebJobsStorage")
                                   .GetString();
            if (s is null)
                throw new InvalidOperationException("cannot locate connection string");
            else
                return s;
        }

        private static string GetGitHubToken()
        {
            string s = JsonDocument.Parse(File.ReadAllText(SecretsFilePath))
                                   .RootElement
                                   .GetProperty("Values")
                                   .GetProperty("GitHubToken")
                                   .GetString();

            if (s is null)
                throw new InvalidOperationException("cannot locate GitHub token");
            else
                return s;
        }

        private static void ViewCurrentStars()
        {
            (_, string json) = GitHubAPI.RequestRepo("scottplot", "scottplot", GetGitHubToken());
            Console.WriteLine($"STARS: {GitHubAPI.TotalStars(json)}");
        }

        private static async Task AddCurrentStarsToTable()
        {
            (_, string json) = GitHubAPI.RequestRepo("scottplot", "scottplot", GetGitHubToken());
            int stars = GitHubAPI.TotalStars(json);
            Functions.StarsTable table = new Functions.StarsTable(GetConnectionString());
            await table.Insert("scottplot", "scottplot", stars, DateTime.Now);
        }

        private static async Task FillTableWithDataFromLog()
        {
            Functions.StarsTable table = new Functions.StarsTable(GetConnectionString());
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
                await table.Insert("scottplot", "scottplot", stars, day);
            }
        }
    }
}
