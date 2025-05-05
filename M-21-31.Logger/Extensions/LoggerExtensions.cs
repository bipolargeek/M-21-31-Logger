using M_21_31.Logger;
using M_21_31.Logger.Extensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Extensions.Logging
{
    public static class LoggerExtensions
    {
        public static void LogEvent(this ILogger logger, 
            EventType eventType, 
            EventStatus eventStatus, 
            object? message, 
            Exception? exception,
            string? commandExecuted,
            long? duration,
            string? requestBody,
            string? responseBody)
        {
            Dictionary<string, object> logEntry = new Dictionary<string, object>();

            logEntry["EventType"] = eventType.GetDescription();
            logEntry["EventTypeCode"] = (int)eventType;
            logEntry["EventStatusCode"] = (int)eventStatus;

            if (message != null)
            {
                logEntry["@Message"] = message;
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

            logger.Log<Dictionary<string, object>>(LogLevel.Information,
                eventType,
                logEntry,
                exception);
        }

        public static void Log<TState>(this ILogger logger, LogLevel logLevel, EventType eventType, TState logEntry, Exception? exception)
        {
            logger.Log<TState>(logLevel,(int)eventType, logEntry, exception, null);
        }
    }
}
