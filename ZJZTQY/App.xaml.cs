using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using System.Windows;
using ZJZTQY.Services;
using ZJZTQY.ViewModels;
using ZJZTQY.Views;

namespace ZJZTQY
{
    public partial class App : Application
    {
        public static IHost? AppHost { get; private set; }

        public App()
        {
            AppHost = Host.CreateDefaultBuilder()
                // Host.CreateDefaultBuilder 默认会自动加载 appsettings.json
                .ConfigureServices((hostContext, services) =>
                {
                    // 1. 注册 HttpClient，命名为 "ApiClient"
                    services.AddHttpClient("ApiClient");

                    // 2. 注册 AuthService 为单例
                    services.AddSingleton<AuthService>();

                    // 3. 注册 UI 窗口 (Transient 表示每次请求都创建新实例)
                    services.AddTransient<LoginWindow>();
                    services.AddTransient<Oa>();

                    // 4. 注册 ViewModels
                    services.AddTransient<OaViewModel>();

                    // 注册 LoginViewModel，需要手动注入 AuthService 和 token 占位符
                    services.AddTransient<LoginViewModel>(provider =>
                    {
                        var authService = provider.GetRequiredService<AuthService>();
                        // 这里传入 "LoginToken" 只是为了匹配 ViewModel 构造函数，实际逻辑中它可能仅用于日志或占位
                        return new LoginViewModel(authService, "LoginToken");
                    });
                })
                .Build();
        }

        protected override async void OnStartup(StartupEventArgs e)
        {
            await AppHost!.StartAsync();

            // 从容器中获取 AuthService
            var authService = AppHost.Services.GetRequiredService<AuthService>();

            // 尝试从本地 Session 获取 Token
            string? token = Helpers.SessionHelper.GetUser();

            bool isValid = false;
            if (!string.IsNullOrEmpty(token))
            {
                // 校验 Token 有效性
                isValid = await authService.CheckTokenAsync(token);
            }

            // 根据校验结果跳转
            if (isValid)
            {
                AppHost.Services.GetRequiredService<Oa>().Show();
            }
            else
            {
                AppHost.Services.GetRequiredService<LoginWindow>().Show();
            }

            base.OnStartup(e);
        }

        protected override async void OnExit(ExitEventArgs e)
        {
            // 获取当前保存的 Token
            string? token = Helpers.SessionHelper.GetUser();

            if (!string.IsNullOrEmpty(token))
            {
                var authService = AppHost!.Services.GetRequiredService<AuthService>();
                // 调用后端 API 设置为离线
                // 注意：OnExit 是同步方法，这里尽量快速执行，或者使用 Fire-and-forget
                await authService.LogoutAsync(token);
            }

            using (AppHost)
            {
                await AppHost!.StopAsync();
            }
            base.OnExit(e);
        }
    }
}