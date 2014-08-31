﻿using System.Data.Common;
using Amazon;

namespace Touch.Queue
{
    public class SqsConnectionStringBuilder : DbConnectionStringBuilder
    {
        public string QueueUrl
        {
            get { return ContainsKey("QueueUrl") ? this["QueueUrl"] as string : null; }
            set { this["QueueUrl"] = value; }
        }

        public RegionEndpoint Region
        {
            get { return RegionEndpoint.GetBySystemName(this["Region"] as string); }
            set { this["Region"] = value; }
        }
    }
}