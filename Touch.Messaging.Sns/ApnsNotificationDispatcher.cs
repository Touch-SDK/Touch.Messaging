﻿using System;
using System.Runtime.Serialization;
using Amazon;
using Amazon.Runtime;
using Touch.Serialization;

namespace Touch.Messaging
{
    public sealed class ApnsNotificationDispatcher : AbstractNotificationBroadcaster
    {
        #region .ctor
        public ApnsNotificationDispatcher(ISerializer jsonSerializer, AWSCredentials credentials, RegionEndpoint region, string applicationArn)
            : base(credentials, region, applicationArn)
        {
            if (jsonSerializer == null) throw new ArgumentNullException("jsonSerializer");
            _jsonSerializer = jsonSerializer;
        }
        #endregion

        private readonly ISerializer _jsonSerializer;

        #region INotificationDispatcher members
        public override void Dispatch(string deviceToken, string message, int count = 0, string data = null)
        {
            var payload = new ApnsPayload { Alert = message, Badge = count, Sound = "default" };
            Broadcats(_jsonSerializer.Serialize(payload), deviceToken);
        }
        #endregion
    }

    [DataContract]
    internal class ApnsPayload
    {
        [DataMember(Name = "alert")]
        public string Alert;

        [DataMember(Name = "badge")]
        public int Badge;

        [DataMember(Name = "sound")]
        public string Sound;
    }
}