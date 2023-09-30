using EasyHardware.Connection.Core.Enums;
using EasyHardware.Connection.Core.Events;
using System;
using System.Text;
using System.Threading.Tasks;
using EasyHardware.Connection.Desktop.Visa;

namespace EasyHardware.Examples
{
    internal class Program
    {
        private static string[] ScpiCommands = {
            ":SOUR1:APPL:SQU 1000,1,0,0\n", // Set the waveform of CH1 to square, the frequency to 1kHz, the amplitude to 1Vpp, the offset to 0VDC, and the start phase to 0°
            ":SOUR1:BURS ON\n",             // Turn on the burst function of CH1
            ":SOUR1:BURS:MODE TRIG\n",      // Set the burst type of CH1 to N cycle = TRIGgered
            ":SOUR1:BURS:NCYC 1\n",         // Set the number of cycles of the N cycle burst of CH1 to 1
            ":SOUR1:BURS:TRIG:SOUR INT\n",  // Set the trigger source of the burst mode of CH1 to internal
            ":OUTP1 ON\n",                  // Turn on the output of CH1
            ":SOUR1:BURS:TRIG\n"            // Trigger a burst output in CH1 immediately
        };

        static async Task Main(string[] args)
        {
            var visa = new VisaConnection("TCPIP0::172.16.252.67::inst0::INSTR", 1000, AnswerReceiveConditionType.NewLine);
            visa.StatusChanged += VisaOnStatusChanged;
            visa.DataTransmit += VisaOnDataTransmit;
            visa.CommunicationEventOccurred += VisaOnCommunicationEventOccurred;
            visa.AnswerReceived += VisaOnAnswerReceived;

            bool openResult = visa.Open();

            if (openResult)
            {
                // Query the ID string of the instrument.
                (ConnectionResultType queryResult, string idnAnswer) = await visa.WriteReadAsync("*IDN?\n");

                // Check ID string
                if (queryResult == ConnectionResultType.Ok && idnAnswer.Contains("DG1022Z"))
                {
                    foreach (var scpiCommand in ScpiCommands)
                    {
                        await visa.WriteAsync(scpiCommand);
                        await Task.Delay(50);
                    }
                }

                visa.Close();
            }

            Console.WriteLine("Press any key to exit..");
            Console.ReadKey();
        }

        private static void VisaOnStatusChanged(object sender, ConnectionStatusEventArgs e)
        {
            Console.WriteLine($"VISA Status Changed. State:{e.State} Message:{e.StatusMessage}");
        }

        private static void VisaOnDataTransmit(object sender, TransmitReceiveDataEventArgs e)
        {
            Console.WriteLine($"VISA Data Transmit. Direction:{e.Record.DataDirection} Message:{Encoding.ASCII.GetString(e.Record.Data)}");
        }

        private static void VisaOnAnswerReceived(object sender, TransmitReceiveDataEventArgs e)
        {
            Console.WriteLine($"VISA Data Answer Received. Direction:{e.Record.DataDirection} Message:{Encoding.ASCII.GetString(e.Record.Data)}");
        }

        private static void VisaOnCommunicationEventOccurred(object sender, CommunicationEventArgs e)
        {
            Console.WriteLine($"VISA Communication Event Occurred. Event:{e.EventType}");
        }
    }
}