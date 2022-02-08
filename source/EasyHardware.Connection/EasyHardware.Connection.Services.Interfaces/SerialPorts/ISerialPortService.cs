using System.Collections.Generic;

namespace EasyHardware.Connection.Services.Interfaces.SerialPorts
{
    public interface ISerialPortService
    {
        List<string> GetAvailableSerialPorts();

        List<ISerialPortInfo> GetSerialPortsInformation();
    }
}