using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Threading.Tasks;
using System.Windows;
using ZJZTQY.Helpers;
using ZJZTQY.Services;
using HandyControl.Controls; // ★ 1. 引入 HandyControl 控件命名空间

namespace ZJZTQY.ViewModels
{
    public partial class LoginViewModel : ObservableObject
    {
        private readonly AuthService _authService;
        private const string Token = "LoginToken"; // ★ 2. 定义 Token 常量，需与 XAML 中的 hc:Growl.Token 一致

        public Action? NavigateToMainPageAction { get; set; }

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(LoginCommand))]
        private string _email = string.Empty;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(LoginCommand))]
        private string _code = string.Empty;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(LoginCommand))]
        private bool _isAgreed = true;

        [ObservableProperty]
        private bool _isRemembered = true;

        [ObservableProperty]
        private string _verifyCodeButtonText = "获取验证码";

        [ObservableProperty]
        private bool _isSendingCode = false;

        public LoginViewModel(AuthService authService)
        {
            _authService = authService;
        }

        private bool CanLogin()
        {
            return !string.IsNullOrWhiteSpace(Email) &&
                   !string.IsNullOrWhiteSpace(Code) &&
                   IsAgreed;
        }

        [RelayCommand]
        private async Task GetCode()
        {
            // ★ 3. 替换 MessageBox 为 Growl
            if (string.IsNullOrWhiteSpace(Email))
            {
                Growl.Warning("请输入邮箱！", Token);
                return;
            }

            IsSendingCode = true;
            VerifyCodeButtonText = "发送中...";

            bool isSent = await _authService.SendCodeAsync(Email);

            if (isSent)
            {
                // ★ 替换 Success
                Growl.Success("验证码已发送，请查收邮件", Token);

                int s = 60;
                while (s > 0)
                {
                    VerifyCodeButtonText = $"{s}s 后重试";
                    await Task.Delay(1000);
                    s--;
                }
            }
            else
            {
                // ★ 替换 Error
                Growl.Error("发送失败，请检查网络或邮箱", Token);
            }
            VerifyCodeButtonText = "获取验证码";
            IsSendingCode = false;
        }

        [RelayCommand(CanExecute = nameof(CanLogin))]
        private async Task Login()
        {
            if (!IsAgreed)
            {
                // ★ 替换 Warning
                Growl.Warning("请先同意服务条款", Token);
                return;
            }

            var result = await _authService.LoginAsync(Email, Code);

            if (result.IsSuccess)
            {
                if (IsRemembered)
                {
                    SessionHelper.SaveUser(result.Token);
                }
                else
                {
                    SessionHelper.Clear();
                }

                // ★ 替换 Success
                Growl.Success("登录成功", Token);
                await Task.Delay(500);
                NavigateToMainPageAction?.Invoke();
            }
            else
            {
                // ★ 替换 Error
                Growl.Error(result.Message, Token);
            }
        }
    }
}