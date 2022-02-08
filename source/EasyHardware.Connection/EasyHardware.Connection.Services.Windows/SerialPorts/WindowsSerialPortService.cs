using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Management;
using EasyHardware.Connection.Services.Interfaces.SerialPorts;

namespace EasyHardware.Connection.Services.Windows.SerialPorts
{
    public class WindowsSerialPortService : ISerialPortService
    {
        public List<string> GetAvailableSerialPorts()
        {
            var currentPorts = SerialPort.GetPortNames().OrderBy(t => t).ToList();
            return currentPorts;
        }

        public List<ISerialPortInfo> GetSerialPortsInformation()
        {
            var ports = new List<ISerialPortInfo>();

            var processClass = new ManagementClass("Win32_PnPEntity");
            var instances = processClass.GetInstances();
            foreach (var baseObject in instances)
            {
                var property = (ManagementObject)baseObject;
                var name = property.GetPropertyValue("Name");
                if (name != null && name.ToString().Contains("COM"))
                {
                    var portInfo = new SerialPortInfo(property);

                    ports.Add(portInfo);
                }
            }
            return ports;
        }
    }
}
