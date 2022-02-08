using System;

namespace EasyHardware.Connection.Services.Interfaces.SerialPorts
{
    public interface ISerialPortInfo
    {
        int Availability { get; set; }
        string Caption { get; set; }
        string ClassGuid { get; set; }
        string[] CompatibleID { get; set; }
        int ConfigManagerErrorCode { get; set; }
        bool ConfigManagerUserConfig { get; set; }
        string CreationClassName { get; set; }
        string Description { get; set; }
        string DeviceID { get; set; }
        bool ErrorCleared { get; set; }
        string ErrorDescription { get; set; }
        string[] HardwareID { get; set; }
        DateTime InstallDate { get; set; }
        int LastErrorCode { get; set; }
        string Manufacturer { get; set; }
        string Name { get; set; }
        string PNPClass { get; set; }
        string PNPDeviceID { get; set; }
        int[] PowerManagementCapabilities { get; set; }
        bool PowerManagementSupported { get; set; }
        bool Present { get; set; }
        string Service { get; set; }
        string Status { get; set; }
        int StatusInfo { get; set; }
        string SystemCreationClassName { get; set; }
        string SystemName { get; set; }
    }
}
