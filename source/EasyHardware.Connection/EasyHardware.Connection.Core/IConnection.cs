using System;
using System.Threading.Tasks;
using EasyHardware.Connection.Core.Enums;
using EasyHardware.Connection.Core.Events;

namespace EasyHardware.Connection.Core
{
    public interface IConnection : IDisposable
    {
        #region Properties

        /// <summary>
        /// Connection state
        /// </summary>
        bool IsOpen { get; }

        /// <summary>
        /// Connection full name
        /// </summary>
        string FullName { get; }

        /// <summary>
        /// Connection short name
        /// </summary>
        string ShortName { get; }

        /// <summary>
        /// Time to wait for a answer before returning from read methods in ms
        /// </summary>
        int ReadTimeout_ms { get; set; }

        /// <summary>
        /// Condition under which the library considers that a answer has been received
        /// </summary>
        AnswerReceiveConditionType ReceiveCondition { get; set; }

        /// <summary>
        /// String value upon receipt of which the library considers that a answer has been received
        /// </summary>
        string SpecifiedStringPattern { get; set; }

        /// <summary>
        ///Bytes sequence upon receipt of which the library considers that a answer has been received
        /// </summary>
        byte[] SpecifiedByteDataPattern { get; set; }

        /// <summary>
        /// Amount of bytes upon receipt of which the library considers that a answer has been received
        /// </summary>
        int SpecifiedDataAmount { get; set; }

        #endregion

        #region  Methods

        bool Open();

        void Close();

        bool OpenIfClosed();

        Task<ConnectionResultType> WriteAsync(string text);

        Task<ConnectionResultType> WriteAsync(byte[] data);

        Task<(ConnectionResultType, string)> WriteReadAsync(string text);

        Task<(ConnectionResultType, byte[])> WriteReadAsync(byte[] data);

        #endregion

        #region Events

        /// <summary>
        /// Received data, but not in WriteRead process
        /// </summary>
        event EventHandler<TransmitReceiveDataEventArgs> DataReceived;

        /// <summary>
        /// Received data (answer), in WriteRead process
        /// </summary>
        event EventHandler<TransmitReceiveDataEventArgs> AnswerReceived;

        event EventHandler<TransmitReceiveDataEventArgs> DataTransmit;

        event EventHandler<ConnectionStatusEventArgs> StatusChanged;

        event EventHandler<CommunicationEventArgs> CommunicationEventOccurred;

        #endregion
    }
}