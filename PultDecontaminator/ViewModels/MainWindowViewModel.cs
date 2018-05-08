using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web.UI;
using Prism.Commands;
using Prism.Mvvm;
using PultDecontominator.Models;
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

//        public ObservableCollection<DecontaminatorRegister> Registers
        public List<DecontaminatorRegister> Registers
        {
            get { return _registers; }
            set { SetProperty(ref _registers, value); }
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
        public DelegateCommand ExecuteOpenComPortCommand { get; private set; }
        public DelegateCommand ExecuteCloseComPortCommand { get; private set; }
        public DelegateCommand<string> ExecuteGenericDelegateCommand { get; private set; }        

        public DelegateCommand DelegateCommandObservesProperty { get; private set; }

        public DelegateCommand DelegateCommandObservesCanExecute { get; private set; }


        public MainWindowViewModel()
        {
            _decontominators = new List<string>();

            //_registers = new ObservableCollection<DecontaminatorRegister>(ReadCSV("1.csv"));
            //_registers = new List<DecontaminatorRegister>(ReadCSV("1.csv"));

            //int value = Int32.Parse(ConfigurationManager.AppSettings["StartingMonthColumn"]);
            string _decs = ConfigurationManager.AppSettings["SlaveIds"];
            _decontominators = _decs.Split(',').ToList();
            



            ExecuteStartDecontominationCommand = new DelegateCommand(ExecuteStartDecontomination, CanExecute);
            ExecuteStartDryCommand = new DelegateCommand(ExecuteStartDry, CanExecute);
            ExecuteStopCommand = new DelegateCommand(ExecuteStop, CanExecute);
            ExecuteSendRegisterCommand = new DelegateCommand(ExecuteSendRegister, CanExecute);

            ExecuteOpenComPortCommand = new DelegateCommand(ExecuteOpenComPort, CanExecute);
            ExecuteCloseComPortCommand = new DelegateCommand(ExecuteCloseComPort, CanExecute);
//            ExecuteDelegateCommand = new DelegateCommand(Execute, CanExecute);

            DelegateCommandObservesProperty = new DelegateCommand(Execute, CanExecute).ObservesProperty(() => IsEnabled);

            DelegateCommandObservesCanExecute = new DelegateCommand(Execute).ObservesCanExecute(() => IsEnabled);

            ExecuteGenericDelegateCommand = new DelegateCommand<string>(ExecuteGeneric).ObservesCanExecute(() => IsEnabled);

            AuthTrue = true;
            _isEnabled = AuthTrue;
        }

        private void ExecuteCloseComPort()
        {
            throw new NotImplementedException();
        }

        private void ExecuteOpenComPort()
        {
            throw new NotImplementedException();
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
        //private ObservableCollection<DecontaminatorRegister> _registers;
        private List<DecontaminatorRegister> _registers = new List<DecontaminatorRegister>();
        //public ObservableCollection<T> Convert<T>(IEnumerable<T> original)
        //{
        //    return new ObservableCollection<T>(original);
        //}
        public IEnumerable<DecontaminatorRegister> ReadCSV(string fileName)
        {
            // We change file extension here to make sure it's a .csv file.
            // TODO: Error checking.
            string[] lines = File.ReadAllLines(System.IO.Path.ChangeExtension(fileName, ".csv"));

            return lines?.Select(line =>
            {
                string[] data = line.Split(';');
                //Name = name;
                //Panel = panel;
                //TypeData = typeData;
                //AddresRegister = addresRegister;
                //Description = description;
                //DescriptionTypeData = descriptionTypeData;
                return new DecontaminatorRegister(data[0], data[1], data[2], Convert.ToInt32(data[3]), data[4],
                    data[5]);
            });
        }
    }
    internal class ItemRegsViewModel : BindableBase
    {
        public ItemRegsViewModel(DecontaminatorRegister item)
        {
            _item = item;
            
        }

        public string Name => _item.Name;
        public string Description => _item.Description;
        public int AddresRegister => _item.AddresRegister;


        private  DecontaminatorRegister _item;
    }
}
