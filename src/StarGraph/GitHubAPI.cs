using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;

namespace StarGraph
{
    public static class GitHubAPI
    {
        public static (StarRecord[] records, int pages, int remaining) RequestStargazersPage(string user, string repo, int page = 1, string token = null)
        {
            string url = $"https://api.github.com/repos/{user}/{repo}/stargazers?page={page}";

            using WebClient client = new();
            WebRequest request = WebRequest.Create(url);
            request.Headers.Set("User-Agent", "request");
            request.Headers.Set("Accept", "application/vnd.github.v3.star+json");
            request.Headers.Add("Authorization", $"Bearer {token}");

            var response = request.GetResponse();
            int pages = int.Parse(response.Headers.GetValues("Link").First().Split("?page=").Last().Split(">").First());
            int remaining = int.Parse(response.Headers.GetValues("X-RateLimit-Remaining").First());
            using var responseReader = new System.IO.StreamReader(response.GetResponseStream());
            string responseText = responseReader.ReadToEnd();
            StarRecord[] records = StarRecordsFromPage(responseText);

            return (records, pages, remaining);
        }

        public static StarRecord[] StarRecordsFromPage(string stargazersPageJson)
        {
            using JsonDocument document = JsonDocument.Parse(stargazersPageJson);
            return document.RootElement.EnumerateArray()
                .Select(x => new StarRecord()
                {
                    DateTime = DateTime.Parse(x.GetProperty("starred_at").GetString()),
                    User = x.GetProperty("user").GetProperty("login").GetString()
                })
                .ToArray();
        }
    }
}
