using Microsoft.Extensions.DependencyInjection;
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
                var oaWindow = serviceProvider.GetRequiredService<Oa>();
                oaWindow.Show();
                this.Close();
            };
        }

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