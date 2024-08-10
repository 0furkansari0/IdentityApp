
using System.Net;
using System.Net.Mail;

namespace IdentityApp.Models
{
    public class SmtpEmailSender : IEmailSender
    {
        private string? _host;
        private int _port;
        private bool _enbleSSL;
        private string? _userName;
        private string? _password;
        public SmtpEmailSender(string? host, int port, bool enbleSSL, string? userName, string? password)
        {
            _host = host;
            _port = port;
            _enbleSSL = enbleSSL;
            _userName = userName;
            _password = password;
        }
        public Task SendEmainAsync(string email, string subject, string message)
        {
            var client = new SmtpClient(_host, _port)
            {
                Credentials = new NetworkCredential(_userName, _password),
                EnableSsl = _enbleSSL
            };

            return client.SendMailAsync(new MailMessage(_userName ?? "", email, subject, message) { IsBodyHtml = true});
        }
    }
}
