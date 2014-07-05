using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amazon;
using Amazon.Runtime;
using Amazon.SimpleNotificationService.Model;
using Touch.Notification;

namespace Touch.Messaging
{
    public abstract class AbstractNotificationBroadcaster : INotificationDispatcher
    {
        #region .ctor
        protected AbstractNotificationBroadcaster(AWSCredentials credentials, RegionEndpoint region, string applicationArn)
        {
            if (credentials == null) throw new ArgumentException("credentials");
            _credentials = credentials;

            if (region == null) throw new ArgumentException("region");
            _region = region;

            if (string.IsNullOrEmpty(applicationArn)) throw new ArgumentException("applicationArn");
            _applicationArn = applicationArn;
        }
        #endregion

        #region Data
        private readonly AWSCredentials _credentials;
        private readonly RegionEndpoint _region;
        private readonly string _applicationArn;

        private readonly object _lock = new object();
        private readonly Dictionary<string,string> _arnMap = new Dictionary<string, string>(); 
        #endregion

        public abstract void Dispatch(string deviceToken, string message, int count = 0, string data = null);

        protected void Broadcats(string message, string deviceToken)
        {
            using (var client = AWSClientFactory.CreateAmazonSimpleNotificationServiceClient(_credentials, _region))
            {
                string endpointArn = null;

                lock (_lock)
                {
                    if (!_arnMap.ContainsKey(deviceToken))
                    {
                        var response = client.ListEndpointsByPlatformApplication(new ListEndpointsByPlatformApplicationRequest
                        {
                            PlatformApplicationArn = _applicationArn
                        });

                        foreach (var endpoint in response.Endpoints)
                            _arnMap.Add(endpoint.Attributes["Token"], endpoint.EndpointArn);
                    }

                    if (!_arnMap.ContainsKey(deviceToken))
                    {
                        var response = client.CreatePlatformEndpoint(new CreatePlatformEndpointRequest
                        {
                            PlatformApplicationArn = _applicationArn,
                            Token = deviceToken
                        });

                        endpointArn = response.EndpointArn;
                        _arnMap.Add(deviceToken, endpointArn);
                    }
                }

                client.Publish(new PublishRequest
                {
                    Message = message,
                    TargetArn = endpointArn
                });
            }
        }
    }
}
