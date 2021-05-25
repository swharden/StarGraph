using System;
using System.Linq;
using System.Net;
using System.Text.Json;

namespace StarGraph
{
    public static class GitHubAPI
    {
        /// <summary>
        /// Return information about all stars obtained from every stargazer page using the GitHub API
        /// </summary>
        public static StarRecord[] RequestAllStargazers(string user, string repo, string token)
        {
            int pageCount = RequestStargazersPage(user, repo, 1, token).pages;
            var recordsList = Enumerable.Range(1, pageCount)
                .AsParallel()
                .SelectMany(pageNumber => RequestStargazersPage(user, repo, page: pageNumber, token).records)
                .ToList();
            recordsList.Sort((a, b) => a.DateTime.CompareTo(b.DateTime));
            return recordsList.ToArray();
        }

        /// <summary>
        /// Use the GitHub API to request star information from a single page
        /// </summary>
        /// <returns></returns>
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
            StarRecord[] records = StarRecordsFromPageJSON(responseText);

            Console.WriteLine($" - Requested {user}/{repo} page {page} ({remaining} requests remain)...");
            return (records, pages, remaining);
        }

        /// <summary>
        /// Return star records given the JSON returned by the GitHub API
        /// </summary>
        public static StarRecord[] StarRecordsFromPageJSON(string stargazersPageJson)
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
