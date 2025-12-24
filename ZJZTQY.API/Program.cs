using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using ZJZTQY.API.Data;
using ZJZTQY.API.Services;
using ZJZTQY.API.Settings; // 引用 Settings 命名空间

var builder = WebApplication.CreateBuilder(args);

// --- 1. 基础服务注册 ---
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// --- 2. 数据库配置 ---
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// --- 3. 缓存与业务服务注册 ---
builder.Services.AddMemoryCache();

// 注册强类型配置 (Options Pattern)
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));

// 注册业务服务
builder.Services.AddScoped<EmailService>();
builder.Services.AddScoped<TokenService>(); // 新增：Token 生成服务

// --- 4. JWT 认证配置 ---
// 临时读取配置以设置 JWT 参数
var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>();

// 防止配置读取失败导致启动崩溃
if (jwtSettings == null || string.IsNullOrEmpty(jwtSettings.SecretKey))
{
    throw new InvalidOperationException("JwtSettings is not configured correctly in appsettings.json");
}

var secretKey = Encoding.ASCII.GetBytes(jwtSettings.SecretKey);

builder.Services.AddAuthentication(options =>
{
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

        ValidIssuer = jwtSettings.Issuer,
        ValidAudience = jwtSettings.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(secretKey),

        ClockSkew = TimeSpan.Zero // 消除默认的 5 分钟时间偏差
    };
});

var app = builder.Build();

// --- 5. 中间件管道配置 ---
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// 必须先认证，后授权
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();