using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Threading.Tasks;
using System.Windows;
using ZJZTQY.Helpers;
using ZJZTQY.Services;
using MessageBox = HandyControl.Controls.MessageBox;

namespace ZJZTQY.ViewModels
{
    public partial class LoginViewModel : ObservableObject
    {
        // ★ 变化1：只依赖 AuthService
        private readonly AuthService _authService;

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

        // ★ 变化2：构造函数注入 AuthService
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
            if (string.IsNullOrWhiteSpace(Email)) { MessageBox.Warning("请输入邮箱！"); return; }

            IsSendingCode = true;
            VerifyCodeButtonText = "发送中...";

            // ★ 变化3：调用 API 发送
            bool isSent = await _authService.SendCodeAsync(Email);

            if (isSent)
            {
                MessageBox.Success("验证码已发送，请查收邮件");
                // 简单的倒计时逻辑
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
                MessageBox.Error("发送失败，请检查网络或邮箱");
            }
            VerifyCodeButtonText = "获取验证码";
            IsSendingCode = false;
        }

        [RelayCommand(CanExecute = nameof(CanLogin))]
        private async Task Login()
        {
            if (!IsAgreed)
            {
                MessageBox.Warning("请先同意服务条款");
                return;
            }

            // ★ 变化4：调用 API 登录
            var result = await _authService.LoginAsync(Email, Code);

            if (result.IsSuccess)
            {
                // 保存 Token 而不是邮箱明文
                if (IsRemembered)
                {
                    SessionHelper.SaveUser(result.Token);
                }
                else
                {
                    SessionHelper.Clear();
                }

                MessageBox.Success("登录成功");
                await Task.Delay(500);
                NavigateToMainPageAction?.Invoke();
            }
            else
            {
                MessageBox.Error(result.Message);
            }
        }
    }
}