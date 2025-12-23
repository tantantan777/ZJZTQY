using Microsoft.Extensions.DependencyInjection; // 引用它为了能获取 OA 窗口
using System;
using System.Diagnostics;
using System.Windows;
using ZJZTQY.ViewModels;

namespace ZJZTQY.Views
{
    public partial class LoginWindow : HandyControl.Controls.Window
    {

        public LoginWindow(LoginViewModel viewModel, IServiceProvider serviceProvider)
        {
            InitializeComponent();
            DataContext = viewModel;

            viewModel.NavigateToMainPageAction = () =>
            {
                // 跳转时也使用 DI 获取新窗口
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