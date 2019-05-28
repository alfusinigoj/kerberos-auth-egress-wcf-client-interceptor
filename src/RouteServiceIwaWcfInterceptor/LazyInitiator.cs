using Microsoft.Extensions.Logging;
using System;

namespace Pivotal.RouteServiceIwaWcfInterceptor
{
    static class LazyInitiator
    {
        const string KBBS_CONFIG_FILE_LOCATION_ENV_NM = "KRB5_CONFIG";
        const string KBBS_CONFIG_FILE_LOCATION_DEFAULT = @"C:\Users\vcap\app\krb5.ini";

        static LazyInitiator()
        {
            var kbbsConfigLocation = Environment.GetEnvironmentVariable(KBBS_CONFIG_FILE_LOCATION_ENV_NM);

            if (string.IsNullOrWhiteSpace(kbbsConfigLocation))
                Environment.SetEnvironmentVariable(KBBS_CONFIG_FILE_LOCATION_ENV_NM, KBBS_CONFIG_FILE_LOCATION_DEFAULT);

            LoggerExtensions.Logger(typeof(LazyInitiator)).LogDebug($"Using kbbs config file location '{kbbsConfigLocation}, can be overriden by setting  environment variable '{KBBS_CONFIG_FILE_LOCATION_ENV_NM}'");
        }
    }
}
