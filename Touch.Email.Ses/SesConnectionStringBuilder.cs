using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        public RegionEndpoint Region
        {
            get { return RegionEndpoint.GetBySystemName(this["Region"] as string); }
            set { this["Region"] = value; }
        }
    }
}
