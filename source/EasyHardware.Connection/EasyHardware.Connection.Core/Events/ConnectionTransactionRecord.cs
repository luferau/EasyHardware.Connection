using System;
using EasyHardware.Connection.Core.Enums;

namespace EasyHardware.Connection.Core.Events
{
    public class ConnectionTransactionRecord
    {
        public ConnectionTransactionRecord()
        {
            DateTime = DateTime.Now;
        }

        public DateTime DateTime { get; set; }

        public DataDirectionType DataDirection { get; set; }

        public string ConnectionName { get; set; }

        public byte[] Data { get; set; }
    }
}
