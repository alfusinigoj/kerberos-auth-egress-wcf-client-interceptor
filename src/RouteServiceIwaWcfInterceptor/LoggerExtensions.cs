using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Configuration;
using System;
using System.Collections.Generic;

namespace Pivotal.RouteServiceIwaWcfInterceptor
{
    internal static class LoggerExtensions
    {
        static ILoggerFactory loggerFactory;
        readonly static Dictionary<string, ILogger> loggers = new Dictionary<string, ILogger>();

        static LoggerExtensions()
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddLogging(builder => builder.AddConsole());
            serviceCollection.AddLogging((builder) =>
            {
                builder.AddConfiguration(new ConfigurationBuilder().AddEnvironmentVariables().Build());
                builder.AddConsole();
            });
            var serviceProvider = serviceCollection.BuildServiceProvider();
            loggerFactory = serviceProvider.GetService<ILoggerFactory>();
        }

        public static ILogger Logger(this Type type)
        {
            var callerName = type.FullName;

            if (loggers.TryGetValue(callerName, out ILogger logger))
                return logger;

            return loggers[callerName] = loggerFactory.CreateLogger(callerName);
        }

        public static ILogger Logger(this object instance)
        {
            return Logger(instance.GetType());
        }
    }
}
