using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.ComponentModel.DataAnnotations;
using ZJZTQY.API.Services;
using ZJZTQY.API.Data;
using ZJZTQY.API.Models;
using ZJZTQY.API.Extensions; // 确保引用了扩展方法命名空间

namespace ZJZTQY.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly EmailService _emailService;
        private readonly TokenService _tokenService;
        private readonly IMemoryCache _cache;
        private readonly AppDbContext _context;

        public AuthController(EmailService emailService, TokenService tokenService, IMemoryCache cache, AppDbContext context)
        {
            _emailService = emailService;
            _tokenService = tokenService;
            _cache = cache;
            _context = context;
        }

        // --- 请求模型 ---
        public class SendCodeRequest
        {
            [Required][EmailAddress] public string Email { get; set; } = string.Empty;
        }

        public class LoginRequest
        {
            [Required][EmailAddress] public string Email { get; set; } = string.Empty;
            [Required] public string Code { get; set; } = string.Empty;
        }

        // --- 接口方法 ---

        [HttpGet("me")]
        [Microsoft.AspNetCore.Authorization.Authorize]
        public IActionResult GetMe()
        {
            // 使用扩展方法获取用户信息，代码更简洁
            var email = User.GetEmail();
            var name = User.GetUsername();
            var id = User.GetUserId();
            // 如果 Extension 中没有 GetRole，可以使用原生方法
            var role = User.FindFirstValue(ClaimTypes.Role);

            return Ok(new
            {
                id,
                email,
                role,
                username = name,
                message = "Token 有效"
            });
        }

        [HttpPost("send-code")]
        public async Task<IActionResult> SendCode([FromBody] SendCodeRequest request)
        {
            var code = System.Security.Cryptography.RandomNumberGenerator.GetInt32(100000, 999999).ToString();

            // 缓存 5 分钟
            _cache.Set(request.Email, code, TimeSpan.FromMinutes(5));

            try
            {
                await _emailService.SendCodeAsync(request.Email, code);
                return Ok(new { message = "验证码已发送" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"发送失败: {ex.Message}" });
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            // 1. 验证码校验
            if (!_cache.TryGetValue(request.Email, out string? cachedCode) || cachedCode != request.Code)
            {
                return BadRequest(new { message = "验证码错误或已失效" });
            }
            _cache.Remove(request.Email);

            // 2. 查找或注册用户
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);

            if (user == null)
            {
                // 【修改点】新用户注册：只存邮箱、时间、在线状态
                // Username 是必填项，暂用邮箱前缀填充，否则数据库报错
                user = new User
                {
                    Email = request.Email,
                    Username = request.Email.Split('@')[0],
                    CreatedAt = DateTime.UtcNow,
                    IsOnline = true, // 注册即登录 -> 在线
                    IsActive = true  // 激活状态
                    // 其他字段全部留空 (string.Empty)
                };
                _context.Users.Add(user);
            }
            else
            {
                // 【修改点】老用户登录：更新为在线状态
                user.IsOnline = true;
                _context.Users.Update(user); // 确保标记为修改
            }

            await _context.SaveChangesAsync();

            // 3. 生成 Token
            var token = _tokenService.GenerateJwtToken(user);

            return Ok(new
            {
                message = "登录成功",
                token = token,
                user = new { user.Username, user.Email, user.Role }
            });
        }

        // 【新增点】退出登录接口
        [HttpPost("logout")]
        [Microsoft.AspNetCore.Authorization.Authorize] // 需要登录才能退出
        public async Task<IActionResult> Logout()
        {
            var userId = User.GetUserId();
            var user = await _context.Users.FindAsync(userId);

            if (user != null)
            {
                user.IsOnline = false; // 设置为离线
                await _context.SaveChangesAsync();
            }

            return Ok(new { message = "已退出登录" });
        }
    }
}