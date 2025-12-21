using Microsoft.Extensions.Configuration;
using System.Net;
using System.Net.Mail;

namespace ZJZTQY.Services
{
    public class EmailService
    {
        private readonly IConfiguration _configuration;
        // 简单起见，我们在内存中存储验证码 (Key=邮箱, Value=验证码)
        // 生产环境通常会用 Redis
        private static readonly Dictionary<string, string> _verificationCodes = new();

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        /// <summary>
        /// 发送验证码
        /// </summary>
        public async Task<bool> SendCodeAsync(string toEmail)
        {
            try
            {
                // 1. 生成6位随机数字
                var code = Random.Shared.Next(100000, 999999).ToString();

                // 2. 读取配置
                var settings = _configuration.GetSection("EmailSettings");
                var host = settings["SmtpServer"];
                var port = int.Parse(settings["Port"] ?? "587");
                var mailFrom = settings["SenderEmail"];
                var pwd = settings["SenderPassword"];

                // 3. 构建邮件
                var message = new MailMessage
                {
                    From = new MailAddress(mailFrom!, "TQY 系统中心"),
                    Subject = "【TQY】注册验证码",
                    Body = $"<h3>您的验证码是：<span style='color:blue'>{code}</span></h3><p>有效期5分钟，请勿泄露。</p>",
                    IsBodyHtml = true
                };
                message.To.Add(toEmail);

                // 4. 发送
                using var client = new SmtpClient(host, port)
                {
                    Credentials = new NetworkCredential(mailFrom, pwd),
                    EnableSsl = true // QQ邮箱必须开启
                };

                await client.SendMailAsync(message);

                // 5. 存入缓存 (先删除旧的)
                if (_verificationCodes.ContainsKey(toEmail))
                    _verificationCodes.Remove(toEmail);

                _verificationCodes.Add(toEmail, code);

                return true;
            }
            catch (Exception ex)
            {
                // 实际开发中应该记录日志
                System.Diagnostics.Debug.WriteLine($"发送失败: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 校验验证码是否正确
        /// </summary>
        public bool VerifyCode(string email, string code)
        {
            if (_verificationCodes.TryGetValue(email, out var correctCode))
            {
                if (correctCode == code)
                {
                    _verificationCodes.Remove(email); // 验证一次即销毁，防止重复使用
                    return true;
                }
            }
            return false;
        }
    }
}