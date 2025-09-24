using Core.Model.Settings;
using Core.Service.Interfaces;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks;

namespace Core.Service.DeployService
{
    public class MailService : IMailService
    {
        private readonly MailSettings _mailSettings;

        public MailService(IOptions<MailSettings> mailSettings)
        {
            _mailSettings = mailSettings.Value;
        }

        public async Task SendEmailAsync(string to, string subject, string body)
        {
            var email = new MimeMessage();
            email.From.Add(new MailboxAddress(_mailSettings.SenderName, _mailSettings.SenderEmail));
            email.To.Add(new MailboxAddress("", to));
            email.Subject = subject;
            email.Body = new TextPart("plain") { Text = body };

            using var smtp = new MailKit.Net.Smtp.SmtpClient();
            await smtp.ConnectAsync(_mailSettings.SmtpServer, _mailSettings.Port, _mailSettings.EnableSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.None);
            await smtp.AuthenticateAsync(_mailSettings.UserName, _mailSettings.Password);
            await smtp.SendAsync(email);
            await smtp.DisconnectAsync(true);
        }
    }
}
