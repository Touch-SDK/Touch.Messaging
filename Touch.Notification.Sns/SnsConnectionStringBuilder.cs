﻿using System;
using System.Data.Common;
using Amazon;

namespace Touch.Notification
{
    public class SnsConnectionStringBuilder : DbConnectionStringBuilder
    {
        public string Application
        {
            get { return ContainsKey("Application") ? this["Application"] as string : null; }
            set { this["Application"] = value; }
        }

        public bool IsSandbox
        {
            get { return ContainsKey("IsSandbox") && Convert.ToBoolean(this["IsSandbox"]); }
            set { this["IsSandbox"] = value; }
        }

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
