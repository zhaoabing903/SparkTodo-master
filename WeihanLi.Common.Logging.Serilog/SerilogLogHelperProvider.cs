﻿using JetBrains.Annotations;
using Serilog;
using Serilog.Events;
using Serilog.Parsing;
using System;
using System.Collections.Generic;

using SSerilog = Serilog;

namespace WeihanLi.Common.Logging.Serilog
{
    internal sealed class SerilogLogHelperProvider : ILogHelperProvider, IDisposable
    {
        private static readonly MessageTemplateParser MessageTemplateParser = new MessageTemplateParser();

        public SerilogLogHelperProvider(LoggerConfiguration configuration)
        {
            SerilogHelper.LogInit(configuration);
        }

        public SerilogLogHelperProvider(Action<LoggerConfiguration> configurationAction)
        {
            SerilogHelper.LogInit(configurationAction);
        }

        public void Dispose()
        {
            SSerilog.Log.CloseAndFlush();
        }

        public void Log(LogHelperLoggingEvent loggingEvent)
        {
            var logger = SSerilog.Log.ForContext(SourceContextPropName, loggingEvent.CategoryName);
            
            var logLevel = GetSerilogEventLevel(loggingEvent.LogLevel);
            if (logger.IsEnabled(logLevel))
            {
                var messageTemplate = loggingEvent.MessageTemplate;
                var properties = new List<LogEventProperty>();
                if (loggingEvent.Properties != null)
                {
                    foreach (var property in loggingEvent.Properties)
                    {
                        if (logger.BindProperty(property.Key, property.Value, false, out var bound))
                            properties.Add(bound);
                    }
                }
                var parsedTemplate = MessageTemplateParser.Parse(messageTemplate ?? "");
                logger.Write(new LogEvent(loggingEvent.DateTime, logLevel, loggingEvent.Exception, parsedTemplate, properties));
            }
        }

        private SSerilog.Events.LogEventLevel GetSerilogEventLevel(LogHelperLogLevel logHelperLevel)
        {
            switch (logHelperLevel)
            {
                case LogHelperLogLevel.All:
                    return SSerilog.Events.LogEventLevel.Verbose;

                case LogHelperLogLevel.Debug:
                    return SSerilog.Events.LogEventLevel.Debug;

                case LogHelperLogLevel.Info:
                    return SSerilog.Events.LogEventLevel.Information;

                case LogHelperLogLevel.Trace:
                    return SSerilog.Events.LogEventLevel.Debug;

                case LogHelperLogLevel.Warn:
                    return SSerilog.Events.LogEventLevel.Warning;

                case LogHelperLogLevel.Error:
                    return SSerilog.Events.LogEventLevel.Error;

                case LogHelperLogLevel.Fatal:
                    return SSerilog.Events.LogEventLevel.Fatal;

                case LogHelperLogLevel.None:
                    return SSerilog.Events.LogEventLevel.Fatal;

                default:
                    return SSerilog.Events.LogEventLevel.Warning;
            }
        }

        private const string SourceContextPropName = "SourceContext";
    }

    public static class LogHelperFactoryExtensions
    {
        public static ILogHelperLoggingBuilder AddSerilog([NotNull]this ILogHelperLoggingBuilder loggingBuilder, Action<LoggerConfiguration> loggerConfigurationAction)
        {
            loggingBuilder.AddProvider(new SerilogLogHelperProvider(loggerConfigurationAction));
            return loggingBuilder;
        }

        public static ILogHelperLoggingBuilder AddSerilog([NotNull] this ILogHelperLoggingBuilder loggingBuilder,
            LoggerConfiguration loggerConfiguration)
        {
            loggingBuilder.AddProvider(new SerilogLogHelperProvider(loggerConfiguration));
            return loggingBuilder;
        }
    }
}
