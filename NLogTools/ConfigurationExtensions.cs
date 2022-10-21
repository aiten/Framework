/*
  This file is part of  https://github.com/aiten/Framework.

  Copyright (c) Herbert Aitenbichler

  Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), 
  to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, 
  and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

  The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, 
  WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE. 
*/

namespace Framework.NLogTools;

using System;
using System.IO;
using System.Reflection;

using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;

using NLog;
using NLog.Web;

public static class ConfigurationExtensions
{
    private static string BaseDirectory => Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

    public static IWebHostBuilder ConfigureAndUseNLog(this IWebHostBuilder hostBuilder)
    {
        return
            hostBuilder
                .ConfigureLogging(logging =>
                {
                    logging.ClearProviders();
                    logging.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Trace);
                })
                .UseNLog();
    }

    public static Logger ConfigureNLogLocation(string appName, Assembly assembly)
    {
        var logDir = Microsoft.Azure.Web.DataProtection.Util.IsAzureEnvironment()
            ? $"{BaseDirectory}/data/logs"
            : $"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}/{appName}/logs";

        if (!Directory.Exists(logDir))
        {
            Directory.CreateDirectory(logDir);
        }

        GlobalDiagnosticsContext.Set("logDir",      logDir);
        GlobalDiagnosticsContext.Set("version",     assembly.GetName().Version?.ToString());
        GlobalDiagnosticsContext.Set("application", appName);
        GlobalDiagnosticsContext.Set("username",    Environment.UserName);

        var logger = NLogBuilder.ConfigureNLog("nlog.config").GetCurrentClassLogger();

#if DEBUG
        LogManager.ThrowExceptions = true;
#endif

        return logger;
    }
}