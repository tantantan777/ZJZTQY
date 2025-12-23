using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Windows;
using ZJZTQY.Helpers;

namespace ZJZTQY.ViewModels
{
    public partial class OaViewModel : ObservableObject
    {

        public Action? NavigateToLoginAction { get; set; }


        [RelayCommand]
        private void Logout()
        {

            SessionHelper.Clear();

 
            NavigateToLoginAction?.Invoke();
        }


        [RelayCommand]
        private void ExitApp()
        {

            Application.Current.Shutdown();
        }
    }
}