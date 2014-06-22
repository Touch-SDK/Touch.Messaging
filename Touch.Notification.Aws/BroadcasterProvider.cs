using System;
using System.Collections.Concurrent;
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
        private IAmazonSimpleNotificationService ClientFactory()
        {
            return AWSClientFactory.CreateAmazonSimpleNotificationServiceClient(_credentials);
        }

        public INotificationBroadcaster<T> GetBroadcaster<T>(string topicName) 
            where T : class, INotification, new()
        {
            if (string.IsNullOrEmpty(topicName)) throw new ArgumentException("topicName");

            object broadcaster;

            if (!_broadcasters.TryGetValue(topicName, out broadcaster))
            {
                broadcaster = new NotificationBroadcaster<T>(_serializer, ClientFactory, topicName);
                _broadcasters[topicName] = broadcaster;
            }

            return (INotificationBroadcaster<T>)broadcaster;
        }
        #endregion
    }
}
