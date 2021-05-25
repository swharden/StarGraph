using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StarGraph
{
    public class StarRecordManager
    {
        public readonly Dictionary<string, DateTime> Records = new();
        public int Count => Records.Count;
        public readonly string UserName;
        public readonly string RepoName;
        private readonly string GitHubToken;

        public StarRecordManager(string userName, string repoName, string gitHubToken)
        {
            UserName = userName;
            RepoName = repoName;
            GitHubToken = gitHubToken;
        }

        /// <summary>
        /// Attempts to add the record (returns true if successful)
        /// </summary>
        public bool TryAdd(StarRecord record) => Records.TryAdd(record.User, record.DateTime);

        /// <summary>
        /// Attempts to add the records (returns true if any were added)
        /// </summary>
        public bool TryAdd(StarRecord[] records)
        {
            int originalCount = Count;
            foreach (StarRecord record in records)
                TryAdd(record);
            return Count > originalCount;
        }

        /// <summary>
        /// Attempts to add the records from JSON (returns true if any were added)
        /// </summary>
        public bool TryAddFromJson(string json) => TryAdd(IO.RecordsFromJson(json));

        /// <summary>
        /// Returns true if at least 1 record from the json already exists
        /// </summary>
        public bool HasDuplicates(StarRecord[] records)
        {
            foreach (StarRecord record in records)
            {
                if (Records.ContainsKey(record.User))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Returns an array of records sorted by date
        /// </summary>
        public StarRecord[] GetSortedRecords()
        {
            var recordsList = Records.ToList();
            recordsList.Sort((pair1, pair2) => pair1.Value.CompareTo(pair2.Value));
            return recordsList.Select(x => new StarRecord() { User = x.Key, DateTime = x.Value }).ToArray();
        }

        /// <summary>
        /// Use the web API to top-off the list of known stargazers with new ones from the web
        /// </summary>
        /// <param name="maxRequestCount">throw an exception if this number of requests is exceeded</param>
        /// <param name="minRemainingRequests">throw an exception if the number of remaining requests is below this number</param>
        public void TryAddFromWeb(int maxRequestCount = 50, int minRemainingRequests = 100)
        {
            (_, int pages, int remaining) = GitHubAPI.RequestStargazersPage(UserName, RepoName, page: 1, token: GitHubToken);
            if (remaining <= minRemainingRequests)
                throw new InvalidOperationException($"too few requests remaining ({remaining})");

            int requestCount = 0;
            for (int i = 0; i < pages; i++)
            {
                requestCount += 1;
                if (requestCount > maxRequestCount)
                    throw new InvalidOperationException($"too many requets without a duplicate ({requestCount})");

                (StarRecord[] newRecords, _, _) = GitHubAPI.RequestStargazersPage(UserName, RepoName, page: pages - i, token: GitHubToken);

                bool duplicates = HasDuplicates(newRecords);
                TryAdd(newRecords);
                if (duplicates)
                    break;
            }
        }
    }
}
