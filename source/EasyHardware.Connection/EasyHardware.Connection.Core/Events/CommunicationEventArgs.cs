using System;
using EasyHardware.Connection.Core.Enums;

namespace EasyHardware.Connection.Core.Events
{
    public class CommunicationEventArgs : EventArgs
    {
        public CommunicationEventType EventType { get; set; }

        public CommunicationEventArgs(CommunicationEventType eventType)
        {
            EventType = eventType;
        }
    }
}
