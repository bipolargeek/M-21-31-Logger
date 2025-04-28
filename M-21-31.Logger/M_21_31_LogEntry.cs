using M_21_31.Logger.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text.Json;

namespace M_21_31.Logger
{
    public class M_21_31_LogEntry : IM_21_31_LogEntry
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public M_21_31_LogEntry(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public Dictionary<string, object> CreateEntry(EventTypes eventType,
            EventStatus eventStatus,
            object message,
            Exception? exception)
        {
            var context = _httpContextAccessor.HttpContext;
            var logEntry = new Dictionary<string, object>();

            logEntry["TimestampUTC"] = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ", CultureInfo.InvariantCulture);
            logEntry["TimestampUTC+4"] = DateTime.UtcNow.AddHours(4).ToString("yyyy-MM-ddTHH:mm:ss.fff+04:00", CultureInfo.InvariantCulture);
            logEntry["EventType"] = eventType.GetDescription();
            logEntry["EventId"] = eventType;
            logEntry["EventStatusCode"] = eventStatus;
            logEntry["DeviceIdentifier"] = GetMacAddress();
            logEntry["TransactionId"] = Guid.NewGuid().ToString();

            if (context != null)
            {
                logEntry["TransactionId"] = context.TraceIdentifier;

                if (context.Connection != null)
                {
                    logEntry["SourceIP4"] = GetIpAddress(context.Connection.RemoteIpAddress, AddressFamily.InterNetwork);
                    logEntry["SourceIP6"] = GetIpAddress(context.Connection.RemoteIpAddress, AddressFamily.InterNetworkV6);
                    logEntry["DestinationIP4"] = GetIpAddress(context.Connection.LocalIpAddress, AddressFamily.InterNetwork);
                    logEntry["DestinationIP6"] = GetIpAddress(context.Connection.LocalIpAddress, AddressFamily.InterNetworkV6);
                }
                if (context.Request != null)
                {
                    logEntry["HttpRequestHeaders"] = context.Request.Headers.ToDictionary(h => h.Key, h => h.Value.ToString());
                    logEntry["HttpRequestMethod"] = context.Request.Method;
                    logEntry["HttpRequestUrl"] = $"{context.Request.Scheme}://{context.Request.Host}{context.Request.Path}{context.Request.QueryString}";
                }
                if (context.Response != null)
                {
                    logEntry["HttpResponseHeaders"] = context.Response.Headers.ToDictionary(h => h.Key, h => h.Value.ToString());
                    logEntry["HttpResponseStatusCode"] = context.Response.StatusCode;
                }

                logEntry["HttpResponseTimeMs"] = GetElapsedMilliseconds(context);
                logEntry["Username"] = context.User?.Identity?.Name ?? string.Empty;
            }

            logEntry["Message"] = message;

            if (exception != null)
                logEntry["Exception"] = exception?.ToString();

            return logEntry;
        }

        //public string GetLogEntry(EventTypes eventType,
        //    EventStatus eventStatus,
        //    string message,
        //    Exception? exception = null)
        //{
        //    var context = _httpContextAccessor.HttpContext;

        //    var logEntry = new Dictionary<string, object>();

        //    logEntry["TimestampUTC"] = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ", CultureInfo.InvariantCulture);
        //    logEntry["TimestampUTC+4"] = DateTime.UtcNow.AddHours(4).ToString("yyyy-MM-ddTHH:mm:ss.fff+04:00", CultureInfo.InvariantCulture);
        //    logEntry["EventType"] = eventType.GetDescription();
        //    logEntry["EventId"] = eventType;
        //    logEntry["EventStatusCode"] = eventStatus;
        //    logEntry["DeviceIdentifier"] = GetMacAddress();
        //    logEntry["SessionTransactionId"] = context.TraceIdentifier;
        //    logEntry["SourceIP4"] = GetIpAddress(context.Connection.RemoteIpAddress, AddressFamily.InterNetwork);
        //    logEntry["SourceIP6"] = GetIpAddress(context.Connection.RemoteIpAddress, AddressFamily.InterNetworkV6);
        //    logEntry["DestinationIP4"] = GetIpAddress(context.Connection.LocalIpAddress, AddressFamily.InterNetwork);
        //    logEntry["DestinationIP6"] = GetIpAddress(context.Connection.LocalIpAddress, AddressFamily.InterNetworkV6);
        //    logEntry["HttpRequestHeaders"] = context.Request.Headers.ToDictionary(h => h.Key, h => h.Value.ToString());
        //    logEntry["HttpRequestMethod"] = context.Request.Method;
        //    logEntry["HttpRequestUrl"] = $"{context.Request.Scheme}://{context.Request.Host}{context.Request.Path}{context.Request.QueryString}";
        //    logEntry["HttpResponseHeaders"] = context.Response.Headers.ToDictionary(h => h.Key, h => h.Value.ToString());
        //    logEntry["HttpResponseStatusCode"] = context.Response.StatusCode;
        //    //logEntry["HttpResponseTimeMs"] = elapsedMs;
        //    logEntry["Username"] = context.User?.Identity?.Name ?? string.Empty;
        //    logEntry["Message"] = message;

        //    if (exception != null)
        //        logEntry["Exception"] = exception?.ToString();

        //    var formattedLog = JsonSerializer.Serialize(logEntry, new JsonSerializerOptions { WriteIndented = false });

        //    return formattedLog;
        //}

        private static double GetElapsedMilliseconds(HttpContext context)
        {
            if (context.Items["__httprequest_starttime__"] is DateTime starttime)
            {
                return (DateTime.UtcNow - starttime).TotalMilliseconds;
            }
            return 0;
        }

        private static string GetIpAddress(IPAddress ipAddress, AddressFamily family)
        {
            if (ipAddress == null) return null;
            if (family == AddressFamily.InterNetwork && ipAddress.AddressFamily == AddressFamily.InterNetwork) return ipAddress.ToString();
            if (family == AddressFamily.InterNetworkV6 && ipAddress.AddressFamily == AddressFamily.InterNetworkV6) return ipAddress.ToString();
            return null;
        }

        private static string GetMacAddress()
        {
            var networkInterfaces = NetworkInterface.GetAllNetworkInterfaces();
            foreach (var nic in networkInterfaces)
            {
                if (nic.OperationalStatus == OperationalStatus.Up)
                {
                    var address = nic.GetPhysicalAddress();
                    if (address != null && address.ToString() != "")
                        return address.ToString();
                }
            }
            return "Unknown";
        }
    }
}
