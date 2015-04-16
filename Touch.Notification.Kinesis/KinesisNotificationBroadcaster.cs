using System;
using System.IO;
using Amazon.Kinesis.Model;
using Amazon.Runtime;
using Amazon.Kinesis;
using Touch.Serialization;

namespace Touch.Notification
{
    public sealed class KinesisNotificationBroadcaster<T> : INotificationBroadcaster<T>
        where T : class, new()
    {
        #region .ctor
        public KinesisNotificationBroadcaster(ISerializer serializer, AWSCredentials credentials, string connectionString)
        {
            if (serializer == null) throw new ArgumentNullException("serializer");
            _serializer = serializer;

            if (credentials == null) throw new ArgumentNullException("credentials");
            _credentials = credentials;

            if (string.IsNullOrEmpty(connectionString)) throw new ArgumentNullException("connectionString");
            _config = new KinesisConnectionStringBuilder { ConnectionString = connectionString };
        } 
        #endregion

        #region Data
        private readonly ISerializer _serializer;
        private readonly KinesisConnectionStringBuilder _config;
        private readonly AWSCredentials _credentials;
        #endregion

        #region INotificationBroadcaster members
        public void Broadcast(T notification)
        {
            if (notification == null) throw new ArgumentNullException("notification");
            
            using (var client = new AmazonKinesisClient(_credentials, _config.Region))
            using (var data = new MemoryStream())
            {
                _serializer.Serialize(notification, data);
                data.Position = 0;

                client.PutRecord(new PutRecordRequest
                {
                    Data = data,
                    StreamName = _config.StreamName,
                    PartitionKey = _config.PartitionKey
                });
            }
        } 
        #endregion
    }
}
