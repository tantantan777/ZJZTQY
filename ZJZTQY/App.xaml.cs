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
        // 定义一个静态属性，方便在某些极端情况下访问（通常推荐构造函数注入）
        public static IHost? AppHost { get; private set; }

        public App()
        {
            AppHost = Host.CreateDefaultBuilder()
                .ConfigureServices((hostContext, services) =>
                {
                    // 1. 注册服务 (Service) - 单例模式，复用 HttpClient
                    services.AddSingleton<AuthService>();

                    // 2. 注册窗口 (View) - 瞬态模式，每次打开都是新的
                    services.AddTransient<LoginWindow>();
                    services.AddTransient<Oa>();

                    // 3. 注册视图模型 (ViewModel)
                    services.AddTransient<LoginViewModel>();
                    services.AddTransient<OaViewModel>();
                })
                .Build();
        }

        protected override async void OnStartup(StartupEventArgs e)
        {
            await AppHost!.StartAsync();

            // --- 下面是之前的启动逻辑，稍微改了一点点 ---

            // 从容器中获取 AuthService，而不是 new
            var authService = AppHost.Services.GetRequiredService<AuthService>();

            string? token = Helpers.SessionHelper.GetUser();
            bool isValid = false;

            if (!string.IsNullOrEmpty(token))
            {
                isValid = await authService.CheckTokenAsync(token);
            }

            if (isValid)
            {
                // 从容器中获取 OA 窗口
                var oaWindow = AppHost.Services.GetRequiredService<Oa>();
                oaWindow.Show();
            }
            else
            {
                Helpers.SessionHelper.Clear();
                // 从容器中获取 登录窗口
                var loginWindow = AppHost.Services.GetRequiredService<LoginWindow>();
                loginWindow.Show();
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