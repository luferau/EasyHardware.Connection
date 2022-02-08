using EasyHardware.Connection.Core.Enums;

namespace EasyHardware.Connection.Core.Queue
{
    public class WriteReadQuery : BaseQueryTcs<(ConnectionResultType, byte[])>, IReadQuery
    {
        public byte[] WriteData { get; set; }

        public int ReadTimeout_ms { get; set; }

        public AnswerReceiveConditionType ReceiveCondition { get; set; }

        public string SpecifiedStringPattern { get; set; }

        public byte[] SpecifiedByteDataPattern { get; set; }

        public int SpecifiedDataAmount { get; set; }
    }
}
