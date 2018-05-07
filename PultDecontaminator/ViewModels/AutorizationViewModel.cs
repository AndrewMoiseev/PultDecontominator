using System;
using Prism.Commands;
using Prism.Mvvm;
using System.Windows;

namespace PultDecontominator.ViewModels
{
    public class AutorizationViewModel : BindableBase
    {
        public Action CloseAction { get; set; }
        public DelegateCommand LoginDelegateCommand { get; private set; }
        public int CntTryPassword { get; set; }

        public string Name
        {
            get { return _name; }
            set { SetProperty(ref _name, value); }
        }
        public string Password
        {
            get { return _passw; }
            set { SetProperty(ref _passw, value); }
        }

        public AutorizationViewModel()
        {
            LoginDelegateCommand = new DelegateCommand(CloseAuth );
            CntTryPassword = 0;
        }

        private void CloseAuth()
        {
            if (Password == "12345")
            {
                Application.Current.MainWindow.Show();
                CloseAction();
            }
            else
            {
                CntTryPassword++;
                MessageBox.Show("Неверных попыток: " + CntTryPassword.ToString());
            }
        }
        private string _name;
        private string _passw;
    }
}