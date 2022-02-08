using System;
using System.Management;
using EasyHardware.Connection.Services.Interfaces.Serial;

namespace EasyHardware.Connection.Services.Windows.Serial
{
    public class SerialPortInfo : ISerialPortInfo
    {
        public SerialPortInfo(ManagementBaseObject property)
        {
            this.Availability = property.GetPropertyValue("Availability") as int? ?? 0;
            this.Caption = property.GetPropertyValue("Caption") as string ?? string.Empty;
            this.ClassGuid = property.GetPropertyValue("ClassGuid") as string ?? string.Empty;
            this.CompatibleID = property.GetPropertyValue("CompatibleID") as string[] ?? new string[] { };
            this.ConfigManagerErrorCode = property.GetPropertyValue("ConfigManagerErrorCode") as int? ?? 0;
            this.ConfigManagerUserConfig = property.GetPropertyValue("ConfigManagerUserConfig") as bool? ?? false;
            this.CreationClassName = property.GetPropertyValue("CreationClassName") as string ?? string.Empty;
            this.Description = property.GetPropertyValue("Description") as string ?? string.Empty;
            this.DeviceID = property.GetPropertyValue("DeviceID") as string ?? string.Empty;
            this.ErrorCleared = property.GetPropertyValue("ErrorCleared") as bool? ?? false;
            this.ErrorDescription = property.GetPropertyValue("ErrorDescription") as string ?? string.Empty;
            this.HardwareID = property.GetPropertyValue("HardwareID") as string[] ?? new string[] { };
            this.InstallDate = property.GetPropertyValue("InstallDate") as DateTime? ?? DateTime.MinValue;
            this.LastErrorCode = property.GetPropertyValue("LastErrorCode") as int? ?? 0;
            this.Manufacturer = property.GetPropertyValue("Manufacturer") as string ?? string.Empty;
            this.Name = property.GetPropertyValue("Name") as string ?? string.Empty;
            this.PNPClass = property.GetPropertyValue("PNPClass") as string ?? string.Empty;
            this.PNPDeviceID = property.GetPropertyValue("PNPDeviceID") as string ?? string.Empty;
            this.PowerManagementCapabilities = property.GetPropertyValue("PowerManagementCapabilities") as int[] ?? new int[] { };
            this.PowerManagementSupported = property.GetPropertyValue("PowerManagementSupported") as bool? ?? false;
            this.Present = property.GetPropertyValue("Present") as bool? ?? false;
            this.Service = property.GetPropertyValue("Service") as string ?? string.Empty;
            this.Status = property.GetPropertyValue("Status") as string ?? string.Empty;
            this.StatusInfo = property.GetPropertyValue("StatusInfo") as int? ?? 0;
            this.SystemCreationClassName = property.GetPropertyValue("SystemCreationClassName") as string ?? string.Empty;
            this.SystemName = property.GetPropertyValue("SystemName") as string ?? string.Empty;
        }

        public int Availability { get; set; }
        public string Caption { get; set; }
        public string ClassGuid { get; set; }
        public string[] CompatibleID { get; set; }
        public int ConfigManagerErrorCode { get; set; }
        public bool ConfigManagerUserConfig { get; set; }
        public string CreationClassName { get; set; }
        public string Description { get; set; }
        public string DeviceID { get; set; }
        public bool ErrorCleared { get; set; }
        public string ErrorDescription { get; set; }
        public string[] HardwareID { get; set; }
        public DateTime InstallDate { get; set; }
        public int LastErrorCode { get; set; }
        public string Manufacturer { get; set; }
        public string Name { get; set; }
        public string PNPClass { get; set; }
        public string PNPDeviceID { get; set; }
        public int[] PowerManagementCapabilities { get; set; }
        public bool PowerManagementSupported { get; set; }
        public bool Present { get; set; }
        public string Service { get; set; }
        public string Status { get; set; }
        public int StatusInfo { get; set; }
        public string SystemCreationClassName { get; set; }
        public string SystemName { get; set; }
    }
}
