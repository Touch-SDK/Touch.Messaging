﻿using System;
using System.Collections.Generic;
using Amazon;
using Amazon.Runtime;
using Amazon.SimpleNotificationService.Model;
using Touch.Serialization;

namespace Touch.Notification
{
    public class SnsTopicNotificationDispatcher<T> : INotificationDispatcher<T>
        where T : class, new()
    {
        #region .ctor
        public SnsTopicNotificationDispatcher(ISerializer jsonSerializer, AWSCredentials credentials, string connectionString)
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
        private readonly object _lock = new object();
        private readonly Dictionary<string, string> _arnMap = new Dictionary<string, string>();
        #endregion

        #region INotificationDispatcher members
        public void Dispatch(string recipient, T notification)
        {
            if (string.IsNullOrEmpty(recipient)) throw new ArgumentException("recipient");
            if (notification == null) throw new ArgumentNullException("notification");

            lock (_lock)
            {
                _arnMap.Clear();

                using (var client = AWSClientFactory.CreateAmazonSimpleNotificationServiceClient(_credentials, Config.Region))
                {
                    if (!_arnMap.ContainsKey(recipient))
                    {
                        var response = client.ListTopics();

                        foreach (var topic in response.Topics)
                        {
                            var i = topic.TopicArn.LastIndexOf(':') + 1;
                            var name = topic.TopicArn.Substring(i);
                            _arnMap[name] = topic.TopicArn;
                        }
                    }

                    if (!_arnMap.ContainsKey(recipient))
                        throw new ArgumentException("Topic not found: " + recipient, "recipient");

                    client.Publish(new PublishRequest
                    {
                        Message = CreateMessage(notification),
                        MessageStructure = "json",
                        TopicArn = _arnMap[recipient]
                    });
                }
            }
        }

        protected virtual string CreateMessage(T notification)
        {
            return JsonSerializer.Serialize(notification);
        }
        #endregion
    }
}
