using System.Runtime.Serialization;

namespace Touch.Messaging
{
    [DataContract]
    internal class SnsPayload
    {
        [DataMember(Name = "APNS")]
        public string Apns;

        [DataMember(Name = "APNS_SANDBOX")]
        public string ApnsSandbox;

        [DataMember(Name = "GCM")]
        public string Gcm;
    }
}
