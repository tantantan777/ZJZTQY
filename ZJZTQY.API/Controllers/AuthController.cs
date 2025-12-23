using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using ZJZTQY.API.Services;
using ZJZTQY.API.Data;  
using ZJZTQY.API.Models;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ZJZTQY.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly EmailService _emailService;
        private readonly IMemoryCache _cache;
        private readonly AppDbContext _context;      
        private readonly IConfiguration _configuration;


        public AuthController(EmailService emailService, IMemoryCache cache, AppDbContext context, IConfiguration configuration)
        {
            _emailService = emailService;
            _cache = cache;
            _context = context;
            _configuration = configuration;
        }

        public class SendCodeRequest
        {
            [Required][EmailAddress] public string Email { get; set; } = string.Empty;
        }
        [HttpGet("me")]
        [Microsoft.AspNetCore.Authorization.Authorize] 
        public IActionResult GetMe()
        {

            var email = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
            var role = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
            var name = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;

            return Ok(new
            {
                email,
                role,
                username = name,
                message = "Token 有效"
            });
        }

        public class LoginRequest
        {
            [Required][EmailAddress] public string Email { get; set; } = string.Empty;
            [Required] public string Code { get; set; } = string.Empty;
        }

        [HttpPost("send-code")]
        public async Task<IActionResult> SendCode([FromBody] SendCodeRequest request)
        {
            var code = System.Security.Cryptography.RandomNumberGenerator.GetInt32(100000, 999999).ToString();
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

            if (!_cache.TryGetValue(request.Email, out string? cachedCode) || cachedCode != request.Code)
            {
                return BadRequest(new { message = "验证码错误或已失效" });
            }

 
            _cache.Remove(request.Email);

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);

            if (user == null)
            {
 
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


            var token = GenerateJwtToken(user);


            return Ok(new
            {
                message = "登录成功",
                token = token,
                user = new { user.Username, user.Email, user.Role } 
            });
        }


        private string GenerateJwtToken(User user)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var key = Encoding.ASCII.GetBytes(jwtSettings["SecretKey"]!);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim(ClaimTypes.Role, user.Role)
                }),
                Expires = DateTime.UtcNow.AddMinutes(double.Parse(jwtSettings["ExpireMinutes"]!)),
                Issuer = jwtSettings["Issuer"],
                Audience = jwtSettings["Audience"],
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}