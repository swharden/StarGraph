using System;
using System.Collections.Generic;
using System.IO;
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

        public static string RecordsToJson(StarRecord[] records)
        {
            using var stream = new MemoryStream();
            var options = new JsonWriterOptions() { Indented = true };
            using var writer = new Utf8JsonWriter(stream, options);

            writer.WriteStartObject();
            writer.WriteString("lastUpdated", DateTime.UtcNow);
            writer.WriteStartArray("starRecords");
            foreach (var record in records)
            {
                writer.WriteStartObject();
                writer.WriteString("utc", record.DateTime.ToUniversalTime());
                writer.WriteString("user", record.User);
                writer.WriteEndObject();
            }
            writer.WriteEndArray();
            writer.WriteEndObject();

            writer.Flush();
            string json = Encoding.UTF8.GetString(stream.ToArray());

            return json;
        }

        public static StarRecord[] RecordsFromJson(string json)
        {
            using JsonDocument document = JsonDocument.Parse(json);
            return document.RootElement
                .GetProperty("starRecords")
                .EnumerateArray()
                .Select(x => new StarRecord()
                {
                    User = x.GetProperty("user").GetString(),
                    DateTime = DateTime.Parse(x.GetProperty("utc").GetString())
                })
                .ToArray();
        }
    }
}
