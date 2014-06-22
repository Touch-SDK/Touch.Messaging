using System;
using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using Amazon.SimpleNotificationService;
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
        /// <param name="notificationTopic">Notification topic name.</param>
        internal NotificationBroadcaster(ISerializer serializer, Func<IAmazonSimpleNotificationService> clientFactory, string notificationTopic)
        {
            if (serializer == null) throw new ArgumentNullException("serializer");
            _serializer = serializer;

            if (clientFactory == null) throw new ArgumentNullException("clientFactory");
            _clientFactory = clientFactory;

            if (string.IsNullOrEmpty(notificationTopic)) throw new ArgumentException("notificationTopic");
            _notificationTopic = notificationTopic;
        }
        #endregion

        #region Data
        private readonly ISerializer _serializer;
        private readonly Func<IAmazonSimpleNotificationService> _clientFactory;
        private readonly string _notificationTopic;
        #endregion;

        #region INotificationBroadcaster implementation
        public void Broadcast(T notification)
        {
            if (notification == null) throw new ArgumentNullException("notification");

            var messageBody = _serializer.Serialize(notification);

            using (var client = _clientFactory.Invoke())
            {
                var request = new PublishRequest
                {
                    TopicArn = _notificationTopic,
                    Message = messageBody
                };

                client.Publish(request);
            }
        }
        #endregion;
    }
}
