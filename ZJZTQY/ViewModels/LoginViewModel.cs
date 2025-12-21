using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Windows;
using ZJZTQY.Helpers;
using ZJZTQY.Services;
using MessageBox = HandyControl.Controls.MessageBox;

namespace ZJZTQY.ViewModels
{
    public partial class LoginPageViewModel : ObservableObject
    {
        private readonly EmailService _emailService;
        private readonly IDatabaseService _dbService;

        public Action? NavigateToMainPageAction { get; set; }

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(LoginCommand))]
        private string _email = string.Empty;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(LoginCommand))]
        private string _code = string.Empty;


        // ★ 新增：同意协议状态
        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(LoginCommand))]
        private bool _isAgreed = true;

        [ObservableProperty]
        private bool _isRemembered = true;


        [ObservableProperty]
        private string _verifyCodeButtonText = "获取验证码";

        [ObservableProperty]
        private bool _isSendingCode = false;

        public LoginPageViewModel(EmailService emailService, IDatabaseService dbService)
        {
            _emailService = emailService;
            _dbService = dbService;
        }

        // ★ 修改：现在必须同时满足 3 个条件才能点登录
        private bool CanLogin()
        {
            bool hasEmail = !string.IsNullOrWhiteSpace(Email);
            bool hasCode = !string.IsNullOrWhiteSpace(Code);
            return hasEmail && hasCode && IsAgreed; // 必须勾选协议
        }

        [RelayCommand]
        private async Task GetCode()
        {
            if (string.IsNullOrWhiteSpace(Email)) { MessageBox.Warning("请输入邮箱！"); return; }

            IsSendingCode = true;
            VerifyCodeButtonText = "发送中...";

            bool isSent = await _emailService.SendCodeAsync(Email);
            if (isSent)
            {
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
                MessageBox.Error("发送失败");
            }
            VerifyCodeButtonText = "获取验证码";
            IsSendingCode = false;
        }

        [RelayCommand(CanExecute = nameof(CanLogin))]
        private async Task Login()
        {
            // 双重保险：再次检查协议
            if (!IsAgreed)
            {
                MessageBox.Warning("请先同意服务条款");
                return;
            }

            if (!_emailService.VerifyCode(Email, Code))
            {
                MessageBox.Error("验证码错误或已过期");
                return;
            }

            var result = await _dbService.LoginOrRegisterAsync(Email);

            if (result.IsSuccess)
            {
                if (IsRemembered)
                {
                    SessionHelper.SaveUser(Email); // 保存邮箱到本地
                }
                else
                {
                    SessionHelper.Clear(); // 如果没勾选，确保清除旧的
                }

                // TODO: 建议把 result.User 赋值给全局变量，例如 App.CurrentUser = result.User;

                MessageBox.Success(result.Message);
                await Task.Delay(1000);
                NavigateToMainPageAction?.Invoke();

            }
            else
            {
                MessageBox.Error(result.Message);
            }
        }
    }
}