using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.ComponentModel.DataAnnotations;
using ZJZTQY.API.Services;
using ZJZTQY.API.Data;
using ZJZTQY.API.Models;

namespace ZJZTQY.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly EmailService _emailService;
        private readonly TokenService _tokenService; // 注入 TokenService
        private readonly IMemoryCache _cache;
        private readonly AppDbContext _context;

        // 构造函数注入所有依赖
        public AuthController(
            EmailService emailService,
            TokenService tokenService,
            IMemoryCache cache,
            AppDbContext context)
        {
            _emailService = emailService;
            _tokenService = tokenService;
            _cache = cache;
            _context = context;
        }

        // --- 请求模型定义 ---
        public class SendCodeRequest
        {
            [Required][EmailAddress] public string Email { get; set; } = string.Empty;
        }

        public class LoginRequest
        {
            [Required][EmailAddress] public string Email { get; set; } = string.Empty;
            [Required] public string Code { get; set; } = string.Empty;
        }

        // --- API 接口 ---

        [HttpGet("me")]
        [Microsoft.AspNetCore.Authorization.Authorize]
        public IActionResult GetMe()
        {
            // 从当前 Token 中解析用户信息
            var email = User.FindFirstValue(ClaimTypes.Email);
            var role = User.FindFirstValue(ClaimTypes.Role);
            var name = User.FindFirstValue(ClaimTypes.Name);
            var idStr = User.FindFirstValue(ClaimTypes.NameIdentifier);

            return Ok(new
            {
                id = idStr,
                email,
                role,
                username = name,
                message = "Token 有效"
            });
        }

        [HttpPost("send-code")]
        public async Task<IActionResult> SendCode([FromBody] SendCodeRequest request)
        {
            // 生成 6 位随机验证码
            var code = System.Security.Cryptography.RandomNumberGenerator.GetInt32(100000, 999999).ToString();

            // 存入缓存，有效期 5 分钟
            _cache.Set(request.Email, code, TimeSpan.FromMinutes(5));

            try
            {
                await _emailService.SendCodeAsync(request.Email, code);
                return Ok(new { message = "验证码已发送" });
            }
            catch (Exception ex)
            {
                // 实际生产中建议记录日志 (Logger) 而不是直接返回错误详情
                return BadRequest(new { message = $"发送失败: {ex.Message}" });
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            // 1. 校验验证码
            if (!_cache.TryGetValue(request.Email, out string? cachedCode) || cachedCode != request.Code)
            {
                return BadRequest(new { message = "验证码错误或已失效" });
            }

            // 验证通过，清除缓存（防止重复使用）
            _cache.Remove(request.Email);

            // 2. 查询或注册用户
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);

            if (user == null)
            {
                // 自动注册逻辑
                user = new User
                {
                    Email = request.Email,
                    Username = request.Email.Split('@')[0],
                    CreatedAt = DateTime.UtcNow,
                    Role = "User",
                    IsActive = true,
                    Company = "筑恒基石"
                };
                _context.Users.Add(user);
                await _context.SaveChangesAsync();
            }

            // 3. 生成 Token (调用 TokenService)
            var token = _tokenService.GenerateJwtToken(user);

            // 4. 返回结果
            return Ok(new
            {
                message = "登录成功",
                token = token,
                user = new { user.Username, user.Email, user.Role }
            });
        }
    }
}