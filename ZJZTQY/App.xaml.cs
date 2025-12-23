using System.Windows;
using ZJZTQY.Helpers;
using ZJZTQY.Services;
using ZJZTQY.Views;

namespace ZJZTQY
{
    public partial class App : Application
    {
        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // 1. 读取本地 Token
            string? token = SessionHelper.GetUser();

            bool isValid = false;

            if (!string.IsNullOrEmpty(token))
            {
                // 2. 如果有 Token，调用 API 校验是否过期
                var authService = new AuthService();
                isValid = await authService.CheckTokenAsync(token);
            }

            if (isValid)
            {
                // ★ 情况A：Token 有效 -> 进入主页
                var oaWindow = new Oa();
                oaWindow.Show();
            }
            else
            {
                // ★ 情况B：无 Token 或已过期 -> 清除残留 -> 进入登录页
                SessionHelper.Clear();

                var loginWindow = new LoginWindow();
                loginWindow.Show();
            }
        }
    }
}