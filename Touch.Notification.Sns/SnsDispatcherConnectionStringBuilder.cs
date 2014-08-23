using System;
using System.Data.Common;
using Amazon;

namespace Touch.Notification
{
    public class SnsDispatcherConnectionStringBuilder : DbConnectionStringBuilder
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

        public RegionEndpoint Region
        {
            get { return RegionEndpoint.GetBySystemName(this["Region"] as string); }
            set { this["Region"] = value; }
        }
    }
}
