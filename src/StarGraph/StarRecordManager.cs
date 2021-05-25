using System;
using System.Collections.Generic;
using System.Text;

namespace StarGraph
{
    public class StarRecordManager
    {
        public readonly Dictionary<string, DateTime> records = new();
        public int Count => records.Count;

        public StarRecordManager()
        {

        }

        public void TryAdd(StarRecord record)
        {
            records.TryAdd(record.User, record.DateTime);
        }

        public void TryAdd(StarRecord[] records)
        {
            foreach (StarRecord record in records)
                TryAdd(record);
        }
    }
}
