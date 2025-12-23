using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
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
            // ★★★ 核心：配置容器 ★★★
            AppHost = Host.CreateDefaultBuilder()
                .ConfigureServices((hostContext, services) =>
                {
                    // 1. 服务：单例模式 (HttpClient 复用)
                    services.AddSingleton<AuthService>();

                    // 2. 窗口与ViewModel：瞬态模式 (每次打开都是新的)
                    services.AddTransient<LoginWindow>();
                    services.AddTransient<LoginViewModel>();

                    services.AddTransient<Oa>();
                    services.AddTransient<OaViewModel>();
                })
                .Build();
        }

        protected override async void OnStartup(StartupEventArgs e)
        {
            await AppHost!.StartAsync();

            // 从容器获取服务，而不是 new
            var authService = AppHost.Services.GetRequiredService<AuthService>();

            string? token = Helpers.SessionHelper.GetUser();
            bool isValid = false;

            if (!string.IsNullOrEmpty(token))
            {
                isValid = await authService.CheckTokenAsync(token);
            }

            if (isValid)
            {
                // 使用 DI 启动主页
                AppHost.Services.GetRequiredService<Oa>().Show();
            }
            else
            {
                Helpers.SessionHelper.Clear();
                // 使用 DI 启动登录页
                AppHost.Services.GetRequiredService<LoginWindow>().Show();
            }

            base.OnStartup(e);
        }

        protected override async void OnExit(ExitEventArgs e)
        {
            await AppHost!.StopAsync();
            base.OnExit(e);
        }
    }
}