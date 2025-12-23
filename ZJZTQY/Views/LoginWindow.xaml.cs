using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using ZJZTQY.Services;
using ZJZTQY.ViewModels;
using ZJZTQY.Views;

namespace ZJZTQY.Views
{
    public partial class LoginWindow : HandyControl.Controls.Window
    {
        public LoginWindow()
        {
            InitializeComponent();

            // ★★★ 重构点：不再读取 appsettings，也不再创建 Email/Database 服务 ★★★

            // 1. 创建新的 HTTP 认证服务
            var authService = new AuthService();

            // 2. 注入 ViewModel (现在只需要 AuthService)
            var viewModel = new LoginViewModel(authService);

            // 3. 处理跳转逻辑：登录成功后 -> 打开 OA 主页，关闭登录窗口
            viewModel.NavigateToMainPageAction = () =>
            {
                // 创建 OA 主窗口
                var oaWindow = new Oa();
                oaWindow.Show();

                // 关闭当前的登录窗口
                this.Close();
            };

            // 4. 绑定上下文
            DataContext = viewModel;
        }

        // --- 下面是保留的原有逻辑（处理超链接点击） ---

        private void OnServicePolicy(object sender, RoutedEventArgs e)
        {
            // 注意：确保你的 Assets/Policies/service.html 文件存在
            var path = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "Policies", "service.html");
            OpenUrl(path);
        }

        private void OnPrivacyPolicy(object sender, RoutedEventArgs e)
        {
            // 注意：确保你的 Assets/Policies/privacy.html 文件存在
            var path = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "Policies", "privacy.html");
            OpenUrl(path);
        }

        private void OpenUrl(string url)
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = url,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                HandyControl.Controls.MessageBox.Error($"无法打开链接: {ex.Message}");
            }
        }
    }
}