using Microsoft.Extensions.Options;
using MimeKit;
using MailKit.Net.Smtp; // 确保引用正确
using ZJZTQY.API.Settings;

namespace ZJZTQY.API.Services
{
    public class EmailService
    {
        private readonly EmailSettings _settings;

        public EmailService(IOptions<EmailSettings> settings)
        {
            _settings = settings.Value;
        }

        public async Task SendCodeAsync(string toEmail, string code)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("筑恒智 OA", _settings.SenderEmail));
            message.To.Add(new MailboxAddress("", toEmail));
            message.Subject = "【筑恒智】登录验证码";
            message.Body = new TextPart("html")
            {
                Text = $"<h3>您的验证码是：<span style='color:blue;font-size:24px'>{code}</span></h3><p>有效期 5 分钟。</p>"
            };

            using var client = new SmtpClient();
            try
            {
                await client.ConnectAsync(_settings.SmtpServer, _settings.Port, MailKit.Security.SecureSocketOptions.StartTls);
                await client.AuthenticateAsync(_settings.SenderEmail, _settings.SenderPassword);
                await client.SendAsync(message);
            }
            finally
            {
                await client.DisconnectAsync(true);
            }
        }
    }
}