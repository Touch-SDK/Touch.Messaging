using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amazon;
using Amazon.Runtime;
using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using Touch.Notification;

namespace Touch.Messaging
{
    public abstract class AbstractNotificationBroadcaster : INotificationDispatcher, IDisposable
    {
        #region .ctor
        protected AbstractNotificationBroadcaster(AWSCredentials credentials, string connectionString)
        {
            if (credentials == null) throw new ArgumentException("credentials");

            if (string.IsNullOrEmpty(connectionString)) throw new ArgumentException("connectionString");
            Config = new SnsConnectionStringBuilder { ConnectionString = connectionString };
            if (string.IsNullOrWhiteSpace(Config.Application)) throw new ArgumentException("Application ARN is not set.", "connectionString");

            _client = AWSClientFactory.CreateAmazonSimpleNotificationServiceClient(credentials, Config.Region);
        }
        #endregion

        #region Data
        protected readonly SnsConnectionStringBuilder Config;

        private readonly object _lock = new object();
        private readonly Dictionary<string,string> _arnMap = new Dictionary<string, string>();

        private readonly IAmazonSimpleNotificationService _client;
        #endregion

        public abstract void Dispatch(string deviceToken, string message, int count = 0, string data = null);

        protected void Broadcats(string message, string deviceToken)
        {
            string endpointArn = null;

            lock (_lock)
            {
                _arnMap.Clear();

                if (!_arnMap.ContainsKey(deviceToken))
                {
                    var response = _client.ListEndpointsByPlatformApplication(new ListEndpointsByPlatformApplicationRequest
                    {
                        PlatformApplicationArn = Config.Application
                    });

                    foreach (var endpoint in response.Endpoints)
                        _arnMap[endpoint.Attributes["Token"]] = endpoint.EndpointArn;
                }

                if (!_arnMap.ContainsKey(deviceToken))
                {
                    var response = _client.CreatePlatformEndpoint(new CreatePlatformEndpointRequest
                    {
                        PlatformApplicationArn = Config.Application,
                        Token = deviceToken
                    });

                    _arnMap[deviceToken] = response.EndpointArn;
                }

                endpointArn = _arnMap[deviceToken];
            }

            _client.Publish(new PublishRequest
            {
                Message = message,
                MessageStructure = "json",
                TargetArn = endpointArn
            });
        }

        public void Dispose()
        {
            _client.Dispose();
        }
    }
}
