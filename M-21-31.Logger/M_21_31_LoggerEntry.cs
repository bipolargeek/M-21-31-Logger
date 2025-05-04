#if NETFRAMEWORK
#define IS_NET
#elif NET
#define IS_NETCORE
#endif

using M_21_31.Logger.Extensions;
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

#if IS_NET
using System.Web;
#else
using Microsoft.AspNetCore.Http;
using Microsoft.IO;
#endif

namespace M_21_31.Logger
{
    public class M_21_31_LoggerEntry : IM_21_31_LoggerEntry
    {

        private readonly IM_21_31_LoggerContext _context;


#if IS_NET
        public M_21_31_LoggerEntry()
        {
            _context = new M_21_31_LoggerContext();
        }
#endif

#if IS_NETCORE
        public M_21_31_LoggerEntry(IM_21_31_LoggerContext context)
        {
            _context = context;
        }
#endif

        public Dictionary<string, object> CreateEntry(EventTypes eventType,
            EventStatus eventStatus,
            object? message,
            Exception? exception)
        {
            var logEntry = new Dictionary<string, object>();

            logEntry["TimestampUTC"] = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ", CultureInfo.InvariantCulture);
            logEntry["TimestampUTC+4"] = DateTime.UtcNow.AddHours(4).ToString("yyyy-MM-ddTHH:mm:ss.fff+04:00", CultureInfo.InvariantCulture);
            logEntry["EventType"] = eventType.GetDescription();
            logEntry["EventTypeCode"] = (int)eventType;
            logEntry["EventStatusCode"] = (int)eventStatus;
            logEntry["DeviceIdentifier"] = GetMacAddress();
            logEntry["TransactionId"] = Guid.NewGuid().ToString();

            if (_context is IM_21_31_LoggerContext context)
            {
                logEntry["TransactionId"] = context.GetTransactionId();
                logEntry["SourceIP4"] = context.GetClientIpV4();
                logEntry["SourceIP6"] = context.GetClientIpV6();
                logEntry["DestinationIP4"] = context.GetServerIp();
                logEntry["DestinationIP6"] = null;
                logEntry["RequestHeaders"] = context.GetAllRequestHeaders();
                logEntry["RequestMethod"] = context.GetRequestMethod();
                logEntry["RequestUrl"] = context.GetRequestUrl();
                logEntry["ResponseHeaders"] = context.GetAllResponseHeaders();
                logEntry["ResponseStatusCode"] = context.GetResponseStatusCode();
                logEntry["ResponseTimeMs"] = context.GetElapsedMilliseconds();
                logEntry["Username"] = context.GetUserName();
            }

            if (message != null)
                logEntry["@Message"] = message;

            if (exception != null)
                logEntry["Exception"] = exception.ToString();

            return logEntry;
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
