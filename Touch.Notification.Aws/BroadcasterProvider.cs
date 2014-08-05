using System;
using System.Collections.Concurrent;
using System.Configuration;
using Amazon;
using Amazon.Runtime;
using Amazon.SimpleNotificationService;
using Touch.Serialization;

namespace Touch.Notification
{
    /// <summary>
    /// Broadcaster provider.
    /// </summary>
    sealed public class BroadcasterProvider : IBroadcasterProvider
    {
        #region .ctor
        public BroadcasterProvider(ISerializer serializer, AWSCredentials credentials)
        {
            if (serializer == null) throw new ArgumentNullException("serializer");
            _serializer = serializer;

            if (credentials == null) throw new ArgumentException("credentials");
            _credentials = credentials;
        }
        #endregion

        #region Data
        private readonly ISerializer _serializer;
        private readonly AWSCredentials _credentials;
        private readonly ConcurrentDictionary<string, object> _broadcasters = new ConcurrentDictionary<string, object>();
        #endregion;

        #region IBroadcasterProvider implementation
        private IAmazonSimpleNotificationService ClientFactory(RegionEndpoint region)
        {
            return AWSClientFactory.CreateAmazonSimpleNotificationServiceClient(_credentials, region);
        }

        public INotificationBroadcaster<T> GetBroadcaster<T>(string topicName) 
            where T : class, INotification, new()
        {
            if (string.IsNullOrEmpty(topicName)) throw new ArgumentException("topicName");

            object broadcaster;

            if (!_broadcasters.TryGetValue(topicName, out broadcaster))
            {
                var connectionString = ConfigurationManager.ConnectionStrings[topicName];

                if (connectionString == null || string.IsNullOrWhiteSpace(connectionString.ConnectionString))
                    throw new ConfigurationErrorsException("Connection string '" + topicName + "' is not set.");

                var builder = new BroadcasterConnectionStringBuilder
                {
                    ConnectionString = connectionString.ConnectionString
                };

                broadcaster = new NotificationBroadcaster<T>(_serializer, ClientFactory, builder);
                _broadcasters[topicName] = broadcaster;
            }

            return (INotificationBroadcaster<T>)broadcaster;
        }
        #endregion
    }
}
