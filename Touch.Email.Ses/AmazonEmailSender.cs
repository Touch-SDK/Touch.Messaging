using System;
using System.Linq;
using Amazon;
using Amazon.Runtime;
using Amazon.SimpleEmail.Model;

namespace Touch.Email
{
    public sealed class AmazonEmailSender : IEmailSender
    {
        #region .ctor
        public AmazonEmailSender(AWSCredentials credentials, string connectionString)
        {
            if (credentials == null) throw new ArgumentNullException("credentials");
            if (string.IsNullOrWhiteSpace(connectionString)) throw new ArgumentNullException("connectionString");

            _config = new SesConnectionStringBuilder { ConnectionString = connectionString };
            if (string.IsNullOrWhiteSpace(_config.SenderAddress)) throw new ArgumentException("SenderAddress is not set.", "connectionString");

            _credentials = credentials;
        }
        #endregion

        #region Data
        private readonly SesConnectionStringBuilder _config;
        private readonly AWSCredentials _credentials;
        #endregion

        #region IEmailSender members
        public void Send(EmailMessage message)
        {
            if (message == null) throw new ArgumentNullException("message");

            var destination = new Destination { ToAddresses = message.Recipients.ToList() };

            var subject = new Content(message.Subject);
            var body = new Body(new Content(message.Body));

            var email = new Message(subject, body);

            var request = new SendEmailRequest
            {
                Destination = destination,
                Message = email,
                Source = _config.SenderAddress
            };

            using (var client = AWSClientFactory.CreateAmazonSimpleEmailServiceClient(_credentials, _config.Region))
            {
                client.SendEmail(request);
            }
        }
        #endregion
    }
}
