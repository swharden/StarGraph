using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace StarGraph
{
    public static class GitHubAPI
    {
        public static (string[] recentGazers, int stars) GetStarInfo(string user, string repo)
        {
            return (null, 123);
        }

        public static int GetStarPageCount(string user, string repo)
        {
            return -1;
        }

        public static (string json, int pages, int remaining) RequestStargazerJson(string user, string repo, int page = 1, string token = null)
        {
            string url = $"https://api.github.com/repos/{user}/{repo}/stargazers?page={page}";

            using WebClient client = new();
            WebRequest request = WebRequest.Create(url);
            request.Headers.Set("User-Agent", "request");
            request.Headers.Set("Accept", "application/vnd.github.v3.star+json");
            var response = request.GetResponse();

            using var reader = new System.IO.StreamReader(response.GetResponseStream());
            string json = reader.ReadToEnd();

            int pages = int.Parse(response.Headers.GetValues("Link").First().Split("?page=").Last().Split(">").First());
            int remaining = int.Parse(response.Headers.GetValues("X-RateLimit-Remaining").First());

            return (json, pages, remaining);
        }
    }
}
