
using Microsoft.Extensions.Logging;
using Serilog.Core;
using Serilog.Events;
using FrameworkLogger = Microsoft.Extensions.Logging.ILogger;
using System.Reflection;
using Serilog.Debugging;
using System.Collections.Concurrent;
using Serilog.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System;
using Microsoft.AspNetCore.Http;
using System.Text.Json;

namespace M_21_31.Logger
{
    class M_21_31_Logger : FrameworkLogger
    {
        internal static readonly ConcurrentDictionary<string, string> DestructureDictionary = new ConcurrentDictionary<string, string>();
        internal static readonly ConcurrentDictionary<string, string> StringifyDictionary = new ConcurrentDictionary<string, string>();

        internal static string GetKeyWithoutFirstSymbol(ConcurrentDictionary<string, string> source, string key)
        {
            if (source.TryGetValue(key, out var value))
                return value;
            if (source.Count < 1000)
                return source.GetOrAdd(key, k => k.Substring(1));
            return key.Substring(1);
        }

        readonly M_21_31_LoggerProvider _provider;
        readonly Serilog.ILogger _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;
        IM_21_31_LogEntry _logEntry;

        static readonly M_21_31_CachingMessageTemplateParser MessageTemplateParser = new M_21_31_CachingMessageTemplateParser();

        // It's rare to see large event ids, as they are category-specific
        static readonly LogEventProperty[] LowEventIdValues = Enumerable.Range(0, 48)
            .Select(n => new LogEventProperty("Id", new ScalarValue(n)))
            .ToArray();

        public M_21_31_Logger(
            M_21_31_LoggerProvider provider,
            Serilog.ILogger? logger = null,
            IHttpContextAccessor? httpContextAccessor = null,
            IM_21_31_LogEntry? logEntry = null,
            string? name = null)
        {
            _provider = provider ?? throw new ArgumentNullException(nameof(provider));

            // If a logger was passed, the provider has already added itself as an enricher
            _logger = logger ?? Serilog.Log.Logger.ForContext(new[] { provider });

            if (name != null)
            {
                _logger = _logger.ForContext(Constants.SourceContextPropertyName, name);
            }

            _httpContextAccessor = httpContextAccessor;
            _logEntry = logEntry;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return logLevel != LogLevel.None && _logger.IsEnabled(LevelConvert.ToSerilogLevel(logLevel));
        }

        public IDisposable BeginScope<TState>(TState state) where TState : notnull
        {
            return _provider.BeginScope(state);
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            if (logLevel == LogLevel.None)
            {
                return;
            }
            var level = LevelConvert.ToSerilogLevel(logLevel);
            if (!_logger.IsEnabled(level))
            {
                return;
            }

            var logEntry = new Dictionary<string, object>();

            var message = formatter(state, exception);

            if (_logEntry != null)
            {
                if (!(state is Dictionary<string, object>))
                {
                    logEntry = _logEntry.CreateEntry(GetEvent(eventId.Id),
                        EventStatus.Success,
                        message,
                        exception);

                }
            }

            //LogEvent? evt = null;
            //try
            //{
            //    evt = PrepareWrite(level, eventId, state, exception, formatter);
            //}
            //catch (Exception ex)
            //{
            //    SelfLog.WriteLine($"Failed to write event through {nameof(M_21_31_Logger)}: {ex}");
            //}

            //// Do not swallow exceptions from here because Serilog takes care of them in case of WriteTo and throws them back to the caller in case of AuditTo.
            //if (evt != null)
            //    _logger.Write(evt);

            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };
            string jsonText = JsonSerializer.Serialize(logEntry, options);

            _logger.Write(ConvertLevel(logLevel), jsonText, exception);
        }

        LogEvent PrepareWrite<TState>(LogEventLevel level, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            string? messageTemplate = null;

            var properties = new List<LogEventProperty>();

            if (state is IEnumerable<KeyValuePair<string, object>> structure)
            {
                foreach (var property in structure)
                {
                    if (property.Key == M_21_31_LoggerProvider.OriginalFormatPropertyName && property.Value is string value)
                    {
                        messageTemplate = value;
                    }
                    else if (property.Key.StartsWith("@"))
                    {
                        if (_logger.BindProperty(GetKeyWithoutFirstSymbol(DestructureDictionary, property.Key), property.Value, true, out var destructured))
                            properties.Add(destructured);
                    }
                    else if (property.Key.StartsWith("$"))
                    {
                        if (_logger.BindProperty(GetKeyWithoutFirstSymbol(StringifyDictionary, property.Key), property.Value?.ToString(), true, out var stringified))
                            properties.Add(stringified);
                    }
                    else
                    {
                        if (_logger.BindProperty(property.Key, property.Value, false, out var bound))
                            properties.Add(bound);
                    }
                }

                var stateType = state.GetType();
                var stateTypeInfo = stateType.GetTypeInfo();
                // Imperfect, but at least eliminates `1 names
                if (messageTemplate == null && !stateTypeInfo.IsGenericType)
                {
                    messageTemplate = "{" + stateType.Name + ":l}";
                    if (_logger.BindProperty(stateType.Name, AsLoggableValue((TState)state, formatter), false, out var stateTypeProperty))
                        properties.Add(stateTypeProperty);
                }
            }

            if (messageTemplate == null)
            {
                string? propertyName = null;
                if (state != null)
                {
                    propertyName = "State";
                    messageTemplate = "{State:l}";
                }
                // `formatter` was originally accepted as nullable, so despite the new annotation, this check should still
                // be made.
                else if (formatter != null!)
                {
                    propertyName = "Message";
                    messageTemplate = "{Message:l}";
                }

                if (propertyName != null)
                {
                    if (_logger.BindProperty(propertyName, AsLoggableValue((TState)state, formatter!), false, out var property))
                        properties.Add(property);
                }
            }

            if (eventId.Id != 0 || eventId.Name != null)
                properties.Add(CreateEventIdProperty(eventId));

            var parsedTemplate = MessageTemplateParser.Parse(messageTemplate ?? "");
            return new LogEvent(DateTimeOffset.Now, level, exception, parsedTemplate, properties);
        }

        static object? AsLoggableValue<TState>(TState state, Func<TState, Exception?, string>? formatter)
        {
            object? stateObj = state;
            if (formatter != null)
                stateObj = formatter(state, null);
            return stateObj;
        }

        internal static LogEventProperty CreateEventIdProperty(EventId eventId)
        {
            var properties = new List<LogEventProperty>(2);

            if (eventId.Id != 0)
            {
                if (eventId.Id >= 0 && eventId.Id < LowEventIdValues.Length)
                    // Avoid some allocations
                    properties.Add(LowEventIdValues[eventId.Id]);
                else
                    properties.Add(new LogEventProperty("Id", new ScalarValue(eventId.Id)));
            }

            if (eventId.Name != null)
            {
                properties.Add(new LogEventProperty("Name", new ScalarValue(eventId.Name)));
            }

            return new LogEventProperty("EventId", new StructureValue(properties));
        }

        private static EventTypes GetEvent(int eventId)
        {
            if (!EnumValueExists<EventTypes>(eventId))
            {
                return EventTypes.Information;
            }
            return (EventTypes)eventId;
        }

        // Method to check if a value exists in an enum
        public static bool EnumValueExists<TEnum>(int value) where TEnum : Enum
        {
            return Enum.IsDefined(typeof(TEnum), value);
        }

        private static Serilog.Events.LogEventLevel ConvertLevel(LogLevel level) => level switch
        {
            LogLevel.Trace => Serilog.Events.LogEventLevel.Verbose,
            LogLevel.Debug => Serilog.Events.LogEventLevel.Debug,
            LogLevel.Information => Serilog.Events.LogEventLevel.Information,
            LogLevel.Warning => Serilog.Events.LogEventLevel.Warning,
            LogLevel.Error => Serilog.Events.LogEventLevel.Error,
            LogLevel.Critical => Serilog.Events.LogEventLevel.Fatal,
            _ => Serilog.Events.LogEventLevel.Information
        };
    }
}