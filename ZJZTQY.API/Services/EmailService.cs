using MailKit.Net.Smtp;
using MimeKit;
using System.Net.Mail;

namespace ZJZTQY.API.Services
{
    public class EmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendCodeAsync(string toEmail, string code)
        {
            // 1. 读取配置
            var settings = _configuration.GetSection("EmailSettings");
            var host = settings["SmtpServer"];
            var port = int.Parse(settings["Port"] ?? "587");
            var mailFrom = settings["SenderEmail"];
            var password = settings["SenderPassword"];

            // 2. 构建邮件 (使用 MimeKit)
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("筑恒智 OA", mailFrom));
            message.To.Add(new MailboxAddress("", toEmail));
            message.Subject = "【筑恒智】登录验证码";

            message.Body = new TextPart("html")
            {
                Text = $"<h3>您的验证码是：<span style='color:blue;font-size:24px'>{code}</span></h3>" +
                       $"<p>有效期 5 分钟，请勿泄露给他人。</p>"
            };

            // 3. 发送 (使用 MailKit)
            using var client = new MailKit.Net.Smtp.SmtpClient();
            try
            {
                // 连接邮件服务器 (QQ邮箱通常需要 StartTls)
                await client.ConnectAsync(host, port, MailKit.Security.SecureSocketOptions.StartTls);

                // 登录
                await client.AuthenticateAsync(mailFrom, password);

                // 发送
                await client.SendAsync(message);
            }
            finally
            {
                await client.DisconnectAsync(true);
            }
        }
    }
}