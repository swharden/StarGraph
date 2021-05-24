using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;

namespace StarGraph
{
    public static class GitHubJSON
    {
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
