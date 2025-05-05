using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace M_21_31.Logger.Middleware
{
    public class M_21_31_HttpRequestLoggerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<M_21_31_HttpRequestLoggerMiddleware> _logger;

        public M_21_31_HttpRequestLoggerMiddleware(RequestDelegate next, ILogger<M_21_31_HttpRequestLoggerMiddleware> logger) 
        {
            _next = next;
            _logger = logger; 
        }

        public async Task Invoke(HttpContext context)
        {
            var stopwatch = Stopwatch.StartNew();

            context.Request.EnableBuffering();
            var reqBody = await new StreamReader(context.Request.Body, Encoding.UTF8, true, 1024, true).ReadToEndAsync();
            context.Request.Body.Position = 0;

            var originalBody = context.Response.Body;
            using var newBody = new MemoryStream();
            context.Response.Body = newBody;

            _logger.LogEvent(EventType.Request,
                EventStatus.Success,
                null,
                null,
                null,
                0,
                reqBody,
                null);

            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogEvent(EventType.UnhandledException,
                    EventStatus.Fail,
                    null,
                    ex,
                    null,
                    null,
                    null,
                    null);

            }

            stopwatch.Stop();
            var durationMs = stopwatch.ElapsedMilliseconds;

            newBody.Position = 0;
            var resBody = await new StreamReader(newBody).ReadToEndAsync();
            newBody.Position = 0;
            await newBody.CopyToAsync(originalBody);

            _logger.LogEvent(EventType.Response,
                EventStatus.Success,
                null,
                null,
                null,
                durationMs,
                null,
                resBody);
        }
    }
}
