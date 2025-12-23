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
                _httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var response = await _httpClient.GetAsync("/api/Auth/me");

                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }
        public AuthService()
        {
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri(BaseUrl);
        }


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