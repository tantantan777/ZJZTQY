using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Windows;
using ZJZTQY.Helpers;

namespace ZJZTQY.ViewModels
{
    public partial class OaViewModel : ObservableObject
    {
        // 定义一个委托，通知 View 层去执行“关闭窗口并打开登录页”的操作
        public Action? NavigateToLoginAction { get; set; }

        // 1. 退出登录 (测试清理 Token)
        [RelayCommand]
        private void Logout()
        {
            // 核心步骤：清除本地存储的 Token
            SessionHelper.Clear();

            // 通知 View 跳转
            NavigateToLoginAction?.Invoke();
        }

        // 2. 退出系统 (测试保留 Token)
        [RelayCommand]
        private void ExitApp()
        {
            // 直接关闭程序，不清理 Token
            Application.Current.Shutdown();
        }
    }
}