namespace EasyHardware.Connection.Core.Queue
{
    public class WriteQuery : BaseQueryTcs<bool>
    {
        public byte[] WriteData { get; set; }
    }
}
