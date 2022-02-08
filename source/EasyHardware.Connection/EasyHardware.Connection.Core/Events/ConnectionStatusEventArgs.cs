using System;
using EasyHardware.Connection.Core.Enums;

namespace EasyHardware.Connection.Core.Events
{
    public class ConnectionStatusEventArgs : EventArgs
    {
        public ConnectionStateType State { get; set; }

        public string StatusMessage { get; set; }

        public ConnectionStatusEventArgs(string statusMessage)
        {
            State = ConnectionStateType.Undefined;
            StatusMessage = statusMessage;
        }

        public ConnectionStatusEventArgs(ConnectionStateType state, string statusMessage)
        {
            State = state;
            StatusMessage = statusMessage;
        }
    }
}
