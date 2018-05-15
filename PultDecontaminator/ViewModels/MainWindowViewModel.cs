using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Configuration;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Threading;
using System.Web.UI;
using System.Windows;
using System.Windows.Media.Converters;
using System.Windows.Threading;
using Modbus.Device;
using Modbus.Serial;
using Modbus.Utility;
using Prism.Commands;
using Prism.Mvvm;
using PultDecontominator.Models;
using PultDecontominator.Services;
using PultDecontominator.Views;

namespace PultDecontominator.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        public bool AuthTrue { get; set; }
        public bool DoStop { get; set; }
        public bool DoDry { get; set; }
        public bool DoDec { get; set; }
        public bool DoReset { get; set; }

        public DispatcherTimer timer;

        public byte DecontaminatorSlaveId
        {
            get { return _decontominatorSlaveId; }
            set { SetProperty(ref _decontominatorSlaveId, value); }
        }

        public List<string> Decontaminators
        {
            get { return _decontominators; }
            set { SetProperty(ref _decontominators, value); }
        }
        
        public string CurrentReg
        {
            get { return _currentReg; }
            set { SetProperty(ref _currentReg, value); }
        }

        public string CurrentTteni
        {
            get { return _currentTteni; }
            set { SetProperty(ref _currentTteni, value); }
        }

        public string CurrentTerm
        {
            get { return _currentTerm; }
            set { SetProperty(ref _currentTerm, value); }
        }

        public string CurrentHum
        {
            get { return _currentHum; }
            set { SetProperty(ref _currentHum, value); }
        }

        public string CurrentH2O2
        {
            get { return _currentH2O2; }
            set { SetProperty(ref _currentH2O2, value); }
        }
        public string CurrentTpvi
        {
            get { return _currentTpvi; }
            set { SetProperty(ref _currentTpvi, value); }
        }

        public ObservableCollection<DecontaminatorRegister> Registers
        //        public List<DecontaminatorRegister> Registers RegisterValue
        {
            get { return _registers; }
            set
            {
                SetProperty(ref _registers, value);
//                RaisePropertyChanged(nameof(Registers));
//                RaisePropertyChanged("RegisterValue");
            }
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
                ExecuteResetAvarCommand.RaiseCanExecuteChanged();
            }
        }

        public SerialPort Port { get => _port; set => _port = value; }

        public string InfoMessage
        {
            get { return _infoMessage; }
            set { SetProperty(ref _infoMessage, value); }

        }
        public string UpdateText
        {
            get { return _updateText; }
            set { SetProperty(ref _updateText, value); }
        }

        public ModbusSerialMaster Master
        {
            get => _master;
            set => _master = value;
        }

        public readonly SerialPortAdapter Adapter;
        public int LogSize;

        public DelegateCommand ExecuteStartDryCommand { get; private set; }
        public DelegateCommand ExecuteStartDecontominationCommand { get; private set; }
        public DelegateCommand ExecuteStopCommand { get; private set; }
        public DelegateCommand ExecuteSendRegisterCommand { get; private set; }
        public DelegateCommand ExecuteOpenComPortCommand { get; private set; }
        public DelegateCommand ExecuteCloseComPortCommand { get; private set; }
        public DelegateCommand ExecuteResetAvarCommand { get; private set; }

        public MainWindowViewModel()
        {
            Decontaminators = new List<string>();
            var _decs = ConfigurationManager.AppSettings["SlaveIds"];
            Decontaminators = _decs.Split(',').ToList();
            LogSize =  Int32.Parse(ConfigurationManager.AppSettings["LogSize"]);
            //Port = new SerialPort("COM7");
            Port = new SerialPort(ConfigurationManager.AppSettings["SerialPort"]);
            // configure serial port
            Port.BaudRate = Int32.Parse(ConfigurationManager.AppSettings["BaudRate"]); 
            Port.DataBits = Int32.Parse(ConfigurationManager.AppSettings["DataBits"]); 
            Port.Parity = Parity.None;
            Port.StopBits = StopBits.One;

            Port.ReadTimeout = 200;
            Port.WriteTimeout = 200;

            Adapter = new SerialPortAdapter(Port);
            Master = ModbusSerialMaster.CreateRtu(Adapter);

            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(Int32.Parse(ConfigurationManager.AppSettings["TimerInterval"]));
            timer.Tick += timer_Tick;
            timer.Start();
            // _registers = new List<DecontaminatorRegister>(ReadCSV("1.csv"));
            try
            {
//                Registers = new List<DecontaminatorRegister>(ReadCSV("1.csv"));
                Registers = new ObservableCollection<DecontaminatorRegister>(ReadCSV("1.csv"));
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Не считался CSV файл");
                throw;
            }
            Registers = new ObservableCollection<DecontaminatorRegister>(Registers.OrderBy(n => n.AddresRegister));


            ExecuteStartDecontominationCommand = new DelegateCommand(ExecuteStartDecontomination, CanExecute);
            ExecuteStartDryCommand = new DelegateCommand(ExecuteStartDry, CanExecute);
            ExecuteStopCommand = new DelegateCommand(ExecuteStop, CanExecute);
            ExecuteSendRegisterCommand = new DelegateCommand(ExecuteSendRegister, CanExecute);
            ExecuteOpenComPortCommand = new DelegateCommand(ExecuteOpenComPort, CanExecuteComOpen);
            ExecuteCloseComPortCommand = new DelegateCommand(ExecuteCloseComPort, CanExecute);
            ExecuteResetAvarCommand = new DelegateCommand(ExecuteResetAvar, CanExecute);
            AuthTrue = true;
           // _isEnabled = AuthTrue;
            IsEnabled = false;
            IsEnabledOpenComPort = true;

            DoStop = false;
            DoDry = false;
            DoDec = false;
            DoReset = false;

            InfoMessage = "";

            _listAddrLenPairs = new List<KeyValuePair<ushort, ushort>>();
            bool doNext = true;
            ushort addrReg = 1;
            ushort saveAddrReg = 1;
            ushort lenReceive = 0;

            ushort kolvoRegs = 1;

            foreach (var register in Registers)
            {
                doNext = (register.AddresRegister - 1) != addrReg;
                addrReg = register.AddresRegister;
                lenReceive++;

                if (doNext)
                {
                    _listAddrLenPairs.Add(new KeyValuePair<ushort, ushort>(saveAddrReg, lenReceive));
                    saveAddrReg = addrReg;
                    lenReceive = 0;
                    //doNext = false;
                }

                kolvoRegs = addrReg;
            }
            lenReceive++;
            _listAddrLenPairs.Add(new KeyValuePair<ushort, ushort>(saveAddrReg, lenReceive ));

            _valRegs = new ushort[kolvoRegs+1];

        }


        private void timer_Tick(object sender, EventArgs e)
        {
            //UpdateText = DateTime.Now.ToString("HH:mm:ss");
            ushort concentrationH2O2 = 0;
            ushort temperatureAir = 0;
            ushort humidityAir = 0;
            ushort temperatureTen = 0;
            ushort temperaturePv = 0;

            // bool isDo = DoStop || DoDry || DoDec || DoReset;

            if (Port != null && Port.IsOpen )
            {

               // InfoMessage += "Нет связи с МК \r\n";

                if (DoDry)
                {
                    Master.WriteSingleRegister(DecontaminatorSlaveId, 3001, 2);
                    Master.WriteSingleRegisterAsync(DecontaminatorSlaveId, 1000, 1);
                }
                if (DoDec)
                {
                    Master.WriteSingleRegister(DecontaminatorSlaveId, 3001, 1);
                    Master.WriteSingleRegisterAsync(DecontaminatorSlaveId, 1000, 1);
                }
                if (DoStop)
                {
                    //Master.WriteSingleRegister(DecontaminatorSlaveId, 1000, 2);
                    try
                    {
                        //ushort[] data1 = Master.ReadHoldingRegisters(17, 107, 3);
                        //Master.WriteSingleRegister(DecontaminatorSlaveId, 3001, 0);
                        Master.WriteSingleRegisterAsync(DecontaminatorSlaveId, 1000, 2);
                    }
                    catch (Exception exception)
                    {
                        Console.WriteLine(exception);
                        throw;
                    }
                }
                if (DoReset)
                {
                    Master.WriteSingleRegisterAsync(DecontaminatorSlaveId, 1000, 3);
                    InfoMessage = "";
                }

                if (IsPolling)
                {
                    UpdateText = DateTime.Now.ToString("HH:mm:ss");

                    //  ////////                  
                    PopulateAvar();


                    foreach (var addrLenPair in _listAddrLenPairs)
                    {
                        try
                        {
                            ushort[] values = Master.ReadHoldingRegistersAsync(DecontaminatorSlaveId, addrLenPair.Key,addrLenPair.Value).Result;
                        Array.Copy(values, 0, _valRegs, addrLenPair.Key, addrLenPair.Value);
                        }
                        catch (Exception e1)
                        {
                            if (e1.InnerException != null)
                            {
                                MessageBox.Show(e1.InnerException.Message);
                            }
                            else
                            {
                                MessageBox.Show(e1.Message);
                            }
                            return;
                            //throw;
                            //ExecuteCloseComPort();
                        }
                    }

                    foreach (var register in Registers)
                    {

                        // register.RegisterValue = Master.ReadHoldingRegisters(DecontaminatorSlaveId, register.AddresRegister, 1)[0];

                        register.RegisterValue = _valRegs[register.AddresRegister];

                        if (register.AddresRegister == 1)
                        {
                            //Mode(адрес 1) – код режима. Расшифровка кода:
                            //0 – Останов
                            //1 – Деконтаминация
                            //2 – Осушение
                            //3 – Дополнительное осушение
                            //4 – Ручной
                            //5 – ТЕСТ
                            //TODO: switch register.RegisterValue
                            CurrentReg = "Код режима  " + register.RegisterValue.ToString() + "\r\n";
                        }
                        if (register.AddresRegister == 2)
                        {
                            //Stage(адрес 2) – код этапа. Расшифровка кода:
                            //0 – не используется
                            //1 – Пауза
                            //2 – Выход на концентрацию
                            //3 – Обеззараживание
                            //4 – Дезактивация
                            // TODO: switch register.RegisterValue
                            CurrentReg += "Этап  " + register.RegisterValue.ToString() + "\r\n";
                        }

                        if (register.AddresRegister == 300)
                        {
                            concentrationH2O2 = register.RegisterValue;
                        }
                        if (register.AddresRegister == 301)
                        {
                            CurrentH2O2 = ModbusUtility.GetSingle( register.RegisterValue, concentrationH2O2).ToString("0.00") +" %";
                        }

                        if (register.AddresRegister == 302)
                        {
                            temperatureAir = register.RegisterValue;
                        }
                        if (register.AddresRegister == 303)
                        {
                            CurrentTerm = ModbusUtility.GetSingle(register.RegisterValue, temperatureAir).ToString("0.00") + " °C";
                        }

                        if (register.AddresRegister == 304)
                        {
                            humidityAir = register.RegisterValue; 
                        }
                        if (register.AddresRegister == 305)
                        {
                            CurrentHum = ModbusUtility.GetSingle(register.RegisterValue, humidityAir).ToString("0.00") + " %";
                        }

                        if (register.AddresRegister == 308)
                        {
                            temperaturePv = register.RegisterValue;
                        }
                        if (register.AddresRegister == 309)
                        {
                            CurrentTpvi = ModbusUtility.GetSingle(register.RegisterValue, temperaturePv).ToString("0.00") + " °C";
                        }

                        if (register.AddresRegister == 306)
                        {
                            temperatureTen = register.RegisterValue;
                        }
                        if (register.AddresRegister == 307)
                        {
                            CurrentTteni = ModbusUtility.GetSingle(register.RegisterValue, temperatureTen).ToString("0.00") + " °C";
                        }
                    }

                }
                DoStop = false;
                DoDry = false;
                DoDec = false;
                DoReset = false;

 ;
            }


        }

        private static List<KeyValuePair<ushort, ushort>> _listAddrLenPairs;
        private ushort[] _valRegs;

        private void PopulateAvar()
        {
            ushort[] registers = new ushort[] { 1, 2, 3 };
            try
            {
                registers = Master.ReadHoldingRegistersAsync(DecontaminatorSlaveId, 2000, 3).Result;
            }
            catch (Exception e)
            {
                if (e.InnerException != null)
                {
                    MessageBox.Show(e.InnerException.Message);
                }
                else
                {
                    MessageBox.Show(e.Message);
                }
                //throw;
                ExecuteCloseComPort();
            }

            //2000:00	Нет связи с МК
            if ((registers[0] & 0x01) != 0) InfoMessage = DateTime.Now+ "Нет связи с МК \r\n" + InfoMessage;
            //2000:01	Влажность не вышла на заданный режим
            if ((registers[0] & 0x02) != 0) InfoMessage = DateTime.Now + "Влажность не вышла на заданный режим \r\n" + InfoMessage;
            //2000:02	Нет готовности в режиме останов
            if ((registers[0] & 0x04) != 0) InfoMessage = DateTime.Now + "Нет готовности в режиме останов \r\n" + InfoMessage;
            //2000:03	Нет связи с датчиками параметров воздуха в режиме Осушение
            if ((registers[0] & 0x08) != 0) InfoMessage = DateTime.Now + "Нет связи с датчиками параметров воздуха в режиме Осушение \r\n" + InfoMessage;
            //2000:04	Температура испарителя вне заданных пределах
            if ((registers[0] & 0x10) != 0) InfoMessage = DateTime.Now + "Температура испарителя вне заданных пределах \r\n" + InfoMessage;
            //2000:05	Температура воздуха испарителя вне заданных пределах
            if ((registers[0] & 0x20) != 0) InfoMessage = DateTime.Now + "Температура воздуха испарителя вне заданных пределах \r\n" + InfoMessage;
            //2000:06	В режиме деконтаминация отказ М1(обороты = 0)
            if ((registers[0] & 0x40) != 0) InfoMessage = DateTime.Now + "В режиме деконтаминация отказ М1(обороты = 0) \r\n" + InfoMessage;
            //2002:00	КЗ или обрыв аналогового входа
            if ((registers[2] & 0x01) != 0) InfoMessage = DateTime.Now + "КЗ или обрыв аналогового входа \r\n" + InfoMessage;
            //2002:01	КЗ или обрыв аналогового входа
            if ((registers[2] & 0x02) != 0) InfoMessage = DateTime.Now + "КЗ или обрыв аналогового входа \r\n" + InfoMessage;
            //2002:02	КЗ или обрыв аналогового входа
            if ((registers[2] & 0x04) != 0) InfoMessage = DateTime.Now + "КЗ или обрыв аналогового входа \r\n" + InfoMessage;
            //2002:03	КЗ или обрыв аналогового входа
            if ((registers[2] & 0x08) != 0) InfoMessage = DateTime.Now + "КЗ или обрыв аналогового входа \r\n" + InfoMessage;
            //2002:04	КЗ или обрыв аналогового входа
            if ((registers[2] & 0x10) != 0) InfoMessage = DateTime.Now + "КЗ или обрыв аналогового входа \r\n" + InfoMessage;
            //2001:00	Температура испарителя вне заданных пределов
            if ((registers[1] & 0x01) != 0) InfoMessage = DateTime.Now + "Температура испарителя вне заданных пределов \r\n" + InfoMessage;
            //2001:01	Температура воздуха ТЭН вне заданных пределов
            if ((registers[1] & 0x02) != 0) InfoMessage = DateTime.Now + "Температура воздуха ТЭН вне заданных пределов \r\n" + InfoMessage;
            //2001:02	Нет выхода на требуемую концентрацию
            if ((registers[1] & 0x04) != 0) InfoMessage = DateTime.Now + "Нет выхода на требуемую концентрацию \r\n" + InfoMessage;
            //2001:03	Нет предварительного выхода на концентрацию
            if ((registers[1] & 0x08) != 0) InfoMessage = DateTime.Now + "Нет предварительного выхода на концентрацию \r\n" + InfoMessage;
            //2001:04	Обороты вентилятора М1 ниже допустимого диапазона
            if ((registers[1] & 0x10) != 0) InfoMessage = DateTime.Now + "Обороты вентилятора М1 ниже допустимого диапазона \r\n" + InfoMessage;
            //2001:05	Обороты вентилятора М2 ниже допустимого диапазона
            if ((registers[1] & 0x20) != 0) InfoMessage = DateTime.Now + "Обороты вентилятора М2 ниже допустимого диапазона \r\n" + InfoMessage;
            //2001:06	Концентрация не соответсвует диапазону в режиме поддержания концентрации
            if ((registers[1] & 0x40) != 0) InfoMessage = DateTime.Now + "Концентрация не соответсвует диапазону в режиме поддержания концентрации \r\n" + InfoMessage;
            //2001:07	Концевики заслонки не соответствуют управляющему сигналу
            if ((registers[1] & 0x80) != 0) InfoMessage = DateTime.Now + "Концевики заслонки Y1 не соответствуют управляющему сигналу \r\n" + InfoMessage;
            //2001:08	Концевики заслонки не соответствуют управляющему сигналу
            if ((registers[1] & 0x100) != 0) InfoMessage = DateTime.Now + " Концевики заслонки Y2 не соответствуют управляющему сигналу \r\n" + InfoMessage;
            //2001:09	Концентрация перекиси не падает до необходимого уровня в дезактивации
            if ((registers[1] & 0x200) != 0) InfoMessage = DateTime.Now + "Концентрация перекиси не падает до необходимого уровня в дезактивации \r\n" + InfoMessage; 
            //2001:10	Нет связи с датчиками параметров воздуха
            if ((registers[1] & 0x400) != 0) InfoMessage = DateTime.Now + "Нет связи с датчиками параметров воздуха \r\n" + InfoMessage;
            //2001:11	Сработка термостата испарителя
            if ((registers[1] & 0x800) != 0) InfoMessage = DateTime.Now + "Сработка термостата испарителя \r\n" + InfoMessage;
            //2001:12	Превышение количества запусков режима доп.осушение
            if ((registers[1] & 0x1000) != 0) InfoMessage = DateTime.Now + "Превышение количества запусков режима доп.осушение \r\n" + InfoMessage;
            //2001:13	Превышение времени нахождения в режиме доп.осушение
            if ((registers[1] & 0x2000) != 0) InfoMessage = DateTime.Now + "Превышение времени нахождения в режиме доп.осушение \r\n" + InfoMessage;

            if (InfoMessage.Length > 100000)
            {
                File.AppendAllText("avar" + DateTime.Now.ToString("yyyy-dd-M--HH-mm-ss") + ".log", InfoMessage);
                InfoMessage = "Данные записаны в лог.";
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

        private void ExecuteResetAvar() => DoReset = true;
        

        private  void ExecuteStartDecontomination() => DoDec=true;
        private  void ExecuteStartDry() => DoDry=true;
        private  void ExecuteStop() => DoStop = true;
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
        private ObservableCollection<DecontaminatorRegister> _registers = new ObservableCollection<DecontaminatorRegister>();
       // private List<DecontaminatorRegister> _registers = new List<DecontaminatorRegister>();

        private bool _isEnabledOpenComPort;
        private bool _isPolling;
        private byte _decontominatorSlaveId;
        private ModbusSerialMaster _master;
        private string _currentReg;
        private string _currentTerm;
        private string _currentHum;
        private string _currentH2O2;
        private string _infoMessage;
        private string _currentTteni;
        private string _currentTpvi;

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
                        return new DecontaminatorRegister(data[0], data[1], data[2], Convert.ToUInt16(data[3]), data[4],
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
