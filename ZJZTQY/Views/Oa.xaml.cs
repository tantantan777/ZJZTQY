using Microsoft.Extensions.DependencyInjection; // 用于获取 LoginWindow
using System;
using System.Windows;
using ZJZTQY.ViewModels;

namespace ZJZTQY.Views
{
    public partial class Oa : Window
    {

        public Oa(OaViewModel viewModel, IServiceProvider serviceProvider)
        {
            InitializeComponent();
            DataContext = viewModel;


            viewModel.NavigateToLoginAction = () =>
            {

                var loginWindow = serviceProvider.GetRequiredService<LoginWindow>();
                loginWindow.Show();

                this.Close();
            };
        }
    }
}