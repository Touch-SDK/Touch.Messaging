using System;
using System.Data.Common;
using Amazon;

namespace Touch.Notification
{
    public class SnsBroadcasterConnectionStringBuilder : DbConnectionStringBuilder
    {
        public string Topic
        {
            get { return ContainsKey("Topic") ? this["Topic"] as string : null; }
            set { this["Topic"] = value; }
        }

        public RegionEndpoint Region
        {
            get { return RegionEndpoint.GetBySystemName(this["Region"] as string); }
            set { this["Region"] = value; }
        }
    }
}
