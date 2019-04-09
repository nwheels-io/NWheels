using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NWheels.Build;
using NWheels.Composition.Model.Impl;

namespace NWheels.Cli
{
    class Program
    {
        static int Main(string[] args)
        {
            Console.WriteLine("NWheels build tool");
            Console.WriteLine($"Building project: {args[0]}");

            int exitCode;

            Console.WriteLine($"------ BUILD STARTING ------");

            try
            {
                var options = new BuildOptions(args[0]);
                var engine = new BuildEngine(options);
                var success = engine.Build(out var output); 

                exitCode = success ? 0 : 1;

                if (success)
                {
                    PrintSlocStats(output.GetSlocPerFileType());
                }
            }
            catch (BuildErrorException e)
            {
                exitCode = 1;
                Console.WriteLine(e.Message);
            }
            catch (Exception e)
            {
                exitCode = 100;
                Console.WriteLine(e.ToString());
            }

            Console.WriteLine($"------ BUILD {(exitCode == 0 ? "SUCCESS" : "FAILURE")} ------");

            return exitCode;
        }

        private static void PrintSlocStats(IReadOnlyDictionary<string, int> slocPerFileType)
        {
            Console.WriteLine($"------ generated code summary ------");

            var saveColor = Console.ForegroundColor;
            var typeColWidth = slocPerFileType.Keys.Max(k => k.Length);
            var slocColWidth = slocPerFileType.Values.Max(v => v.ToString("#,###").Length);
            var totalWidth = typeColWidth + slocColWidth + 3;
            
            foreach (var key in slocPerFileType.Keys.OrderBy(k => k))
            {
                var valueString = slocPerFileType[key].ToString("#,###");
                var paddingWidth = totalWidth - key.Length - valueString.Length - 2; 
                
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.Write($"{key} ");
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.Write(string.Empty.PadRight(paddingWidth, '.'));
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($" {slocPerFileType[key]:#,###}");
            }
            
            Console.ForegroundColor = saveColor;
        }
    }
}