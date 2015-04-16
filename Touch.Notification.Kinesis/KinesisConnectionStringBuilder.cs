using System;
using System.Data.Common;
using Amazon;

namespace Touch.Notification
{
    internal class KinesisConnectionStringBuilder : DbConnectionStringBuilder
    {
        public string StreamName
        {
            get { return ContainsKey("StreamName") ? this["StreamName"] as string : null; }
            set { this["StreamName"] = value; }
        }

        public RegionEndpoint Region
        {
            get { return RegionEndpoint.GetBySystemName(this["Region"] as string); }
            set { this["Region"] = value; }
        }

        public string PartitionKey
        {
            get { return ContainsKey("PartitionKey") ? this["PartitionKey"] as string : null; }
            set { this["PartitionKey"] = value; }
        }
    }
}
