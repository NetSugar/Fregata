using Fregata.Exceptions;
using Microsoft.Extensions.Logging;
using System;

namespace Fregata.Utils
{
    public static class Log<T>
    {
        private static readonly ILogger _logger;

        static Log()
        {
            _logger = IocProvider.GetService<ILogger<T>>();
        }

        public static void Trace(string msg)
        {
            if (_logger.IsEnabled(LogLevel.Trace))
            {
                _logger.LogTrace(msg);
            }
        }

        public static void Debug(string msg)
        {
            if (_logger.IsEnabled(LogLevel.Debug))
                _logger.LogDebug(msg);
        }

        public static void Info(string msg)
        {
            if (_logger.IsEnabled(LogLevel.Information))
                _logger.LogInformation(msg);
        }

        public static void Warn(string msg)
        {
            if (_logger.IsEnabled(LogLevel.Warning))
                _logger.LogWarning(msg);
        }

        public static void Warn(Exception ex, string desc = null)
        {
            if (_logger.IsEnabled(LogLevel.Warning))
                _logger.LogWarning(ex.ToErrMsg(desc: desc));
        }

        public static void Error(string msg)
        {
            if (_logger.IsEnabled(LogLevel.Error))
                _logger.LogError(msg);
        }

        public static void Error(Exception ex, string desc = null)
        {
            if (_logger.IsEnabled(LogLevel.Error))
                _logger.LogError(ex.ToErrMsg(desc: desc));
        }
    }

    public class Log
    {
        private static readonly ILogger _logger;

        static Log()
        {
            _logger = IocProvider.GetService<ILogger<Log>>();
        }

        public static void Trace(string msg)
        {
            if (_logger.IsEnabled(LogLevel.Trace))
            {
                _logger.LogTrace(msg);
            }
        }

        public static void Debug(string msg)
        {
            if (_logger.IsEnabled(LogLevel.Debug))
                _logger.LogDebug(msg);
        }

        public static void Info(string msg)
        {
            if (_logger.IsEnabled(LogLevel.Information))
                _logger.LogInformation(msg);
        }

        public static void Warn(string msg)
        {
            if (_logger.IsEnabled(LogLevel.Warning))
                _logger.LogWarning(msg);
        }

        public static void Warn(Exception ex, string memberName = null)
        {
            if (_logger.IsEnabled(LogLevel.Warning))
                _logger.LogWarning(ex.ToErrMsg(desc: memberName));
        }

        public static void Error(string msg)
        {
            if (_logger.IsEnabled(LogLevel.Error))
                _logger.LogError(msg);
        }

        public static void Error(Exception ex, string desc = null)
        {
            if (_logger.IsEnabled(LogLevel.Error))
                _logger.LogError(ex.ToErrMsg(desc: desc));
        }
    }
}