namespace EasyHardware.Connection.Core.Enums
{
    /// <summary>
    /// Defines the condition under which the library considers that a response has been received
    /// </summary>
    public enum AnswerReceiveConditionType
    {
        AnyData,                // Arrival of any amount of any kind of data
        NewLine,                // Message ends with a newline character \r (0x0D) or \n (0x0A)
        SpecifiedDataAmount,    // Message received when arrival of a certain number of data bytes
        SpecifiedString,        // Message contains specified string
        SpecifiedByteData,      // Message contains specified byte[] data 
        CorrectCrc              // Message received when calculation of the checksum CRC equals 0
    }
}