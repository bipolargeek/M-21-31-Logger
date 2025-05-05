#if NETFRAMEWORK
#define IS_NET
#elif NET
#define IS_NETCORE
#endif

using log4net;
using log4net.Config;
using log4net.Core;
using log4net.Repository;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Xml;

namespace M_21_31.Logger
{
    public class Log4Net_Logger : ILog
    {
        private readonly ILog _logger;
        private readonly IM_21_31_LoggerEntry _loggerEntry;

        JsonSerializerOptions options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            WriteIndented = true // For pretty print
        };

        public Log4Net_Logger(Type type)
        {
            if (log4net.LogManager.GetRepository().Configured == false)
            {
                var logRepo = log4net.LogManager.GetRepository(Assembly.GetExecutingAssembly());
                log4net.Config.XmlConfigurator.Configure(logRepo, new FileInfo("log4net.config"));
            }

            _logger = log4net.LogManager.GetLogger(type);

#if IS_NET
            _loggerEntry = new M_21_31_LoggerEntry();
#endif
        }

        public bool IsDebugEnabled => _logger.IsDebugEnabled;
        public bool IsInfoEnabled => _logger.IsInfoEnabled;
        public bool IsWarnEnabled => _logger.IsWarnEnabled;
        public bool IsErrorEnabled => _logger.IsErrorEnabled;
        public bool IsFatalEnabled => _logger.IsFatalEnabled;

        public ILogger Logger => _logger.Logger;

        public void Debug(object? message)
        {
            //if (!IsDebugEnabled) return;

            var logEntry = _loggerEntry.CreateEntry(EventType.Debug,
                EventStatus.Success,
                (!(message is Dictionary<string, object>) ? message : null),
                null);

            if (message is Dictionary<string, object> dictionary)
            {
                foreach (var entry in logEntry)
                {
                    logEntry[entry.Key] = entry.Value;
                }
            }

            var jsonMsg = JsonSerializer.Serialize(logEntry, options);

            _logger.Debug(jsonMsg);
        }

        public void Debug(object? message, Exception? exception)
        {
            //if (!IsDebugEnabled) return;

            var logEntry = _loggerEntry.CreateEntry(EventType.Debug,
                EventStatus.Success,
                message,
                exception);

            var jsonMsg = JsonSerializer.Serialize(logEntry, options);

            _logger.Debug(jsonMsg);
        }

        public void DebugFormat(string format, params object?[]? args)
        {
            //if (!IsDebugEnabled) return;

            var message = string.Format(format, args);

            var logEntry = _loggerEntry.CreateEntry(EventType.Debug,
                EventStatus.Success,
                message,
                null);

            var jsonMsg = JsonSerializer.Serialize(logEntry, options);

            _logger.Debug(jsonMsg);
        }

        public void DebugFormat(string format, object? arg0)
        {
            //if (!IsDebugEnabled) return;

            var message = string.Format(format, arg0);

            var logEntry = _loggerEntry.CreateEntry(EventType.Debug,
                EventStatus.Success,
                message,
                null);

            var jsonMsg = JsonSerializer.Serialize(logEntry, options);

            _logger.Debug(jsonMsg);
        }
        public void DebugFormat(string format, object? arg0, object? arg1)
        {
            //if (!IsDebugEnabled) return;

            var message = string.Format(format, arg0, arg1);

            var logEntry = _loggerEntry.CreateEntry(EventType.Debug,
                EventStatus.Success,
                message,
                null);

            var jsonMsg = JsonSerializer.Serialize(logEntry, options);

            _logger.Debug(jsonMsg);
        }

        public void DebugFormat(string format, object? arg0, object? arg1, object? arg2)
        {
            //if (!IsDebugEnabled) return;

            var message = string.Format(format, arg0, arg1, arg2);

            var logEntry = _loggerEntry.CreateEntry(EventType.Debug,
                EventStatus.Success,
                message,
                null);

            var jsonMsg = JsonSerializer.Serialize(logEntry, options);

            _logger.Debug(jsonMsg);
        }

        public void DebugFormat(IFormatProvider? provider, string format, params object?[]? args)
        {
            //if (!IsDebugEnabled) return;

            var message = string.Format(provider, format, args);

            var logEntry = _loggerEntry.CreateEntry(EventType.Debug,
                EventStatus.Success,
                message,
                null);

            var jsonMsg = JsonSerializer.Serialize(logEntry, options);

            _logger.Debug(jsonMsg);
        }
        public void Info(object? message)
        {
            //if (!IsInfoEnabled) return;

            var logEntry = new Dictionary<string, object>();

            if (message is Dictionary<string, object> entries)
            {
                logEntry = _loggerEntry.CreateEntry(EventType.Information,
                    EventStatus.Success,
                    null,
                    null);

                foreach (var entry in entries)
                {
                    logEntry[entry.Key] = entry.Value;
                }
            }
            else
            {
                logEntry = _loggerEntry.CreateEntry(EventType.Information,
                    EventStatus.Success,
                    message,
                    null);
            }

            var jsonMsg = JsonSerializer.Serialize(logEntry, options);

            _logger.Info(jsonMsg);
        }

        public void Info(object? message, Exception? exception)
        {
            //if (!IsInfoEnabled) return;

            var logEntry = new Dictionary<string, object>();

            if (message is Dictionary<string, object> entries)
            {
                logEntry = _loggerEntry.CreateEntry(EventType.Information,
                    EventStatus.Success,
                    null,
                    exception);

                foreach (var entry in entries)
                {
                    logEntry[entry.Key] = entry.Value;
                }
            }
            else
            {
                logEntry = _loggerEntry.CreateEntry(EventType.Information,
                    EventStatus.Success,
                    message,
                    exception);
            }

            var jsonMsg = JsonSerializer.Serialize(logEntry, options);

            _logger.Info(jsonMsg);
        }

        public void InfoFormat(string format, params object?[]? args)
        {
            //if (!IsInfoEnabled) return;

            var message = string.Format(format, args);

            var logEntry = _loggerEntry.CreateEntry(EventType.Information,
                EventStatus.Success,
                message,
                null);

            var jsonMsg = JsonSerializer.Serialize(logEntry, options);

            _logger.Info(jsonMsg);
        }

        public void InfoFormat(string format, object? arg0)
        {
            //if (!IsInfoEnabled) return;

            var message = string.Format(format, arg0);

            var logEntry = _loggerEntry.CreateEntry(EventType.Information,
                EventStatus.Success,
                message,
                null);

            var jsonMsg = JsonSerializer.Serialize(logEntry, options);

            _logger.Info(jsonMsg);
        }

        public void InfoFormat(string format, object? arg0, object? arg1)
        {
            //if (!IsInfoEnabled) return;

            var message = string.Format(format, arg0, arg1);

            var logEntry = _loggerEntry.CreateEntry(EventType.Information,
                EventStatus.Success,
                message,
                null);

            var jsonMsg = JsonSerializer.Serialize(logEntry, options);

            _logger.Info(jsonMsg);
        }

        public void InfoFormat(string format, object? arg0, object? arg1, object? arg2)
        {
            //if (!IsInfoEnabled) return;

            var message = string.Format(format, arg0, arg1, arg2);

            var logEntry = _loggerEntry.CreateEntry(EventType.Information,
                EventStatus.Success,
                message,
                null);

            var jsonMsg = JsonSerializer.Serialize(logEntry, options);

            _logger.Info(jsonMsg);
        }

        public void InfoFormat(IFormatProvider? provider, string format, params object?[]? args)
        {
            //if (!IsInfoEnabled) return;

            var message = string.Format(provider, format, args);

            var logEntry = _loggerEntry.CreateEntry(EventType.Information,
                EventStatus.Success,
                message,
                null);

            var jsonMsg = JsonSerializer.Serialize(logEntry, options);

            _logger.Info(jsonMsg);
        }
        public void Warn(object? message)
        {
            //if (!IsWarnEnabled) return;

            var logEntry = _loggerEntry.CreateEntry(EventType.Warning,
                EventStatus.Success,
                message,
                null);

            var jsonMsg = JsonSerializer.Serialize(logEntry, options);

            _logger.Warn(jsonMsg);
        }

        public void Warn(object? message, Exception? exception)
        {
            //if (!IsWarnEnabled) return;

            var logEntry = _loggerEntry.CreateEntry(EventType.Warning,
                EventStatus.Success,
                message,
                exception);

            var jsonMsg = JsonSerializer.Serialize(logEntry, options);

            _logger.Warn(jsonMsg);
        }

        public void WarnFormat(string format, params object?[]? args)
        {
            //if (!IsWarnEnabled) return;

            var message = string.Format(format, args);

            var logEntry = _loggerEntry.CreateEntry(EventType.Warning,
                EventStatus.Success,
                message,
                null);

            var jsonMsg = JsonSerializer.Serialize(logEntry, options);

            _logger.Warn(jsonMsg);
        }

        public void WarnFormat(string format, object? arg0)
        {
            //if (!IsWarnEnabled) return;

            var message = string.Format(format, arg0);

            var logEntry = _loggerEntry.CreateEntry(EventType.Warning,
                EventStatus.Success,
                message,
                null);

            var jsonMsg = JsonSerializer.Serialize(logEntry, options);

            _logger.Warn(jsonMsg);
        }

        public void WarnFormat(string format, object? arg0, object? arg1)
        {
            //if (!IsWarnEnabled) return;

            var message = string.Format(format, arg0, arg1);

            var logEntry = _loggerEntry.CreateEntry(EventType.Warning,
                EventStatus.Success,
                message,
                null);

            var jsonMsg = JsonSerializer.Serialize(logEntry, options);

            _logger.Warn(jsonMsg);
        }

        public void WarnFormat(string format, object? arg0, object? arg1, object? arg2)
        {
            //if (!IsWarnEnabled) return;

            var message = string.Format(format, arg0, arg1, arg2);

            var logEntry = _loggerEntry.CreateEntry(EventType.Warning,
                EventStatus.Success,
                message,
                null);

            var jsonMsg = JsonSerializer.Serialize(logEntry, options);

            _logger.Warn(jsonMsg);
        }

        public void WarnFormat(IFormatProvider? provider, string format, params object?[]? args)
        {
            //if (!IsWarnEnabled) return;

            var message = string.Format(provider, format, args);

            var logEntry = _loggerEntry.CreateEntry(EventType.Warning,
                EventStatus.Success,
                message,
                null);

            var jsonMsg = JsonSerializer.Serialize(logEntry, options);

            _logger.Warn(jsonMsg);
        }
        public void Error(object? message)
        {
            //if (!IsErrorEnabled) return;

            var logEntry = _loggerEntry.CreateEntry(EventType.Error,
                EventStatus.Success,
                message,
                null);

            var jsonMsg = JsonSerializer.Serialize(logEntry, options);

            _logger.Error(jsonMsg);
        }

        public void Error(object? message, Exception? exception)
        {
            //if (!IsErrorEnabled) return;

            var logEntry = _loggerEntry.CreateEntry(EventType.Error,
                EventStatus.Success,
                message,
                exception);

            var jsonMsg = JsonSerializer.Serialize(logEntry, options);

            _logger.Error(jsonMsg);
        }

        public void ErrorFormat(string format, params object?[]? args)
        {
            //if (!IsErrorEnabled) return;

            var message = string.Format(format, args);

            var logEntry = _loggerEntry.CreateEntry(EventType.Error,
                EventStatus.Success,
                message,
                null);

            var jsonMsg = JsonSerializer.Serialize(logEntry, options);

            _logger.Error(jsonMsg);
        }

        public void ErrorFormat(string format, object? arg0)
        {
            //if (!IsErrorEnabled) return;

            var message = string.Format(format, arg0);

            var logEntry = _loggerEntry.CreateEntry(EventType.Error,
                EventStatus.Success,
                message,
                null);

            var jsonMsg = JsonSerializer.Serialize(logEntry, options);

            _logger.Error(jsonMsg);
        }

        public void ErrorFormat(string format, object? arg0, object? arg1)
        {
            //if (!IsErrorEnabled) return;

            var message = string.Format(format, arg0, arg1);

            var logEntry = _loggerEntry.CreateEntry(EventType.Error,
                EventStatus.Success,
                message,
                null);

            var jsonMsg = JsonSerializer.Serialize(logEntry, options);

            _logger.Error(jsonMsg);
        }

        public void ErrorFormat(string format, object? arg0, object? arg1, object? arg2)
        {
            //if (!IsErrorEnabled) return;

            var message = string.Format(format, arg0, arg1, arg2);

            var logEntry = _loggerEntry.CreateEntry(EventType.Error,
                EventStatus.Success,
                message,
                null);

            var jsonMsg = JsonSerializer.Serialize(logEntry, options);

            _logger.Error(jsonMsg);
        }

        public void ErrorFormat(IFormatProvider? provider, string format, params object?[]? args)
        {
            //if (!IsErrorEnabled) return;

            var message = string.Format(provider, format, args);

            var logEntry = _loggerEntry.CreateEntry(EventType.Error,
                EventStatus.Success,
                message,
                null);

            var jsonMsg = JsonSerializer.Serialize(logEntry, options);

            _logger.Error(jsonMsg);
        }
        public void Fatal(object? message)
        {
            //if (!IsFatalEnabled) return;

            var logEntry = _loggerEntry.CreateEntry(EventType.Fatal,
                EventStatus.Success,
                message,
                null);

            var jsonMsg = JsonSerializer.Serialize(logEntry, options);

            _logger.Fatal(jsonMsg);
        }

        public void Fatal(object? message, Exception? exception)
        {
            //if (!IsFatalEnabled) return;

            var logEntry = _loggerEntry.CreateEntry(EventType.Fatal,
                EventStatus.Success,
                message,
                exception);

            var jsonMsg = JsonSerializer.Serialize(logEntry, options);

            _logger.Fatal(jsonMsg);
        }

        public void FatalFormat(string format, params object?[]? args)
        {
            //if (!IsFatalEnabled) return;

            var message = string.Format(format, args);

            var logEntry = _loggerEntry.CreateEntry(EventType.Fatal,
                EventStatus.Success,
                message,
                null);

            var jsonMsg = JsonSerializer.Serialize(logEntry, options);

            _logger.Fatal(jsonMsg);
        }

        public void FatalFormat(string format, object? arg0)
        {
            //if (!IsFatalEnabled) return;

            var message = string.Format(format, arg0);

            var logEntry = _loggerEntry.CreateEntry(EventType.Fatal,
                EventStatus.Success,
                message,
                null);

            var jsonMsg = JsonSerializer.Serialize(logEntry, options);

            _logger.Fatal(jsonMsg);
        }

        public void FatalFormat(string format, object? arg0, object? arg1)
        {
            //if (!IsFatalEnabled) return;

            var message = string.Format(format, arg0, arg1);

            var logEntry = _loggerEntry.CreateEntry(EventType.Fatal,
                EventStatus.Success,
                message,
                null);

            var jsonMsg = JsonSerializer.Serialize(logEntry, options);

            _logger.Fatal(jsonMsg);
        }

        public void FatalFormat(string format, object? arg0, object? arg1, object? arg2)
        {
            //if (!IsFatalEnabled) return;

            var message = string.Format(format, arg0, arg1, arg2);

            var logEntry = _loggerEntry.CreateEntry(EventType.Fatal,
                EventStatus.Success,
                message,
                null);

            var jsonMsg = JsonSerializer.Serialize(logEntry, options);

            _logger.Fatal(jsonMsg);
        }

        public void FatalFormat(IFormatProvider? provider, string format, params object?[]? args)
        {
            //if (!IsFatalEnabled) return;

            var message = string.Format(provider, format, args);

            var logEntry = _loggerEntry.CreateEntry(EventType.Fatal,
                EventStatus.Success,
                message,
                null);

            var jsonMsg = JsonSerializer.Serialize(logEntry, options);

            _logger.Fatal(jsonMsg);
        }

    }
}
