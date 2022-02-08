using System.IO.Ports;

namespace EasyHardware.Connection.Desktop.Serial
{
    public class SerialPortParameters
    {
        #region Constructors

        public SerialPortParameters() : this("COM1", 9600, 8, StopBits.One, Parity.None, Handshake.None, 3000)
        {

        }

        public SerialPortParameters(string portName) : this(portName, 9600, 8, StopBits.One, Parity.None, Handshake.None, 3000)
        {

        }

        public SerialPortParameters(string portName, int baudRate)
            : this(portName, baudRate, 8, StopBits.One, Parity.None, Handshake.None, 3000)
        {

        }

        public SerialPortParameters(string portName, int baudRate, int readTimeout_ms)
            : this(portName, baudRate, 8, StopBits.One, Parity.None, Handshake.None, readTimeout_ms)
        {

        }

        public SerialPortParameters(string portName, int baudRate, int dataBits, StopBits stopBits, Parity parity, Handshake handShake, int readTimeout_ms)
        {
            this.PortName = portName;
            this.BaudRate = baudRate;
            this.DataBits = dataBits;
            this.StopBits = stopBits;
            this.Parity = parity;
            this.HandShake = handShake;
            this.ReadTimeout_ms = readTimeout_ms;
        }

        #endregion

        #region Properies

        public string PortName { get; set; }

        public int BaudRate { get; set; }

        public Parity Parity { get; set; }

        public int DataBits { get; set; }

        public StopBits StopBits { get; set; }

        public Handshake HandShake { get; set; }

        public int ReadTimeout_ms { get; set; }

        #endregion
    }
}
