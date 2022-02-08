using EasyHardware.Connection.Core.Enums;

namespace EasyHardware.Connection.Core.Queue
{
    public interface IReadQuery
    {
        int ReadTimeout_ms { get; set; }

        AnswerReceiveConditionType ReceiveCondition { get; set; }

        string SpecifiedStringPattern { get; set; }

        byte[] SpecifiedByteDataPattern { get; set; }

        int SpecifiedDataAmount { get; set; }
    }
}