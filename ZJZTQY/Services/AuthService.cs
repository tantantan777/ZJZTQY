using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration; // 务必安装 Microsoft.Extensions.Configuration.Binder 包

namespace ZJZTQY.Services
{
    public class AuthService
    {
        private readonly HttpClient _httpClient;

        // 构造函数注入 Factory 和 Configuration
        public AuthService(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            // 创建客户端实例
            _httpClient = httpClientFactory.CreateClient("ApiClient");

            // 从 appsettings.json 读取 BaseUrl，如果读取失败则使用默认值
            string baseUrl = configuration["ApiSettings:BaseUrl"] ?? "http://localhost:5142";
            _httpClient.BaseAddress = new Uri(baseUrl);
        }

        public async Task<bool> CheckTokenAsync(string token)
        {
            try
            {
                // 使用 HttpRequestMessage 构建请求，避免修改 _httpClient.DefaultRequestHeaders 导致并发问题
                var request = new HttpRequestMessage(HttpMethod.Get, "/api/Auth/me");
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var response = await _httpClient.SendAsync(request);

                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        // 定义数据传输对象 (DTO)
        public record LoginResponse(string Message, string Token, UserDto User);
        public record UserDto(string Username, string Email, string Role);

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