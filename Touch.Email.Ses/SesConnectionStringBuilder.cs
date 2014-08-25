using System.Data.Common;
using Amazon;

namespace Touch.Email
{
    public class SesConnectionStringBuilder : DbConnectionStringBuilder
    {
        public string SenderAddress
        {
            get { return ContainsKey("SenderAddress") ? this["SenderAddress"] as string : null; }
            set { this["SenderAddress"] = value; }
        }

        public string SenderName
        {
            get { return ContainsKey("SenderName") ? this["SenderName"] as string : null; }
            set { this["SenderName"] = value; }
        }

        public RegionEndpoint Region
        {
            get { return RegionEndpoint.GetBySystemName(this["Region"] as string); }
            set { this["Region"] = value; }
        }
    }
}
