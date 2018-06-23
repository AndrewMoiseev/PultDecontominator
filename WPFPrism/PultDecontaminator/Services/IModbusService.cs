using System;
using System.IO.Ports;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Modbus.Data;
using Modbus.Device;
using Modbus.Utility;
using Modbus.Serial;

namespace PultDecontominator.Services
{
    public interface IModbusService
    {
        Task<bool> InitializeComPortTask();
        Task<bool> StartDecontominationTask();
        Task<bool> StartDryTask();
        Task<bool> StopTask();
        Task<bool> SendDataTask();
        Task<UInt16> ReadRegisterTask { get; }
        Task<bool> SendRegisterTask();
        Task<bool> SendRegisterTask(ushort reg);
        Task<bool> SendRegisterTask(int addr, ushort reg);
    }
}