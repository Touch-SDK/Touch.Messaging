using System;
using System.Runtime.Serialization;
using Amazon;
using Amazon.Runtime;
using Touch.Serialization;

namespace Touch.Messaging
{
    public sealed class GcmNotificationDispatcher : AbstractNotificationBroadcaster
    {
        #region .ctor
        public GcmNotificationDispatcher(ISerializer jsonSerializer, AWSCredentials credentials, string connectionString)
            : base(credentials, connectionString)
        {
            if (jsonSerializer == null) throw new ArgumentNullException("jsonSerializer");
            _jsonSerializer = jsonSerializer;
        }
        #endregion

        private readonly ISerializer _jsonSerializer;

        #region INotificationDispatcher members
        public override void Dispatch(string deviceToken, string message, int count = 0, string data = null)
        {
            var payload = _jsonSerializer.Serialize(new GcmPayload {Body = new GcmPayloadBody {Text = message, Badge = count, Data = data}});
            var snsPayload = new SnsPayload { Gcm = payload };
            Broadcats(_jsonSerializer.Serialize(snsPayload), deviceToken);
        }
        #endregion
    }

    [DataContract]
    internal class GcmPayload
    {
        [DataMember(Name = "data")]
        public GcmPayloadBody Body;
    }

    [DataContract]
    internal class GcmPayloadBody
    {
        [DataMember(Name = "text")]
        public string Text;

        [DataMember(Name = "badge")]
        public int Badge;

        [DataMember(Name = "request_token")]
        public string Data;
    }
}
