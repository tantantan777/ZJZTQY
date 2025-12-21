using System.Windows;
using ZJZTQY.Helpers;
using ZJZTQY.Views;

namespace ZJZTQY
{
    public partial class App : Application
    {
        // 可以在这里加一个全局用户变量，供 OA 使用
        // public static User? CurrentUser { get; set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // 1. 检查本地是否有“记住我”的存档
            string? savedEmail = SessionHelper.GetUser();

            if (!string.IsNullOrEmpty(savedEmail))
            {
                // ★ 情况A：有存档 -> 自动登录 -> 打开主页
                // (为了更安全，这里其实应该再调用一次数据库查一下用户是否存在，但作为演示这样足够了)

                var oaWindow = new Oa();
                oaWindow.Show();
            }
            else
            {
                // ★ 情况B：没存档 -> 打开登录页
                var loginWindow = new LoginWindow();
                loginWindow.Show();
            }
        }
    }
}