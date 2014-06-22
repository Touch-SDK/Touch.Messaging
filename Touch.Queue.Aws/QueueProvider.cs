using System;
using System.Collections.Concurrent;
using Amazon;
using Amazon.Runtime;
using Amazon.SQS;
using Amazon.SQS.Model;
using Touch.Serialization;

namespace Touch.Queue
{
    /// <summary>
    /// Queue provider.
    /// </summary>
    sealed public class QueueProvider : IQueueProvider
    {
        #region .ctor
        public QueueProvider(ISerializer serializer, AWSCredentials credentials, RegionEndpoint regionEndpoint)
        {
            if (serializer == null) throw new ArgumentNullException("serializer");
            _serializer = serializer;

            if (credentials == null) throw new ArgumentException("credentials");
            _credentials = credentials;

            if (regionEndpoint == null) throw new ArgumentException("regionEndpoint");
            _regionEndpoint = regionEndpoint;
        }
        #endregion

        #region Data
        private readonly ISerializer _serializer;
        private readonly AWSCredentials _credentials;
        private readonly RegionEndpoint _regionEndpoint;
        private readonly ConcurrentDictionary<string, object> _queues = new ConcurrentDictionary<string, object>();
        #endregion;

        #region IQueueProvider implementation
        private IAmazonSQS ClientFactory()
        {
            return AWSClientFactory.CreateAmazonSQSClient(_credentials, _regionEndpoint);
        }

        public IMessageQueue<T> GetQueue<T>(string queueName) where T : class, IMessage, new()
        {
            if (string.IsNullOrEmpty(queueName)) throw new ArgumentException("queueName");

            object queue;

            if (!_queues.TryGetValue(queueName, out queue))
            {
                using (var client = ClientFactory())
                {
                    var request = new GetQueueUrlRequest
                    {
                        QueueName = queueName
                    };

                    var queueUri = client.GetQueueUrl(request).QueueUrl;

                    queue = new MessageQueue<T>(_serializer, ClientFactory, queueUri);
                    _queues[queueName] = queue;
                }
            }

            return (IMessageQueue<T>)queue;
        }
        #endregion
    }
}
