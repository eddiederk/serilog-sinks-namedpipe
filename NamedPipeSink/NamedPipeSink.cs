using System;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Configuration;
using System.IO.Pipes;
using System.IO;
using System.Text;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using System.Threading;
using Serilog.Formatting;
using Serilog.Sinks.SystemConsole.Themes;
using Serilog.Formatting.Json;

namespace Serilog.Sinks.NamedPipe
{
    public class NamedPipeSink : ILogEventSink
    {
        private readonly BlockingCollection<LogEvent> m_Queue = new BlockingCollection<LogEvent>();
        private bool ReadyToEmit = false;
        private Thread workerThread;

        readonly Encoding _encoding;
        readonly string _pipeName;
        readonly JsonFormatter formatter;

        internal NamedPipeSink(string pipeName, Encoding encoding = null)
        {
            if (string.IsNullOrWhiteSpace(pipeName)) throw new ArgumentNullException(nameof(pipeName));
            _encoding = encoding ?? new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);
            _pipeName = @"serilog\"+pipeName;
            formatter = new JsonFormatter();

            workerThread = new Thread(new ThreadStart(ConnectionLifetime));
            workerThread.Start();
        }

        public void Emit(LogEvent logEvent)
        {
            if (ReadyToEmit) m_Queue.Add(logEvent);
        }

        private void ConnectionLifetime()
        {
            while (true)
            {
                ReadyToEmit = false;
                using (NamedPipeServerStream pipe = new NamedPipeServerStream(_pipeName))
                {
                    pipe.WaitForConnection();

                    ReadyToEmit = true;
                    try
                    {
                        using (var _pipeWriter = new StreamWriter(pipe, _encoding) { AutoFlush = true })
                        {
                            while (true)
                            {
                                while (!m_Queue.IsCompleted)
                                {
                                    var i = m_Queue.Take();
                                    var json = SimplifiedLogEvent.Serialize(i, i.RenderMessage());
                                    _pipeWriter.WriteLine(json);
                                }

                                Thread.Sleep(10);
                            }
                        }
                    }

                    // Catch the IOException that is raised if the pipe is broken or disconnected.
                    catch (IOException){ ReadyToEmit = false; }
                    catch (Exception) { }
                }
            }
        }
    }
}
