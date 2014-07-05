using System;
using System.Runtime.Serialization;
using Amazon;
using Amazon.Runtime;
using Touch.Serialization;

namespace Touch.Messaging
{
    public sealed class ApnsNotificationDispatcher : AbstractNotificationBroadcaster
    {
        #region .ctor
        public ApnsNotificationDispatcher(ISerializer jsonSerializer, AWSCredentials credentials, string connectionString)
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
            var payload = _jsonSerializer.Serialize(new ApnsPayload { Body = new ApnsPayloadBody { Alert = message, Badge = count, Sound = "default" }});
            var snsPayload = Config.IsSandbox ? new SnsPayload { ApnsSandbox = payload } : new SnsPayload { Apns = payload };
            Broadcats(_jsonSerializer.Serialize(snsPayload), deviceToken);
        }
        #endregion
    }

    [DataContract]
    internal class ApnsPayload
    {
        [DataMember(Name = "aps")]
        public ApnsPayloadBody Body;
    }

    [DataContract]
    internal class ApnsPayloadBody
    {
        [DataMember(Name = "alert")]
        public string Alert;

        [DataMember(Name = "badge")]
        public int Badge;

        [DataMember(Name = "sound")]
        public string Sound;
    }
}
