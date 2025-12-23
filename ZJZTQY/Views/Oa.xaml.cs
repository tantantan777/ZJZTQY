using Microsoft.Extensions.DependencyInjection; // 用于获取 LoginWindow
using System;
using System.Windows;
using ZJZTQY.ViewModels;

namespace ZJZTQY.Views
{
    public partial class Oa : Window
    {
        // 构造函数注入 ViewModel 和 ServiceProvider
        public Oa(OaViewModel viewModel, IServiceProvider serviceProvider)
        {
            InitializeComponent();
            DataContext = viewModel;

            // 实现 ViewModel 定义的跳转逻辑
            viewModel.NavigateToLoginAction = () =>
            {
                // 1. 从容器中取出登录窗口 (自动注入依赖)
                var loginWindow = serviceProvider.GetRequiredService<LoginWindow>();
                loginWindow.Show();

                // 2. 关闭当前 OA 窗口
                this.Close();
            };
        }
    }
}