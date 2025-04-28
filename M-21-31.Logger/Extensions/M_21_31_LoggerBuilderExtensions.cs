using M_21_31.Logger.Middleware;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog.Extensions.Logging;
using System;

namespace M_21_31.Logger.Extensions
{

    /// <summary>
    /// Extends <see cref="ILoggingBuilder"/> with Serilog configuration methods.
    /// </summary>
    public static class M_21_31_LoggerBuilderExtensions
    {
        /// <summary>
        /// Add Serilog to the logging pipeline.
        /// </summary>
        /// <param name="builder">The <see cref="T:Microsoft.Extensions.Logging.ILoggingBuilder" /> to add logging provider to.</param>
        /// <param name="logger">The Serilog logger; if not supplied, the static <see cref="Serilog.Log"/> will be used.</param>
        /// <param name="dispose">When true, dispose <paramref name="logger"/> when the framework disposes the provider. If the
        /// logger is not specified but <paramref name="dispose"/> is true, the <see cref="Log.CloseAndFlush()"/> method will be
        /// called on the static <see cref="Log"/> class instead.</param>
        /// <returns>Reference to the supplied <paramref name="builder"/>.</returns>
        public static ILoggingBuilder AddM_21_31_Logging(this ILoggingBuilder builder, Serilog.ILogger? logger = null, bool dispose = false)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));

            builder.Services.AddHttpContextAccessor();
            builder.Services.AddTransient<IM_21_31_LogEntry, M_21_31_LogEntry>();

            var serviceProvider = builder.Services.BuildServiceProvider();
            var httpContextAccessor = serviceProvider.GetRequiredService<IHttpContextAccessor>();
            var logEntry = serviceProvider.GetRequiredService<IM_21_31_LogEntry>();

            builder.ClearProviders();

            if (dispose)
            {
                builder.Services.AddSingleton<ILoggerProvider, M_21_31_LoggerProvider>(services => new M_21_31_LoggerProvider(logger, httpContextAccessor, logEntry, true));
            }
            else
            {
                builder.AddProvider(new M_21_31_LoggerProvider(logger, httpContextAccessor, logEntry));
            }

            builder.AddFilter<M_21_31_LoggerProvider>(null, LogLevel.Trace);

            return builder;
        }

        public static IApplicationBuilder UseM_21_31Logging<T>(this IApplicationBuilder builder)
        {
            var context = builder.ApplicationServices.GetRequiredService<IHttpContextAccessor>(); // resolve service
            var logger = builder.ApplicationServices.GetRequiredService<ILogger<T>>(); // resolve service
            return app.UseMiddleware<RequestResponseLoggingMiddleware>(logger);

            return builder.UseMiddleware<M_21_31_HttpRequestLoggerMiddleware>();
        }

    }

}
