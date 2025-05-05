using log4net;
using M_21_31.Logger;
using M_21_31.Logger.Extensions;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace log4net
{
    public static class Log4Net_Entensions
    {

        public static void LogEvent(this ILog logger,
            EventType eventType,
            EventStatus eventStatus,
            object? message,
            Exception? exception,
            string? commandExecuted,
            long? duration,
            string? requestBody,
            string? responseBody)
        {
            IM_21_31_LoggerEntry? _loggerEntry = null;

#if IS_NET
            _loggerEntry = new M_21_31_LoggerEntry();
#endif

            Dictionary<string, object> logEntry = new Dictionary<string, object>();

            if (_loggerEntry != null)
            {
                logEntry = _loggerEntry.CreateEntry(eventType,
                    eventStatus,
                    null,
                    null);
            }

            logEntry["EventType"] = eventType.GetDescription();
            logEntry["EventTypeCode"] = (int)eventType;
            logEntry["EventStatusCode"] = (int)eventStatus;

            if (message != null)
            {
                logEntry["Message"] = message;
            }
            if (exception != null)
            {
                logEntry["Exception"] = exception.ToString();
            }
            if (!string.IsNullOrEmpty(commandExecuted))
            {
                logEntry["CommandExecuted"] = commandExecuted;
            }
            if (duration.HasValue)
            {
                logEntry["Duration"] = duration;
            }
            if (!string.IsNullOrEmpty(requestBody))
            {
                logEntry["RequestBody"] = requestBody;
            }
            if (!string.IsNullOrEmpty(responseBody))
            {
                logEntry["ResponseBody"] = responseBody;
            }

            //JsonSerializerOptions options = new JsonSerializerOptions
            //{
            //    PropertyNameCaseInsensitive = true,
            //    Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            //    WriteIndented = true // For pretty print
            //};

            //var jsonMsg = JsonSerializer.Serialize(logEntry, options);

            logger.Info(logEntry);
        }
    }
}
