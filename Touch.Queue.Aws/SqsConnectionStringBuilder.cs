using System;
using System.Data.Common;
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

        public int WaitTime
        {
            get { return ContainsKey("WaitTime") ? Convert.ToInt32(this["WaitTime"]) : 0; }
            set { this["WaitTime"] = value; }
        }

        public int VisibilityTimeout
        {
            get { return ContainsKey("VisibilityTimeout") ? Convert.ToInt32(this["VisibilityTimeout"]) : 0; }
            set { this["VisibilityTimeout"] = value; }
        }
    }
}
