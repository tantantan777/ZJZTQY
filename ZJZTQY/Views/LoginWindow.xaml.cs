using Microsoft.Extensions.DependencyInjection; // 引用它为了能获取 OA 窗口
using System;
using System.Diagnostics;
using System.Windows;
using ZJZTQY.ViewModels;

namespace ZJZTQY.Views
{
    public partial class LoginWindow : HandyControl.Controls.Window
    {
        // 构造函数：直接接收 ViewModel 和 ServiceProvider
        // 容器会自动把它们填进去
        public LoginWindow(LoginViewModel viewModel, IServiceProvider serviceProvider)
        {
            InitializeComponent();
            DataContext = viewModel;

            // 配置 ViewModel 中的跳转逻辑
            viewModel.NavigateToMainPageAction = () =>
            {
                // 使用 ServiceProvider 获取 OA 窗口实例 (这样 OA 窗口也能享受依赖注入)
                var oaWindow = serviceProvider.GetRequiredService<Oa>();
                oaWindow.Show();

                this.Close();
            };
        }

        // --- 超链接点击逻辑保持不变 ---
        private void OnServicePolicy(object sender, RoutedEventArgs e)
        {
            var path = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "Policies", "service.html");
            OpenUrl(path);
        }

        private void OnPrivacyPolicy(object sender, RoutedEventArgs e)
        {
            var path = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "Policies", "privacy.html");
            OpenUrl(path);
        }

        private void OpenUrl(string url)
        {
            try
            {
                Process.Start(new ProcessStartInfo { FileName = url, UseShellExecute = true });
            }
            catch (Exception ex)
            {
                HandyControl.Controls.MessageBox.Error($"无法打开链接: {ex.Message}");
            }
        }
    }
}