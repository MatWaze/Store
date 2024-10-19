using MimeKit;
using System.Net;
using MailKit.Net.Smtp;

namespace Store.Infrastructure
{
    public class SendEmailGoogle : ISendEmail
    {
        private readonly IConfiguration config;

        public SendEmailGoogle(IConfiguration conf) 
        {
            config = conf;
        }

        public async Task<string> SendEmailAsync(string emailName, string subject, string body)
        {
            var emailToSend = new MimeMessage();
            string accountName = config["GmailAccount:Name"];
            string appPassword = config["GmailAccount:AppPassword"];
            
            emailToSend.From.Add(MailboxAddress.Parse(accountName));
            emailToSend.To.Add(MailboxAddress.Parse(emailName));
            emailToSend.Subject = subject;
            emailToSend.Body = new TextPart(MimeKit.Text.TextFormat.Html)
            {
                Text = body
            };
            string response = String.Empty;
            using (var emailClient = new SmtpClient())
            {
                await emailClient.ConnectAsync("smtp.gmail.com", 587,
                    MailKit.Security.SecureSocketOptions.StartTls);
                await emailClient.AuthenticateAsync(accountName, appPassword);
                response = await emailClient.SendAsync(emailToSend);
                await emailClient.DisconnectAsync(true);
            }
            return response;
        }
    }
}
