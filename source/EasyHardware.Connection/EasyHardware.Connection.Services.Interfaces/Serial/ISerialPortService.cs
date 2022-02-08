using System.Collections.Generic;

namespace EasyHardware.Connection.Services.Interfaces.Serial
{
    public interface ISerialPortService
    {
        List<string> GetAvailableSerialPorts();

        List<ISerialPortInfo> GetSerialPortsInformation();
    }
}