using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        /// Request every stargazer page and add all stargazers seen.
        /// </summary>
        public void TryAddFromWebAllPages()
        {
            (_, int pageCount, int remaining) = GitHubAPI.RequestStargazersPage(UserName, RepoName, page: 1, token: GitHubToken);
            var pageNumbers = Enumerable.Range(1, pageCount);
            Parallel.ForEach(pageNumbers, pageNumber =>
            {
                (StarRecord[] records, _, _) = GitHubAPI.RequestStargazersPage(UserName, RepoName, page: pageNumber, token: GitHubToken);
                TryAdd(GitHubAPI.RequestStargazersPage(UserName, RepoName, page: pageNumber, token: GitHubToken).records);
            });
        }

        /// <summary>
        /// This method is a cautious way to top-off the existing list of stargazers without making many API requests.
        /// It works backwards from the last page and stops when it finds a duplicate.
        /// This means only the last few pages may need to be inspected, but early stargazers who un-starred will be retained.
        /// </summary>
        /// <param name="maxRequestCount">throw an exception if this number of requests is exceeded</param>
        /// <param name="minRemainingRequests">throw an exception if the number of remaining requests is below this number</param>
        public void TryAddFromWebUntilDuplicate(int maxRequestCount = 50, int minRemainingRequests = 100)
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
