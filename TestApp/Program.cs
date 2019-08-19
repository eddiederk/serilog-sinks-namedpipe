using Serilog;
using Serilog.Sinks.NamedPipe;
using System;
using System.IO;
using System.IO.Pipes;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace PipeListener
{
    class Program
    {
        static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .Enrich.FromLogContext()
                .WriteTo.Console(outputTemplate: "[{Timestamp:MM/dd HH:mm:ss.fff} {Level:u3}] {SourceContext:l} {Message:lj}{Properties}{NewLine}{Exception}")
                .WriteTo.NamedPipe("testing")
                .CreateLogger();


            var log = Log.ForContext<Program>().ForContext("Property1", 1);

            int counter = 0;
            while (true)
            {
                if (counter % 2 == 1) log.Debug("This is a message {k}", counter++);
                else Log.Information("This is message number {k}", counter++);

                Thread.Sleep(1000);
            }

        }
    }
}
