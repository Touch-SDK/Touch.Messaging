using Amazon.Runtime;
using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Touch.Notification;

namespace Touch.Messaging
{
    public sealed class ApnsNotificationDispatcher : INotificationDispatcher
    {
        #region .ctor
        public ApnsNotificationDispatcher(AWSCredentials credentials)
        {
            if (credentials == null) throw new ArgumentException("credentials");
            _credentials = credentials;
        }
        #endregion

        #region Data
        private readonly AWSCredentials _credentials;
        #endregion

        #region INotificationDispatcher members
        public void Dispatch(string deviceToken, string message, int count = 0, string data = null)
        {
            using (var client = new AmazonSimpleNotificationServiceClient(_credentials))
            {
                var endpoints = client.ListEndpointsByPlatformApplication(new ListEndpointsByPlatformApplicationRequest { PlatformApplicationArn = "arn:aws:sns:us-west-2:430104099533:app/GCM/OMS_GCM_Development" });
                


                var request = new PublishRequest
                { 
                    Message = message,
                    
                };
                client.Publish(request);
                throw new NotImplementedException();
            }
        }
        #endregion
    }
}
