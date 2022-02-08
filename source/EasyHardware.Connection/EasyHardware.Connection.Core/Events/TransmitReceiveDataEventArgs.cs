using System;
using System.Text;
using EasyHardware.Connection.Core.Enums;

namespace EasyHardware.Connection.Core.Events
{
    public class TransmitReceiveDataEventArgs : EventArgs
    {
        public ConnectionTransactionRecord Record { get; set; }

        public TransmitReceiveDataEventArgs(byte[] data)
        {
            Init(DataDirectionType.Transmit, string.Empty, data);
        }

        public TransmitReceiveDataEventArgs(byte[] data, int index, int count)
        {
            var dataShort = new byte[count];
            var j = 0;
            for (var i = index; i < count; i++)
            {
                dataShort[j++] = data[i];
            }
            Init(DataDirectionType.Transmit, string.Empty, dataShort);
        }

        public TransmitReceiveDataEventArgs(DataDirectionType dataDirection, byte[] data)
        {
            Init(dataDirection, string.Empty, data);
        }

        public TransmitReceiveDataEventArgs(DataDirectionType dataDirection, string name, byte[] data)
        {
            Init(dataDirection, name, data);
        }

        public TransmitReceiveDataEventArgs(string text)
        {
            Init(DataDirectionType.Transmit, string.Empty, StringToBytes(text));
        }

        public TransmitReceiveDataEventArgs(DataDirectionType dataDirection, string text)
        {
            Init(dataDirection, string.Empty, StringToBytes(text));
        }

        private void Init(DataDirectionType dataDirection, string name, byte[] data)
        {
            Record = new ConnectionTransactionRecord
            {
                DataDirection = dataDirection,
                ConnectionName = name,
                Data = data
            };
        }

        private byte[] StringToBytes(string text) => Encoding.ASCII.GetBytes(text);
    }
}
