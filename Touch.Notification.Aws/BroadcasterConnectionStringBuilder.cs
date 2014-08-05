using System.Data.Common;
using Amazon;

namespace Touch.Notification
{
    public class BroadcasterConnectionStringBuilder : DbConnectionStringBuilder
    {
        public string TopicArn
        {
            get { return ContainsKey("TopicArn") ? this["TopicArn"] as string : null; }
            set { this["TopicArn"] = value; }
        }

        public RegionEndpoint Region
        {
            get { return RegionEndpoint.GetBySystemName(this["Region"] as string); }
            set { this["Region"] = value; }
        }
    }
}
