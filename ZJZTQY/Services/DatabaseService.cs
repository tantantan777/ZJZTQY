using Microsoft.EntityFrameworkCore;
using ZJZTQY.Data;
using ZJZTQY.Models;

namespace ZJZTQY.Services
{
    public class DatabaseService : IDatabaseService
    {
        public async Task<(bool IsSuccess, string Message, User? User)> LoginOrRegisterAsync(string email)
        {
            try
            {
                using var context = new AppDbContext();

                // 1. 查找数据库里有没有这个人
                var user = await context.Users.FirstOrDefaultAsync(u => u.Email == email);

                if (user != null)
                {
                    // A. 老用户：直接登录
                    return (true, "欢迎回来", user);
                }

                // B. 新用户：自动注册
                var newUser = new User
                {
                    Email = email,
                    Username = email.Split('@')[0], // 默认用邮箱前缀当名字
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true,
                    Role = "User"
                };

                context.Users.Add(newUser);
                await context.SaveChangesAsync();

                return (true, "新用户注册并登录成功", newUser);
            }
            catch (Exception ex)
            {
                return (false, $"数据库错误: {ex.Message}", null);
            }
        }
    }
}