using System;
using Amazon;
using Amazon.Runtime;
using Amazon.SimpleNotificationService.Model;
using Touch.Serialization;

namespace Touch.Notification
{
    public class SnsTopicNotificationBroadcaster<T> : INotificationBroadcaster<T>
        where T : class, new()
    {
        #region .ctor
        public SnsTopicNotificationBroadcaster(ISerializer jsonSerializer, AWSCredentials credentials, string connectionString)
        {
            if (jsonSerializer == null) throw new ArgumentNullException("credentials");
            JsonSerializer = jsonSerializer;

            if (credentials == null) throw new ArgumentNullException("credentials");
            _credentials = credentials;

            if (string.IsNullOrEmpty(connectionString)) throw new ArgumentException("connectionString");
            Config = new SnsConnectionStringBuilder { ConnectionString = connectionString };

            if (string.IsNullOrWhiteSpace(Config.Topic)) throw new ArgumentException("Missing TopicArn", "connectionString");
        }
        #endregion

        #region Data
        protected readonly ISerializer JsonSerializer;
        protected readonly SnsConnectionStringBuilder Config;

        private readonly AWSCredentials _credentials;
        #endregion

        #region INotificationBroadcaster members
        public void Broadcast(T notification)
        {
            if (notification == null) throw new ArgumentNullException("notification");

            using (var client = AWSClientFactory.CreateAmazonSimpleNotificationServiceClient(_credentials, Config.Region))
            {
                var request = new PublishRequest
                {
                    TopicArn = Config.Topic,
                    Message = CreateMessage(notification)
                };

                client.Publish(request);
            }
        }

        protected virtual string CreateMessage(T notification)
        {
            return JsonSerializer.Serialize(notification);
        }
        #endregion
    }
}
