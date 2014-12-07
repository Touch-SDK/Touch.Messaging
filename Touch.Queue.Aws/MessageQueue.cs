using System;
using System.Collections.Generic;
using Amazon;
using Amazon.Runtime;
using Amazon.SQS;
using Amazon.SQS.Model;
using Touch.Serialization;

namespace Touch.Queue
{
    public sealed class MessageQueue<T> : IMessageQueue<T>
        where T : class, new()
    {
        #region .ctor
        public MessageQueue(ISerializer serializer, AWSCredentials credentials, string connectionString)
        {
            if (serializer == null) throw new ArgumentNullException("serializer");
            _serializer = serializer;

            if (credentials == null) throw new ArgumentNullException("credentials");
            _credentials = credentials;

            if (string.IsNullOrEmpty(connectionString)) throw new ArgumentException("connectionString");
            _config = new SqsConnectionStringBuilder { ConnectionString = connectionString };
        }
        #endregion

        #region Data
        private readonly ISerializer _serializer;
        private readonly AWSCredentials _credentials;
        private readonly SqsConnectionStringBuilder _config;
        #endregion

        #region Private methods
        private IAmazonSQS GetClient() { return AWSClientFactory.CreateAmazonSQSClient(_credentials, _config.Region); }

        private IEnumerable<IQueueItem<T>> LoadNewMessages(int take)
        {
            using (var client = GetClient())
            {
                var request = new ReceiveMessageRequest
                {
                    MaxNumberOfMessages = take,
                    QueueUrl = _config.QueueUrl
                };

                if (_config.VisibilityTimeout > 0)
                    request.VisibilityTimeout = _config.VisibilityTimeout;

                if (_config.WaitTime > 0)
                    request.WaitTimeSeconds = _config.WaitTime;
                
                var response = client.ReceiveMessage(request);

                var queue = new Queue<IQueueItem<T>>();

                foreach (var rawMessage in response.Messages)
                {
                    var item = ParseMessage(rawMessage);
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
                    QueueUrl = _config.QueueUrl,
                    MessageBody = body
                };

                client.SendMessage(request);
            }
        }

        public IEnumerable<IQueueItem<T>> Dequeue(uint take)
        {
            return LoadNewMessages((int)take);
        }

        public void DeleteMessage(IQueueItem<T> message)
        {
            using (var client = GetClient())
            {
                var request = new DeleteMessageRequest
                {
                    QueueUrl = _config.QueueUrl,
                    ReceiptHandle = message.Receipt
                };

                client.DeleteMessage(request);
            }
        }
        #endregion
    }
}
