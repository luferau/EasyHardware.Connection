using System;
using System.IO.Ports;
using EasyHardware.Connection.Core;
using EasyHardware.Connection.Core.Enums;
using EasyHardware.Connection.Core.Events;

namespace EasyHardware.Connection.Desktop.Serial
{
    /// <summary>
    /// Represents a Serial port connection (include Virtual Serial ports)
    /// </summary>
    public class SerialPortConnection : ConnectionBase
    {
        private readonly SerialPort _serialPort;

        private readonly SerialPortParameters _parameters;

        public SerialPortConnection(SerialPortParameters parameters, AnswerReceiveConditionType receiveCondition = AnswerReceiveConditionType.AnyData)
        {
            this._parameters = parameters;

            this._serialPort = new SerialPort
            {
                PortName = this._parameters.PortName,
                BaudRate = this._parameters.BaudRate,
                Parity = this._parameters.Parity,
                DataBits = this._parameters.DataBits,
                StopBits = this._parameters.StopBits
            };

            this._serialPort.DataReceived += this.SerialPort_OnDataReceived;
            this._serialPort.ErrorReceived += this.SerialPort_OnErrorReceived;

            this.ReadTimeout_ms = this._parameters.ReadTimeout_ms;
            this.ReceiveCondition = receiveCondition;
            this.SpecifiedStringPattern = "";
        }

        #region IConnection properties

        public override bool IsOpen
        {
            get
            {
                try
                {
                    return this._serialPort.IsOpen;
                }
                catch (Exception exp)
                {
                    this.ExceptionHandler($"Serial port {this._serialPort.PortName} call IsOpen error.\n{exp.Message}", "Exception");
                    return false;
                }
            }
        }

        public override string FullName => $"{_serialPort.PortName}:{_serialPort.BaudRate},{_serialPort.DataBits}N{_serialPort.StopBits}";

        public override string ShortName => $"{_serialPort.PortName}";

        #endregion

        #region IConnection methods

        public override bool Open()
        {
            try
            {
                this._serialPort.Open();
                this.OnStatusChanged(new ConnectionStatusEventArgs(
                    ConnectionStateType.Opened,
                    $"Serial port {this._serialPort.PortName} successfully opened on baud rate: {this._serialPort.BaudRate}"));

                return true;
            }
            catch (Exception exp)
            {
                ExceptionHandler($"Serial port {this._serialPort.PortName} open error.\n{exp.Message}", "Exception");
                return false;
            }
        }

        public override void Close()
        {
            if (!this.IsOpen)
            {
                return;
            }

            try
            {
                this._serialPort.Close();
                this.OnStatusChanged(new ConnectionStatusEventArgs(
                    ConnectionStateType.Closed,
                    $"Serial port {this._serialPort.PortName} successfully closed."));
            }
            catch (Exception exp)
            {
                this.ExceptionHandler($"Serial port {this._serialPort.PortName} close error.\n{exp.Message}", "Exception");
            }
        }

        #endregion

        protected override void WriteInternal(byte[] data)
        {
            try
            {
                this._serialPort.Write(data, 0, data.Length);

                this.OnDataTransmit(new TransmitReceiveDataEventArgs(data));
            }
            catch (Exception exp)
            {
                ExceptionHandler($"Serial port {this._serialPort.PortName} write error.\n{exp.Message}", "Exception");
            }
        }

        protected override byte[] ReadInternal()
        {
            try
            {
                if (this._serialPort.BytesToRead > 0)
                {
                    var buffer = new byte[this._serialPort.BytesToRead];
                    var bytesRead = this._serialPort.Read(buffer, 0, buffer.Length);
                    if (bytesRead > 0)
                    {
                        return buffer;
                    }
                }
            }
            catch (Exception exp)
            {
                this.ExceptionHandler($"Serial port {this._serialPort.PortName} read error.\n{exp.Message}", "Exception");
            }

            return Array.Empty<byte>();
        }

        protected override void BeforeWriteRead(bool discardBuffers = true)
        {
            try
            {
                this._serialPort.DataReceived -= this.SerialPort_OnDataReceived;

                if (discardBuffers)
                {
                    this._serialPort.DiscardInBuffer();
                    this._serialPort.DiscardOutBuffer();
                }
            }
            catch (Exception exp)
            {
                this.ExceptionHandler($"Serial port {this._serialPort.PortName} call discard in/out buffers error.\n{exp.Message}", "Exception");
            }
        }

        protected override void AfterWriteRead()
        {
            this._serialPort.DataReceived += SerialPort_OnDataReceived;
        }

        private void SerialPort_OnDataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            var buffer = this.ReadInternal();

            this.OnDataReceived(new TransmitReceiveDataEventArgs(DataDirectionType.Receive, buffer));
        }

        private void SerialPort_OnErrorReceived(object sender, SerialErrorReceivedEventArgs e)
        {

        }
    }
}
