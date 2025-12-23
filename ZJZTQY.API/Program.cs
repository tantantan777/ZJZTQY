using Microsoft.AspNetCore.Authentication.JwtBearer; // ★ 必须引用
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;              // ★ 必须引用
using System.Text;                                 // ★ 必须引用
using ZJZTQY.API.Data;
using ZJZTQY.API.Services;

var builder = WebApplication.CreateBuilder(args);

// --- 1. 注册服务 (Add Services) ---

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// A. 注册数据库
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// B. 注册基础服务
builder.Services.AddMemoryCache();
builder.Services.AddScoped<EmailService>();

// C. ★★★ 核心修复：注册 JWT 认证服务 ★★★
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = Encoding.ASCII.GetBytes(jwtSettings["SecretKey"]!);

builder.Services.AddAuthentication(options =>
{
    // 默认使用 JWT Bearer 认证
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,

        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(secretKey),

        // 允许的时间偏差 (防止服务器时间不同步导致的验证失败)
        ClockSkew = TimeSpan.Zero
    };
});

var app = builder.Build();

// --- 2. 配置管道 (Middleware) ---

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// app.UseHttpsRedirection(); // 开发阶段暂时关掉 HTTPS，避免证书麻烦

// ★★★ 核心修复：必须先 Authentication (认人)，再 Authorization (查权限) ★★★
// 顺序绝对不能反！
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();