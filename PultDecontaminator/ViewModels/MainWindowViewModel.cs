using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Linq;
using Prism.Commands;
using Prism.Mvvm;
using PultDecontominator.Services;

namespace PultDecontominator.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        public bool AuthTrue { get; set; }

        public List<string> Decontaminators
        {
            get { return _decontominators; }
        }

        public bool IsEnabled
        {
            get { return _isEnabled; }
            set
            {
                SetProperty(ref _isEnabled, value);
                ExecuteStartDryCommand.RaiseCanExecuteChanged();
                ExecuteStartDecontominationCommand.RaiseCanExecuteChanged();
                ExecuteStopCommand.RaiseCanExecuteChanged();
                ExecuteSendRegisterCommand.RaiseCanExecuteChanged();
            }
        }

        public string UpdateText
        {
            get { return _updateText; }
            set { SetProperty(ref _updateText, value); }
        }


        public DelegateCommand ExecuteStartDryCommand { get; private set; }
        public DelegateCommand ExecuteStartDecontominationCommand { get; private set; }
        public DelegateCommand ExecuteStopCommand { get; private set; }
        public DelegateCommand ExecuteSendRegisterCommand { get; private set; }

        public DelegateCommand<string> ExecuteGenericDelegateCommand { get; private set; }        

        public DelegateCommand DelegateCommandObservesProperty { get; private set; }

        public DelegateCommand DelegateCommandObservesCanExecute { get; private set; }


        public MainWindowViewModel()
        {
            _decontominators = new List<string>();
            //int value = Int32.Parse(ConfigurationManager.AppSettings["StartingMonthColumn"]);
            string _decs = ConfigurationManager.AppSettings["SlaveIds"];
            _decontominators = _decs.Split(',').ToList();
            
            ExecuteStartDecontominationCommand = new DelegateCommand(ExecuteStartDecontomination, CanExecute);
            ExecuteStartDryCommand = new DelegateCommand(ExecuteStartDry, CanExecute);
            ExecuteStopCommand = new DelegateCommand(ExecuteStop, CanExecute);
            ExecuteSendRegisterCommand = new DelegateCommand(ExecuteSendRegister, CanExecute);
//            ExecuteDelegateCommand = new DelegateCommand(Execute, CanExecute);

            DelegateCommandObservesProperty = new DelegateCommand(Execute, CanExecute).ObservesProperty(() => IsEnabled);

            DelegateCommandObservesCanExecute = new DelegateCommand(Execute).ObservesCanExecute(() => IsEnabled);

            ExecuteGenericDelegateCommand = new DelegateCommand<string>(ExecuteGeneric).ObservesCanExecute(() => IsEnabled);

            AuthTrue = true;
            _isEnabled = AuthTrue;
        }

        private async void ExecuteStartDecontomination() => await _modbusService.StartDecontominationTask();
        private async void ExecuteStartDry() => await _modbusService.StartDryTask();
        private async void ExecuteStop() => await _modbusService.StopTask();
        private async void ExecuteSendRegister()
        {
            _register = 1;
            await _modbusService.SendRegisterTask(_register);
        }

        private void Execute()
        {
            UpdateText = $"Updated: {DateTime.Now}";
        }

        private void ExecuteGeneric(string parameter)
        {
            UpdateText = parameter;
        }

        private bool CanExecute()
        {
            return IsEnabled;
        }
        private bool _isEnabled;
        private string _updateText;
        private ModbusService _modbusService = new ModbusService();
        private ushort _register;
        private List<string> _decontominators;
    }
}
