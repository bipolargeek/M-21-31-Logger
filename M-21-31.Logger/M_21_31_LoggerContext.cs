#if NETFRAMEWORK
#define IS_NET
#elif NET
#define IS_NETCORE
#endif

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

#if IS_NET
using System.Web;
#else
using Microsoft.AspNetCore.Http;
using Microsoft.IO;
#endif

namespace M_21_31.Logger
{
#if IS_NET
    public class M_21_31_LoggerContext : IM_21_31_LoggerContext
    {
        private readonly HttpContext _context;
        private static readonly string ResponseBodyKey = "__response_body_stream__";
        private static readonly string OriginalStreamKey = "__original_response_stream__";
        private static readonly string StartTimeKey = "__httprequest_starttime__";
        private static readonly string TransactionIdKey = "__transaction_id__";

        public M_21_31_LoggerContext()
        {
            _context = HttpContext.Current ?? null;
        }

        public void BeginCapture()
        {
            try
            {
                if (_context?.Response != null)
                {
                    var originalStream = _context.Response.Filter;
                    var captureStream = new MemoryStream();

                    _context.Items[OriginalStreamKey] = originalStream;
                    _context.Items[ResponseBodyKey] = captureStream;
                    _context.Response.Filter = captureStream;

                    _context.Items[TransactionIdKey] = Guid.NewGuid().ToString();
                    _context.Items[StartTimeKey] = DateTime.UtcNow;
                }
            }
            catch { }
        }

        public void EndCapture()
        {
            try
            {
                if (_context != null &&
                _context.Items[OriginalStreamKey] is Stream originalStream &&
                _context.Items[ResponseBodyKey] is MemoryStream captureStream)
            {
                captureStream.Position = 0;
                captureStream.CopyTo(originalStream);
                _context.Response.Filter = originalStream;
                }
            }
            catch { }
        }

        public async Task<string> GetResponseBody()
        {
            try
            {
                if (_context != null &&
                _context.Items[ResponseBodyKey] is MemoryStream captureStream)
            {
                captureStream.Position = 0;
                using (var reader = new StreamReader(captureStream, _context.Response.ContentEncoding, true, 1024, leaveOpen: true))
                    return await reader.ReadToEndAsync();
            }
        }
            catch { }
            return null;
        }

        public string GetTransactionId()
        {
            if (_context != null &&
                _context.Items[TransactionIdKey] is string transactionId)
                return transactionId;
            return Guid.NewGuid().ToString();
        }

        public double GetElapsedMilliseconds()
        {
            if (_context != null &&
                _context.Items[StartTimeKey] is DateTime start)
                return (DateTime.UtcNow - start).TotalMilliseconds;
            return 0;
        }

        public string GetRequestUrl() => (_context != null ? _context?.Request?.Url?.ToString() : string.Empty);

        public string GetRequestMethod() => (_context != null ? _context?.Request?.HttpMethod : string.Empty);

        public Dictionary<string, string> GetAllRequestHeaders()
        {
            var headers = new Dictionary<string, string>();
            if (_context?.Request != null)
            {
                foreach (string key in _context.Request.Headers)
                    headers[key] = _context.Request.Headers[key];
            }
            return headers;
        }

        public Dictionary<string, string> GetAllResponseHeaders()
        {
            var headers = new Dictionary<string, string>();
            try
            {
                if (_context?.Response != null)
                {
                    foreach (string key in _context.Response.Headers)
                        headers[key] = _context.Response.Headers[key];
                }
            }
            catch { }
            return headers;
        }

        public async Task<string> GetRequestBody()
        {
            if (_context?.Request != null)
            {
                _context.Request.InputStream.Position = 0;
                using (var reader = new StreamReader(_context.Request.InputStream, _context.Request.ContentEncoding))
                    return await reader.ReadToEndAsync();
            }
            return string.Empty;
        }

        public string GetResponseStatusCode() => (_context != null ? _context?.Response?.StatusCode.ToString() : string.Empty);

        public string GetClientIpV4() => GetIpAddress(AddressFamily.InterNetwork);

        public string GetClientIpV6() => GetIpAddress(AddressFamily.InterNetworkV6);

        public string GetServerIp() => (_context != null ? _context?.Request?.ServerVariables["LOCAL_ADDR"] : string.Empty);

        public string GetServerMacAddress() => GetMacFromIp(GetServerIp());

        public string GetUserName()
        {
            if (_context != null)
            {
                var user = _context?.User?.Identity;
                return (user != null && user.IsAuthenticated) ? user.Name : "anonymous";
            }
            return string.Empty;
        }

        private string GetIpAddress(AddressFamily family)
        {
            if (_context != null)
            {
                var ip = IPAddress.TryParse(_context?.Request?.UserHostAddress, out var parsed)
                    ? parsed
                    : null;
                return (ip != null && ip.AddressFamily == family) ? ip.ToString() : null;
            }
            return string.Empty;
        }

        private static string GetMacFromIp(string ipAddress)
        {
            if (string.IsNullOrWhiteSpace(ipAddress)) return null;
            foreach (var nic in NetworkInterface.GetAllNetworkInterfaces())
            {
                var props = nic.GetIPProperties();
                foreach (var addr in props.UnicastAddresses)
                {
                    if (addr.Address.ToString() == ipAddress)
                        return string.Join(":", nic.GetPhysicalAddress().GetAddressBytes().Select(b => b.ToString("X2")));
                }
            }
            return null;
        }
    }

    /*
    // Global.asax for .NET Framework to use LoggingContext

    public class Global : HttpApplication
    {
        protected void Application_BeginRequest(object sender, EventArgs e)
        {
            var logger = new M_21_31.Logger.LoggingContext();
            logger.BeginCapture();
            HttpContext.Current.Items["Logger"] = logger;
        }

        protected void Application_EndRequest(object sender, EventArgs e)
        {
            if (HttpContext.Current.Items["Logger"] is M_21_31.Logger.LoggingContext logger)
            {
                logger.EndCapture();
                var responseBody = logger.GetResponseBody();
                var duration = logger.GetElapsedMilliseconds();
                // log or use responseBody/duration
            }
        }
    }
    */

#else
    public class M_21_31_LoggerContext : IM_21_31_LoggerContext
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private static readonly RecyclableMemoryStreamManager _streamManager = new();
        private static readonly string ResponseBodyKey = "__response_body_stream__";
        private static readonly string OriginalStreamKey = "__original_response_stream__";
        private static readonly string StartTimeKey = "__httprequest_starttime__";
        private static readonly string TransactionIdKey = "__transaction_id__";

        public M_21_31_LoggerContext(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }
         
        public void BeginCapture()
        {
            if (_httpContextAccessor.HttpContext is HttpContext context)
            {
                var originalStream = context.Response.Body;
                var captureStream = _streamManager.GetStream();

                context.Items[OriginalStreamKey] = originalStream;
                context.Items[ResponseBodyKey] = captureStream;
                context.Response.Body = captureStream;

                context.Items[TransactionIdKey] = Guid.NewGuid().ToString();
                context.Items[StartTimeKey] = DateTime.UtcNow;
            }
        }

        public void EndCapture()
        {
            if (_httpContextAccessor.HttpContext is HttpContext context)
            {
                if (context.Items[OriginalStreamKey] is Stream original &&
                context.Items[ResponseBodyKey] is MemoryStream capture)
                {
                    capture.Position = 0;
                    capture.CopyTo(original);
                    context.Response.Body = original;
                }
            }
        }

        public async Task<string> GetResponseBody()
        {
            if (_httpContextAccessor.HttpContext is HttpContext context)
            {
                if (context.Items[ResponseBodyKey] is MemoryStream stream)
                {
                    stream.Position = 0;
                    using var reader = new StreamReader(stream, Encoding.UTF8, true, 1024, leaveOpen: true);
                    var content = await reader.ReadToEndAsync();
                    stream.Position = 0;
                    return content;
                }
            }
            return string.Empty;
        }

        public async Task<string> GetRequestBody()
        {
            if (_httpContextAccessor.HttpContext is HttpContext context)
            {
                context.Request.EnableBuffering();
                using var stream = _streamManager.GetStream();
                await context.Request.Body.CopyToAsync(stream);
                context.Request.Body.Position = 0;
                return Encoding.UTF8.GetString(stream.ToArray());
            }

            return string.Empty;
        }

        public double GetElapsedMilliseconds()
        {
            if (_httpContextAccessor.HttpContext is HttpContext context)
            {
                if (context.Items[StartTimeKey] is DateTime start)
                    return (DateTime.UtcNow - start).TotalMilliseconds;
            }
            return 0;
        }

        public string GetTransactionId()
        {
            if (_httpContextAccessor.HttpContext is HttpContext context)
            {
                if (context.Items[TransactionIdKey] is string transactionId)
                    return transactionId;
            }
            return Guid.NewGuid().ToString();
        }

        public string GetRequestUrl()
        {
            if (_httpContextAccessor.HttpContext is HttpContext context)
            {
                var req = context?.Request;
                return req != null ? $"{req.Scheme}://{req.Host}{req.Path}{req.QueryString}" : null;
            }
            return string.Empty;
        }

        public string GetRequestMethod()
        {
            if (_httpContextAccessor.HttpContext is HttpContext context)
            {
                return context?.Request?.Method;
            }
            return string.Empty;
        }

        public Dictionary<string, string> GetAllRequestHeaders()
        {
            if (_httpContextAccessor.HttpContext is HttpContext context)
            {
                return context?.Request?.Headers.ToDictionary(h => h.Key, h => h.Value.ToString());
            }
            return new Dictionary<string, string>();
        }

        public Dictionary<string, string> GetAllResponseHeaders()
        {
            if (_httpContextAccessor.HttpContext is HttpContext context)
            {
                return context?.Response?.Headers.ToDictionary(h => h.Key, h => h.Value.ToString());
            }
            return new Dictionary<string, string>();
        }

        public string GetResponseStatusCode()
        {
            if (_httpContextAccessor.HttpContext is HttpContext context)
            {
                return context?.Response?.StatusCode.ToString() ?? string.Empty;
            }
            return string.Empty;
        }

        public string GetClientIpV4() => GetIpByFamily(AddressFamily.InterNetwork);

        public string GetClientIpV6() => GetIpByFamily(AddressFamily.InterNetworkV6);

        public string GetServerIp()
        {
            if (_httpContextAccessor.HttpContext is HttpContext context)
            {
                context?.Connection?.LocalIpAddress?.ToString();
            }
            return string.Empty;
        }

        public string GetServerMacAddress() => GetMacFromIp(GetServerIp());

        public string GetUserName()
        {
            if (_httpContextAccessor.HttpContext is HttpContext context)
            {
                var user = context?.User?.Identity;
                return (user != null && user.IsAuthenticated) ? user.Name : "anonymous";
            }
            return "anonymous";
        }

        private string GetIpByFamily(AddressFamily family)
        {
            if (_httpContextAccessor.HttpContext is HttpContext context)
            {
                var ip = context?.Connection?.RemoteIpAddress;
                return (ip != null && ip.AddressFamily == family) ? ip.ToString() : string.Empty;
            }
            return string.Empty;
        }

        private static string GetMacFromIp(string ipAddress)
        {
            if (string.IsNullOrWhiteSpace(ipAddress)) return null;
            foreach (var nic in NetworkInterface.GetAllNetworkInterfaces())
            {
                var props = nic.GetIPProperties();
                foreach (var addr in props.UnicastAddresses)
                {
                    if (addr.Address.ToString() == ipAddress)
                        return string.Join(":", nic.GetPhysicalAddress().GetAddressBytes().Select(b => b.ToString("X2")));
                }
            }
            return string.Empty;
        }
    }

    /*
    // ASP.NET Core Middleware

    public class ResponseCaptureMiddleware
    {
        private readonly RequestDelegate _next;

        public ResponseCaptureMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var logger = new M_21_31.Logger.LoggingContext(context);
            logger.BeginCapture();
            context.Items["Logger"] = logger;

            await _next(context);

            logger.EndCapture();
            var responseBody = await logger.GetResponseBodyAsync();
            var duration = logger.GetElapsedMilliseconds();
            // log or use responseBody/duration
        }
    }

    // Register in Startup.cs or Program.cs
    // app.UseMiddleware<M_21_31.Logger.ResponseCaptureMiddleware>();
    */
#endif
}
