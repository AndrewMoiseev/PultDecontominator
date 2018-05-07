using System.Threading.Tasks;

namespace PultDecontominator.Services
{
    public class ModbusService : IModbusService
    {

        

        private IModbusService _modbusServiceImplementation;
        public async Task<bool> StartDryTask()
        {
            return await _modbusServiceImplementation.StartDryTask();
        }

        public async Task<bool> StopTask()
        {
            return await _modbusServiceImplementation.StopTask();
        }

        public async Task<bool> SendDataTask()
        {
            return await _modbusServiceImplementation.SendDataTask();
        }

        public Task<ushort> ReadRegisterTask => _modbusServiceImplementation.ReadRegisterTask;

        Task<ushort> IModbusService.ReadRegisterTask => throw new System.NotImplementedException();

        public async Task<bool> SendRegisterTask()
        {
            throw new System.NotImplementedException();
        }

        public async Task<bool> SendRegisterTask(ushort reg)
        {
            throw new System.NotImplementedException();
        }
        public async Task<bool> SendRegisterTask(int addr, ushort reg)
        {
            throw new System.NotImplementedException();
        }

        Task<bool> IModbusService.InitializeComPortTask()
        {
            throw new System.NotImplementedException();
        }

        public Task<bool> StartDecontominationTask()
        {
            throw new System.NotImplementedException();
        }

        Task<bool> IModbusService.StartDryTask()
        {
            throw new System.NotImplementedException();
        }

        Task<bool> IModbusService.StopTask()
        {
            throw new System.NotImplementedException();
        }

        Task<bool> IModbusService.SendDataTask()
        {
            throw new System.NotImplementedException();
        }

        Task<bool> IModbusService.SendRegisterTask()
        {
            throw new System.NotImplementedException();
        }

        Task<bool> IModbusService.SendRegisterTask(ushort reg)
        {
            throw new System.NotImplementedException();
        }

        Task<bool> IModbusService.SendRegisterTask(int addr, ushort reg)
        {
            throw new System.NotImplementedException();
        }
    }
}