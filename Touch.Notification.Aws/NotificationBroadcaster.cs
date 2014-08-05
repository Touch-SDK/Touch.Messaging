using System;
using Amazon;
using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using Touch.Serialization;

namespace Touch.Notification
{
    /// <summary>
    /// Amazon SNS notification broadcaster.
    /// </summary>
    /// <typeparam name="T">Notification type.</typeparam>
    internal sealed class NotificationBroadcaster<T> : INotificationBroadcaster<T>
        where T : class, INotification, new()
    {
        #region .ctor
        /// <summary>
        /// Amazon SNS notification broadcaster contructor.
        /// </summary>
        /// <param name="serializer">Notification message serializer.</param>
        /// <param name="clientFactory">Notification client factory.</param>
        /// <param name="connectionString">Connection string.</param>
        internal NotificationBroadcaster(ISerializer serializer, Func<RegionEndpoint, IAmazonSimpleNotificationService> clientFactory, BroadcasterConnectionStringBuilder connectionString)
        {
            if (serializer == null) throw new ArgumentNullException("serializer");
            _serializer = serializer;

            if (clientFactory == null) throw new ArgumentNullException("clientFactory");
            _clientFactory = clientFactory;

            if (connectionString == null) throw new ArgumentNullException("connectionString");
            _connectionString = connectionString;
        }
        #endregion

        #region Data
        private readonly ISerializer _serializer;
        private readonly Func<RegionEndpoint, IAmazonSimpleNotificationService> _clientFactory;
        private readonly BroadcasterConnectionStringBuilder _connectionString;
        #endregion;

        #region INotificationBroadcaster implementation
        public void Broadcast(T notification)
        {
            if (notification == null) throw new ArgumentNullException("notification");

            var messageBody = _serializer.Serialize(notification);

            using (var client = _clientFactory(_connectionString.Region))
            {
                var request = new PublishRequest
                {
                    TopicArn = _connectionString.TopicArn,
                    Message = messageBody
                };

                client.Publish(request);
            }
        }
        #endregion;
    }
}
