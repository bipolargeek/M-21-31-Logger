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
            EventTypes eventType, 
            EventStatus eventStatus, 
            string? message, 
            Exception? exception,
            string? commandExecuted,
            long? duration,
            string? requestBody,
            string? responseBody)
        {
            Dictionary<string, object> logEntry = new Dictionary<string, object>();

            logEntry["EventType"] = eventType.GetDescription();
            logEntry["EventStatusCode"] = eventStatus;
            logEntry["EventId"] = eventType;

            if (!string.IsNullOrEmpty(message))
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
                logEntry["HttpRequestBody"] = requestBody;
            }
            if (!string.IsNullOrEmpty(responseBody))
            {
                logEntry["HttpResponseBody"] = responseBody;
            }

            logger.Log<Dictionary<string, object>>(LogLevel.Information, 
                logEntry,
                exception);
        }

        public static void Log<TState>(this ILogger logger, LogLevel logLevel, TState logEntry, Exception? exception)
        {
            logger.Log<TState>(logLevel, 0, logEntry, exception, null);
        }
    }
}
