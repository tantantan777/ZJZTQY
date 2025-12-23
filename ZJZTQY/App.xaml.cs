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
            AppHost = Host.CreateDefaultBuilder()
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddSingleton<AuthService>();

                    services.AddTransient<LoginWindow>();
                    services.AddTransient<Oa>();
                    services.AddTransient<OaViewModel>();


                    services.AddTransient<LoginViewModel>(provider =>
                    {

                        var authService = provider.GetRequiredService<AuthService>();

                        return new LoginViewModel(authService, "LoginToken");
                    });
                })
                .Build();
        }

        protected override async void OnStartup(StartupEventArgs e)
        {
            await AppHost!.StartAsync();

            var authService = AppHost.Services.GetRequiredService<AuthService>();
            string? token = Helpers.SessionHelper.GetUser();

            bool isValid = false;
            if (!string.IsNullOrEmpty(token))
            {
                isValid = await authService.CheckTokenAsync(token);
            }

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
            await AppHost!.StopAsync();
            base.OnExit(e);
        }
    }
}