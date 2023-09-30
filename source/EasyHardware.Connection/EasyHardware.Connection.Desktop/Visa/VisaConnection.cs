using EasyHardware.Connection.Core.Events;
using System;
using EasyHardware.Connection.Core;
using EasyHardware.Connection.Core.Enums;
using NationalInstruments.Visa;

namespace EasyHardware.Connection.Desktop.Visa
{
    /// <summary>
    /// Represents a VISA connection
    /// </summary>
    public class VisaConnection : ConnectionBase
    {
        private MessageBasedSession _mbSession;

        public string ResourceName { get; }

        public VisaConnection(string resourceName, int readTimeout_ms = 1000, AnswerReceiveConditionType receiveCondition = AnswerReceiveConditionType.NewLine)
        {
            ResourceName = resourceName;
            ReadTimeout_ms = readTimeout_ms;
            ReceiveCondition = receiveCondition;

            _isOpen = false;
        }

        #region IConnection properties

        private bool _isOpen;
        public override bool IsOpen => _isOpen;

        public override string ShortName => ResourceName;

        #endregion

        #region IConnection methods

        public override bool Open()
        {
            try
            {
                using (var rmSession = new ResourceManager())
                {
                    _mbSession = (MessageBasedSession)rmSession.Open(ResourceName);

                    _isOpen = true;
                }

                OnStatusChanged(
                    new ConnectionStatusEventArgs(
                        ConnectionStateType.Opened,
                        $"VISA connection successfully opened with ResourceName: {ResourceName}"));

                return true;
            }
            catch (Exception exp)
            {
                _isOpen = false;
                ExceptionHandler($"VISA connection open error with ResourceName: {ResourceName}. Message: {exp.Message}", "Exception");

                return false;
            }
        }

        public override void Close()
        {
            if (IsOpen)
            {
                _isOpen = false;
                _mbSession.Dispose();
                OnStatusChanged(
                    new ConnectionStatusEventArgs(
                        ConnectionStateType.Closed, 
                        $"VISA connection successfully closed with ResourceName: {ResourceName}"));
            }
        }

        #endregion

        protected override void WriteInternal(byte[] data)
        {
            try
            {
                _mbSession.RawIO.Write(data);

                OnDataTransmit(new TransmitReceiveDataEventArgs(data));
            }
            catch (Exception exp)
            {
                ExceptionHandler($"VISA connection {ResourceName} write error.\n{exp.Message}", "Exception");
            }
        }

        protected override byte[] ReadInternal()
        {
            try
            {
                return _mbSession.RawIO.Read();
            }
            catch (Exception exp)
            {
                this.ExceptionHandler($"VISA connection {ResourceName} read error.\n{exp.Message}", "Exception");
            }

            return Array.Empty<byte>();
        }
    }
}