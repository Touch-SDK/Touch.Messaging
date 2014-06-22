using System;
using System.Collections.Generic;
using System.Linq;
using Amazon.SQS;
using Amazon.SQS.Model;
using Touch.Serialization;

namespace Touch.Queue
{
    internal sealed class MessageQueue<T> : IMessageQueue<T>
        where T : class, IMessage, new()
    {
        #region .ctor
        public MessageQueue(ISerializer serializer, Func<IAmazonSQS> clientFactory, string queueUrl)
        {
            if (serializer == null) throw new ArgumentNullException("serializer");
            _serializer = serializer;

            if (clientFactory == null) throw new ArgumentNullException("clientFactory");
            _clientFactory = clientFactory;

            if (string.IsNullOrEmpty(queueUrl)) throw new ArgumentNullException("queueUrl");
            _queueUrl = queueUrl;
        }
        #endregion

        #region Data
        private readonly ISerializer _serializer;
        private readonly Func<IAmazonSQS> _clientFactory;
        private readonly string _queueUrl;
        #endregion

        #region Private methods
        /// <summary>
        /// Get SQS client instance.
        /// </summary>
        /// <returns></returns>
        private IAmazonSQS GetClient() { return _clientFactory.Invoke(); }

        /// <summary>
        /// Load new messages from the queue.
        /// </summary>
        /// <param name="take">Number of messages to load.</param>
        /// <param name="visibilityTimeout">Visibility timeout.</param>
        /// <returns>Queue of new messages.</returns>
        private IEnumerable<IQueueItem<T>> LoadNewMessages(int take, TimeSpan visibilityTimeout)
        {
            using (var client = GetClient())
            {
                var request = new ReceiveMessageRequest
                {
                    MaxNumberOfMessages = take,
                    QueueUrl = _queueUrl,
                    VisibilityTimeout = Convert.ToInt32(visibilityTimeout.TotalSeconds)
                };
                
                var response = client.ReceiveMessage(request);

                var queue = new Queue<IQueueItem<T>>();

                foreach (var rawMessage in response.Messages)
                {
                    var item = ParseMessage(rawMessage);
                    item.ExpirationTime = DateTime.UtcNow.Add(visibilityTimeout);
                    queue.Enqueue(item);
                }

                return queue;
            }
        }
        #endregion

        #region Parsers
        /// <summary>
        /// Parse raw Amazon SQS message.
        /// </summary>
        /// <param name="rawMessage">SQS message.</param>
        /// <returns>Parsed message.</returns>
        private QueueItem<T> ParseMessage(Message rawMessage)
        {
            return new QueueItem<T>
            {
                Id = rawMessage.MessageId,
                Receipt = rawMessage.ReceiptHandle,
                Body = _serializer.Deserialize<T>(rawMessage.Body)
            };
        }
        #endregion

        #region IMessageQueue<T> implementation
        public void Enqueue(T message)
        {
            using (var client = GetClient())
            {
                var body = _serializer.Serialize(message);

                var request = new SendMessageRequest
                {
                    QueueUrl = _queueUrl,
                    MessageBody = body
                };

                client.SendMessage(request);
            }
        }

        public IEnumerable<IQueueItem<T>> Dequeue(uint take, TimeSpan visibilityTimeout)
        {
            var data = LoadNewMessages((int)take, visibilityTimeout);
            return data.Where(item => item.ExpirationTime > DateTime.UtcNow);
        }

        public void DeleteMessage(IQueueItem<T> message)
        {
            using (var client = GetClient())
            {
                var request = new DeleteMessageRequest
                {
                    QueueUrl = _queueUrl,
                    ReceiptHandle = message.Receipt
                };

                client.DeleteMessage(request);
            }
        }
        #endregion
    }
}
