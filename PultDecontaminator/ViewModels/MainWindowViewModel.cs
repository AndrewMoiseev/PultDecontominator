using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Configuration;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Threading;
using System.Web.UI;
using System.Windows;
using System.Windows.Threading;
using Prism.Commands;
using Prism.Mvvm;
using PultDecontominator.Models;
using PultDecontominator.Services;

namespace PultDecontominator.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        public bool AuthTrue { get; set; }
        public DispatcherTimer timer;
        public List<string> Decontaminators
        {
            get { return _decontominators; }
            set { SetProperty(ref _decontominators, value); }
        }
//        public ObservableCollection<DecontaminatorRegister> Registers
        public List<DecontaminatorRegister> Registers
        {
            get { return _registers; }
            set { SetProperty(ref _registers, value); }
        }

        public bool IsEnabledOpenComPort
        {
            get { return _isEnabledOpenComPort; }
            set
            {
                SetProperty(ref _isEnabledOpenComPort, value);
                ExecuteOpenComPortCommand.RaiseCanExecuteChanged();
                ExecuteStartDecontominationCommand.RaiseCanExecuteChanged();
                ExecuteStopCommand.RaiseCanExecuteChanged();
                ExecuteSendRegisterCommand.RaiseCanExecuteChanged();
            }
        }

        public bool IsPolling
        {
            get { return _isPolling; }
            set { SetProperty(ref _isPolling, value); }
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
                ExecuteCloseComPortCommand.RaiseCanExecuteChanged();
            }
        }

        public SerialPort Port { get => _port; set => _port = value; }

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

        public MainWindowViewModel()
        {

            //_decontominators = new List<string>();
            Decontaminators = new List<string>();
            //_registers = new ObservableCollection<DecontaminatorRegister>(ReadCSV("1.csv"));
            Port = new SerialPort("COM1");
            // configure serial port
            Port.BaudRate = Int32.Parse(ConfigurationManager.AppSettings["BaudRate"]); 
            Port.DataBits = Int32.Parse(ConfigurationManager.AppSettings["DataBits"]); 
            Port.Parity = Parity.None;
            Port.StopBits = StopBits.One;

            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(Int32.Parse(ConfigurationManager.AppSettings["TimerInterval"]));
            timer.Tick += timer_Tick;
            timer.Start();
            // _registers = new List<DecontaminatorRegister>(ReadCSV("1.csv"));
            try
            {
                Registers = new List<DecontaminatorRegister>(ReadCSV("1.csv"));

            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Не считался CSV файл");
                throw;
            }
            //int value = Int32.Parse(ConfigurationManager.AppSettings["StartingMonthColumn"]);

            var _decs = ConfigurationManager.AppSettings["SlaveIds"];
            Decontaminators = _decs.Split(',').ToList();

            ExecuteStartDecontominationCommand = new DelegateCommand(ExecuteStartDecontomination, CanExecute);
            ExecuteStartDryCommand = new DelegateCommand(ExecuteStartDry, CanExecute);
            ExecuteStopCommand = new DelegateCommand(ExecuteStop, CanExecute);
            ExecuteSendRegisterCommand = new DelegateCommand(ExecuteSendRegister, CanExecute);
            ExecuteOpenComPortCommand = new DelegateCommand(ExecuteOpenComPort, CanExecuteComOpen);
            ExecuteCloseComPortCommand = new DelegateCommand(ExecuteCloseComPort, CanExecute);

            AuthTrue = true;
           // _isEnabled = AuthTrue;
            IsEnabled = false;
            IsEnabledOpenComPort = true;
        }
        private void timer_Tick(object sender, EventArgs e)
        {
            //UpdateText = DateTime.Now.ToString("HH:mm:ss");

            if (Port != null && Port.IsOpen)
            {
              if(IsPolling)  UpdateText = DateTime.Now.ToString("HH:mm:ss");
            }
        }
        private void ExecuteCloseComPort()
        {
            try
            {
                if (Port != null && Port.IsOpen) Port.Close();
                IsEnabled = false;
                IsEnabledOpenComPort = true;


            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
                throw;
            }
            
        }

        private void ExecuteOpenComPort()
        {
            try
            {
                if (Port != null && !Port.IsOpen) Port.Open();
                IsEnabled = true;
                IsEnabledOpenComPort = false;

            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
                throw;
            }
        }

        private async void ExecuteStartDecontomination() => await _modbusService.StartDecontominationTask();
        private async void ExecuteStartDry() => await _modbusService.StartDryTask();
        private async void ExecuteStop() => await _modbusService.StopTask();
        private async void ExecuteSendRegister()
        {
            _register = 1;
            _addres = 0;
            await _modbusService.SendRegisterTask(_addres,_register);
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
        private bool CanExecuteComOpen()
        {
          //  IsEnabledOpenComPort = !Port.IsOpen;
            return IsEnabledOpenComPort;
        }

        private SerialPort _port;
        private bool _isEnabled;
        private string _updateText;
        private ModbusService _modbusService = new ModbusService();
        private ushort _register;
        private int _addres;
        private List<string> _decontominators;
        //private ObservableCollection<DecontaminatorRegister> _registers;
        private List<DecontaminatorRegister> _registers = new List<DecontaminatorRegister>();

        private bool _isEnabledOpenComPort;
        private bool _isPolling;

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

                    try
                    {
                        return new DecontaminatorRegister(data[0], data[1], data[2], Convert.ToInt32(data[3]), data[4],
                            data[5]);

                    }
                            catch (Exception e)
                    {
                            MessageBox.Show(e.Message, "Не считался CSV файл");
                            throw;
                    }


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
