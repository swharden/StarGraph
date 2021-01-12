using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Linq;
using System.Text.Json;
using System.IO;

namespace StarGraph
{

    public static class GitHubAPI
    {
        // https://docs.github.com/en/free-pro-team@latest/github/authenticating-to-github/creating-a-personal-access-token
        //private static string GetSecretToken();

        public static (WebHeaderCollection headers, string json) RequestStargazers(string user, string repo, string token, int page = 1) =>
            GetResponse($"https://api.github.com/repos/{user}/{repo}/stargazers?page={page}", token);

        public static (WebHeaderCollection headers, string json) RequestRepo(string user, string repo, string token) =>
            GetResponse($"https://api.github.com/repos/{user}/{repo}", token);

        private static (WebHeaderCollection headers, string json) GetResponse(string url, string token)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "GET";
            request.Timeout = 5000;
            request.Headers.Add("User-Agent", "request");
            request.Headers.Add("Accept", "application/vnd.github.v3.star+json");
            request.Headers.Add("Authorization", $"Bearer {token}");
            request.AllowAutoRedirect = false; // authentication does not get redirected

            using HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            if (response.StatusCode != HttpStatusCode.OK)
                throw new InvalidOperationException($"HTTP status ({response.StatusCode}) is not OK");

            using StreamReader sr = new StreamReader(response.GetResponseStream());
            string json = sr.ReadToEnd();
            return (response.Headers, json);
        }

        public static Dictionary<string, DateTime> GetGazerDatesHTTP(string user, string repo, string token)
        {
            // make an initial request to determine the total number of pages
            var (h1, _) = RequestStargazers(user, repo, token);
            int totalPages = GitHubAPI.TotalPages(h1);

            // request each page
            var gazerDates = new Dictionary<string, DateTime>();
            for (int i = 0; i < totalPages; i++)
            {
                int pageNumber = i + 1;
                var (headers, json) = RequestStargazers(user, repo, token, pageNumber);
                using JsonDocument document = JsonDocument.Parse(json);
                var elements = document.RootElement.EnumerateArray();
                Console.WriteLine($"Page {pageNumber}/{totalPages} has {elements.Count()} stars " +
                    $"(requests remaining: {RateLimitRemaining(headers)})");
                foreach (JsonElement element in elements)
                {
                    DateTime date = element.GetProperty("starred_at").GetDateTime();
                    string gazer = element.GetProperty("user").GetProperty("login").GetString();
                    gazerDates[gazer] = date;
                }
            }
            Console.WriteLine($"Complete! {gazerDates.Count()} stars total.");

            return gazerDates;
        }

        private static int TotalPages(WebHeaderCollection headers) =>
            int.Parse(headers.GetValues("link").First().Split("?page=").Last().Split(">").First());

        private static int RateLimitRemaining(WebHeaderCollection headers) =>
            int.Parse(headers.GetValues("X-RateLimit-Remaining").First());

        private static int RateLimitReset(WebHeaderCollection headers) =>
            int.Parse(headers.GetValues("X-RateLimit-Reset").First());

        private static int TotalStars(string json) =>
            JsonDocument.Parse(json).RootElement.GetProperty("stargazers_count").GetInt32();

        private static DateTime CreationDate(string json) =>
            JsonDocument.Parse(json).RootElement.GetProperty("created_at").GetDateTime();

        public static Dictionary<string, DateTime> GetGazerDatesFromLog(string filePath)
        {
            var starDates = new Dictionary<string, DateTime>();
            foreach (string line in System.IO.File.ReadAllLines(filePath))
            {
                string[] parts = line.Split(',');
                if (parts.Length != 2)
                    continue;
                string user = parts[0];
                DateTime date = DateTime.Parse(parts[1]);
                starDates[user] = date;
            }
            return starDates;
        }

        public static void SaveGazerDates(Dictionary<string, DateTime> starDates, string filePath)
        {
            filePath = System.IO.Path.GetFullPath(filePath);
            System.IO.File.WriteAllLines(filePath, starDates.Select(x => $"{x.Key},{x.Value}"));
            Console.WriteLine($"Saved: {filePath}");
        }
    }
}
