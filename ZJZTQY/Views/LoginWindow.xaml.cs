using Microsoft.Extensions.Configuration;
using System.Diagnostics;
using System.IO;
using System.Windows;
using ZJZTQY.Services;
using ZJZTQY.ViewModels;
using ZJZTQY.Views; // 引用 OA 页面所在的命名空间

namespace ZJZTQY.Views
{
    public partial class LoginWindow : HandyControl.Controls.Window
    {
        public LoginWindow()
        {
            InitializeComponent();

            // 1. 读取配置
            var builder = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
            IConfiguration config = builder.Build();

            // 2. 创建服务
            var emailService = new EmailService(config);
            var dbService = new DatabaseService();

            // 3. 注入 ViewModel (我们可以继续复用 LoginPageViewModel，逻辑是一样的)
            var viewModel = new LoginPageViewModel(emailService, dbService);

            // 4. 处理跳转：登录成功后 -> 打开 OA 主页，关闭登录窗口
            viewModel.NavigateToMainPageAction = () =>
            {
                // 创建 OA 主窗口
                var oaWindow = new Oa();
                oaWindow.Show();

                // 关闭当前的登录窗口
                this.Close();
            };

            DataContext = viewModel;
        }

        // --- 下面是原来的点击事件 ---

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