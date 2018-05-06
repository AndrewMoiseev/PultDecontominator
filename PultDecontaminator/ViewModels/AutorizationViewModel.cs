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

        private string _name;
        public string Name
        {
            get { return _name; }
            set { SetProperty(ref _name, value); }
        }

        public AutorizationViewModel()
        {
            LoginDelegateCommand = new DelegateCommand(CloseAuth, CanExecute);
        }

        private void CloseAuth()
        {
            Application.Current.MainWindow.Show();
            CloseAction();
        }
        private bool CanExecute()
        {
            return true;
        }
    }
}