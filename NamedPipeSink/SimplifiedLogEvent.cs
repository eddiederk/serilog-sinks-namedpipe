using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Serilog.Sinks.NamedPipe
{
    /// <summary>
    /// A wrapper class for <see cref="Events.LogEvent"/> that is sent as a message to SignalR clients.
    /// </summary>
    public class SimplifiedLogEvent
    {
        /// <summary>
        /// Construct a new <see cref="LogEvent"/>.
        /// </summary>
        public SimplifiedLogEvent() { }

        /// <summary>
        /// Construct a new <see cref="LogEvent"/>.
        /// </summary>
        public static string Serialize(Events.LogEvent logEvent, string renderedMessage)
        {
            var e = new SimplifiedLogEvent()
            {
                Timestamp = logEvent.Timestamp,
                Exception = logEvent.Exception,
                MessageTemplate = logEvent.MessageTemplate.Text,
                Level = logEvent.Level,
                RenderedMessage = renderedMessage,
                Properties = new Dictionary<string, object>()
            };

            foreach (var pair in logEvent.Properties)
            {
                e.Properties.Add(pair.Key, SimplifyPropertyFormatter.Simplify(pair.Value));
            }

            return Newtonsoft.Json.JsonConvert.SerializeObject(e);
        }

        /// <summary>
        /// The time at which the event occurred.
        /// </summary>
        public DateTimeOffset Timestamp { get; set; }

        /// <summary>
        /// The template that was used for the log message.
        /// </summary>
        public string MessageTemplate { get; set; }

        /// <summary>
        /// The level of the log.
        /// </summary>
        public LogEventLevel Level { get; set; }

        /// <summary>
        /// A string representation of the exception that was attached to the log (if any).
        /// </summary>
        public Exception Exception { get; set; }

        /// <summary>
        /// The rendered log message.
        /// </summary>
        public string RenderedMessage { get; set; }

        /// <summary>
        /// Properties associated with the event, including those presented in <see cref="Events.LogEvent.MessageTemplate"/>.
        /// </summary>
        public IDictionary<string, object> Properties { get; set; }
    }


    public static class SimplifyPropertyFormatter
    {
        static readonly HashSet<Type> SpecialScalars = new HashSet<Type>
        {
            typeof(bool),
            typeof(byte), typeof(short), typeof(ushort), typeof(int), typeof(uint),
                typeof(long), typeof(ulong), typeof(float), typeof(double), typeof(decimal),
            typeof(byte[])
        };

        /// <summary>
        /// Simplify the object so as to make handling the serialized
        /// representation easier.
        /// </summary>
        /// <param name="value">The value to simplify (possibly null).</param>
        /// <returns>A simplified representation.</returns>
        public static object Simplify(LogEventPropertyValue value)
        {
            var scalar = value as ScalarValue;
            if (scalar != null)
                return SimplifyScalar(scalar.Value);

            var dict = value as DictionaryValue;
            if (dict != null)
                return dict
                    .Elements
                    .ToDictionary(kv => SimplifyScalar(kv.Key), kv => Simplify(kv.Value));

            var seq = value as SequenceValue;
            if (seq != null)
                return seq.Elements.Select(Simplify).ToArray();

            var str = value as StructureValue;
            if (str != null)
            {
                var props = str.Properties.ToDictionary(p => p.Name, p => Simplify(p.Value));
                if (str.TypeTag != null)
                    props["$typeTag"] = str.TypeTag;
                return props;
            }

            return null;
        }

        static object SimplifyScalar(object value)
        {
            if (value == null)
                return null;

            var valueType = value.GetType();
            if (SpecialScalars.Contains(valueType))
                return value;

            return value.ToString();
        }
    }
}
