using Serilog;
using Serilog.Sinks.NamedPipe;
using System;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace PipeListener
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Title = "LoggerClient";
            string namedPipe = null;
            if (args.Length == 0) namedPipe = GetNamedPipe();
            else namedPipe = CleanUpName(args[0]);

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console(outputTemplate: "[{Timestamp:MM/dd HH:mm:ss.fff} {Level:u3}] {SourceContext:l} {Message:lj}{Properties}{NewLine}")
                .CreateLogger()
                .UseNamedPipeClient(namedPipe);

            while (true)
            {
                Thread.Sleep(1);
            }

        }

        private static string GetNamedPipe()
        {
            while (true)
            {
                var openPipes = System.IO.Directory.GetFiles(@"\\.\pipe\").Where(x=>x.Contains("serilog\\")).ToArray();

                for (int k = 0; k < openPipes.Length; k++)
                {
                    Console.WriteLine($"{k})\t{openPipes[k]}");
                }

                if (openPipes.Length == 0) Console.WriteLine("No applications using Serilog.NamedPipes are running.\r\nType the PipeName or Press any key to search again.");
                else if (openPipes.Length == 1) return CleanUpName(openPipes[0]);
                else Console.WriteLine("Enter the pipe you wish to connect to.");

                var response = Console.ReadLine();
                if (!string.IsNullOrWhiteSpace(response))
                {
                    if (int.TryParse(response, out int k))
                    {
                        if (openPipes.Length > k) return CleanUpName(openPipes[k]);
                    }
                    else
                    {
                        return CleanUpName(response);
                    }
                }
            }
        }

        private static string CleanUpName(string pipeName)
        {
            string[] args = new string[] { "pipe", "serilog", "\\" };
            return "serilog\\" + pipeName.Split(args, StringSplitOptions.RemoveEmptyEntries).Last();
        }
    }
}
