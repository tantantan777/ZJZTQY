using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Windows;

namespace ZJZTQY.Services
{
    public class AuthService
    {
        private readonly HttpClient _httpClient;


        private const string BaseUrl = "http://localhost:5142";
        public async Task<bool> CheckTokenAsync(string token)
        {
            try
            {
                // 设置请求头：Authorization: Bearer <token>
                _httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                // 请求刚才写的 /api/Auth/me 接口
                var response = await _httpClient.GetAsync("/api/Auth/me");

                return response.IsSuccessStatusCode; // 200 OK 表示有效，401 表示过期
            }
            catch
            {
                return false; // 网络不通也视为校验失败
            }
        }
        public AuthService()
        {
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri(BaseUrl);
        }

        // 定义接收数据的格式
        public record LoginResponse(string Message, string Token, UserDto User);
        public record UserDto(string Username, string Email, string Role);

        // 1. 发送验证码
        public async Task<bool> SendCodeAsync(string email)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("/api/Auth/send-code", new { Email = email });
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        // 2. 登录
        public async Task<(bool IsSuccess, string Message, string? Token)> LoginAsync(string email, string code)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("/api/Auth/login", new { Email = email, Code = code });

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
                    return (true, "登录成功", result?.Token);
                }
                else
                {
                    // 尝试读取后端返回的错误信息
                    var errorStr = await response.Content.ReadAsStringAsync();
                    return (false, "登录失败: " + errorStr, null);
                }
            }
            catch (Exception ex)
            {
                return (false, $"网络错误: {ex.Message}", null);
            }
        }
    }
}