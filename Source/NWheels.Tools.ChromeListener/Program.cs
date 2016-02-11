using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace NWheels.Tools.ChromeListener
{
    class Program
    {
        private static Stream _s_stdin;
        private static Stream _s_stdout;
        private static string _s_tempDirectoryPath;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        static void Main(string[] args)
        {
            _s_tempDirectoryPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "MessagesTemp");
            _s_stdin = Console.OpenStandardInput();
            _s_stdout = Console.OpenStandardOutput();

            while (true)
            {
                string messageFilePath;
                ReceiveMessage(out messageFilePath);
                var process = Process.Start("notepad.exe", messageFilePath);
                
                WriteResponseMessage(new {
                    Success = "true",
                    File = messageFilePath,
                    Pid = process.Id
                });
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static void ReceiveMessage(out string messageFilePath)
        {
            var lengthBytes = new byte[4];

            _s_stdin.Read(lengthBytes, 0, lengthBytes.Length);

            var length = BitConverter.ToInt32(lengthBytes, 0);
            var messageBytes = new byte[length];

            _s_stdin.Read(messageBytes, 0, messageBytes.Length);

            var message = Encoding.UTF8.GetString(messageBytes);
            messageFilePath = Path.Combine(_s_tempDirectoryPath, DateTime.Now.ToString("yyyyMMddHHmmssfff") + "-" + Guid.NewGuid().ToString("N") + ".txt");

            File.WriteAllText(messageFilePath, message);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static void WriteResponseMessage(object response)
        {
            var message = JsonConvert.SerializeObject(response);
            var messageBytes = Encoding.UTF8.GetBytes(message);
            var lengthBytes = BitConverter.GetBytes(messageBytes.Length);

            _s_stdout.Write(lengthBytes, 0, lengthBytes.Length);
            _s_stdout.Write(messageBytes, 0, messageBytes.Length);
        }
    }
}
