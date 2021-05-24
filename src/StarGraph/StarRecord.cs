using System;

namespace StarGraph
{
    public struct StarRecord
    {
        public string User;
        public DateTime DateTime;

        public override string ToString() => $"{DateTime} {User}";
    }
}
