using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace NWheels.Testability
{
    [Serializable]
    public class MicroserviceProcessException : Exception
    {
        public MicroserviceProcessException(MicroserviceProcess process) 
            : base(FormatMessage(process))
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected MicroserviceProcessException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static void ThrowIfFailed(MicroserviceProcess process)
        {
            if (process.Errors.Count > 0 || process.ExitCode != 0)
            {
                throw new MicroserviceProcessException(process);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static string FormatMessage(MicroserviceProcess process)
        {
            StringBuilder message = new StringBuilder("Microservice process failure!");

            message.AppendLine($" Exit code: {(process.ExitCode.HasValue ? process.ExitCode.ToString() : "N/A")}");
            message.AppendLine($"Executable: {process.GetExecutableFileName()}");
            message.AppendLine($"Arguments: {process.GetExecutableArguments()}");

            foreach (var error in process.Errors)
            {
                message.AppendLine("------ Exception ------");
                message.AppendLine(error.ToString());
            }

            message.AppendLine("------ Output ------");
            process.CopyOutput(message);
            message.AppendLine("--- End of Output ---");

            return message.ToString();
        }
    }
}
