
using Microsoft.Extensions.Logging;
using Serilog.Core;
using Serilog.Events;
using FrameworkLogger = Microsoft.Extensions.Logging.ILogger;
using Serilog.Context;
using System.Collections.Generic;
using System.Threading;
using System;
using Microsoft.AspNetCore.Http;

namespace M_21_31.Logger
{
    /// <summary>
    /// An <see cref="ILoggerProvider"/> that pipes events through Serilog.
    /// </summary>
    [ProviderAlias("Serilog")]
    public class M_21_31_LoggerProvider : ILoggerProvider, ILogEventEnricher
    {
        internal const string OriginalFormatPropertyName = "{OriginalFormat}";
        internal const string ScopePropertyName = "Scope";

        // May be null; if it is, Log.Logger will be lazily used
        readonly Serilog.ILogger? _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IM_21_31_LogEntry _logEntry;
        readonly Action? _dispose;

        /// <summary>
        /// Construct a <see cref="M_21_31_LoggerProvider"/>.
        /// </summary>
        /// <param name="logger">A Serilog logger to pipe events through; if null, the static <see cref="Log"/> class will be used.</param>
        /// <param name="dispose">If true, the provided logger or static log class will be disposed/closed when the provider is disposed.</param>
        public M_21_31_LoggerProvider(Serilog.ILogger? logger = null, IHttpContextAccessor? httpContextAccessor = null, IM_21_31_LogEntry? logEntry = null, bool dispose = false)
        {
            if (logger != null)
                _logger = logger.ForContext(new[] { this });

            if (httpContextAccessor != null)
                _httpContextAccessor = httpContextAccessor;

            _logEntry = logEntry;

            if (dispose)
            {
                if (logger != null)
                    _dispose = () => (logger as IDisposable)?.Dispose();
                else
                    _dispose = Serilog.Log.CloseAndFlush;
            }
        }

        /// <inheritdoc />
        public FrameworkLogger CreateLogger(string name)
        {
            return new M_21_31_Logger(this, _logger, _httpContextAccessor, _logEntry, name);
        }

        /// <inheritdoc cref="IDisposable" />
        public IDisposable BeginScope<T>(T state)
        {
            if (CurrentScope != null)
                return new M_21_31_LoggerScope(this, state);

            // The outermost scope pushes and pops the Serilog `LogContext` - once
            // this enricher is on the stack, the `CurrentScope` property takes care
            // of the rest of the `BeginScope()` stack.
            var popSerilogContext = LogContext.Push(this);
            return new M_21_31_LoggerScope(this, state, popSerilogContext);
        }

        /// <inheritdoc />
        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            List<LogEventPropertyValue>? scopeItems = null;
            for (var scope = CurrentScope; scope != null; scope = scope.Parent)
            {
                scope.EnrichAndCreateScopeItem(logEvent, propertyFactory, out var scopeItem);

                if (scopeItem != null)
                {
                    scopeItems ??= new List<LogEventPropertyValue>();
                    scopeItems.Add(scopeItem);
                }
            }

            if (scopeItems != null)
            {
                scopeItems.Reverse();
                logEvent.AddPropertyIfAbsent(new LogEventProperty(ScopePropertyName, new SequenceValue(scopeItems)));
            }
        }

        readonly AsyncLocal<M_21_31_LoggerScope?> _value = new AsyncLocal<M_21_31_LoggerScope?>();

        internal M_21_31_LoggerScope? CurrentScope
        {
            get => _value.Value;
            set => _value.Value = value;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            _dispose?.Invoke();
        }
    }
}
