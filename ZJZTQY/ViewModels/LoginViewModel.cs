using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Threading.Tasks;
using System.Windows;
using ZJZTQY.Helpers;
using ZJZTQY.Services;
using HandyControl.Controls;
using HandyControl.Data;

namespace ZJZTQY.ViewModels
{
    public partial class LoginViewModel : ObservableObject
    {
        private readonly AuthService _authService;

        private readonly string _token;

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

        [ObservableProperty]
        private string _loginButtonContent = "登 录 / 注 册";

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(LoginCommand))]
        private bool _isLoggingIn = false;

        public LoginViewModel(AuthService authService, string token)
        {
            _authService = authService;
            _token = token;
        }

        private bool CanLogin()
        {
            return !IsLoggingIn &&
                   !string.IsNullOrWhiteSpace(Email) &&
                   !string.IsNullOrWhiteSpace(Code) &&
                   IsAgreed;
        }

        private GrowlInfo CreateMsg(string msg)
        {
            return new GrowlInfo
            {
                Message = msg,
                Token = _token,
                ShowCloseButton = true
            };
        }

        [RelayCommand]
        private async Task GetCode()
        {
            if (string.IsNullOrWhiteSpace(Email))
            {
                Growl.Warning(CreateMsg("请输入邮箱！"));
                return;
            }

            IsSendingCode = true;
            VerifyCodeButtonText = "发送中...";

            bool isSent = await _authService.SendCodeAsync(Email);

            if (isSent)
            {
                Growl.Success(CreateMsg("验证码已发送，请查收邮件"));

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
                Growl.Error(CreateMsg("发送失败，请检查网络或邮箱"));
            }
            VerifyCodeButtonText = "获取验证码";
            IsSendingCode = false;
        }

        [RelayCommand(CanExecute = nameof(CanLogin))]
        private async Task Login()
        {
            if (!IsAgreed)
            {
                Growl.Warning(CreateMsg("请先同意服务条款"));
                return;
            }

            IsLoggingIn = true;
            LoginButtonContent = "正在登录...";

            try
            {
                var result = await _authService.LoginAsync(Email, Code);

                if (result.IsSuccess)
                {
                    if (IsRemembered) SessionHelper.SaveUser(result.Token);
                    else SessionHelper.Clear();

                    Growl.Success(CreateMsg("登录成功"));

                    await Task.Delay(500);
                    NavigateToMainPageAction?.Invoke();
                }
                else
                {
                    Growl.Error(CreateMsg(result.Message));
                }
            }
            finally
            {
                IsLoggingIn = false;
                LoginButtonContent = "登 录 / 注 册";
            }
        }
    }
}