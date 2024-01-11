using System;
using Core.Models;
using Serilog;

namespace Core.Helpers
{
    public static class LogHelper
    {
        public static readonly string LogTemplate =
            $"{"{" + nameof(LogDetail.Protocol) + "}"} {"{" + nameof(LogDetail.Method) + "}"} {"{" + nameof(LogDetail.Path) + "}"} responded {"{" + nameof(LogDetail.StatusCode) + "}"} in {"{" + nameof(LogDetail.ElapsedMilliseconds) + "}"} ms";

        public static ILogger GetPreparedLogger(ILogger logger, LogDetail logDetail)
        {
            if (logDetail == null)
            {
                return logger;
            }

            logger = logger
                .ForContext(nameof(logDetail.Host), logDetail.Host)
                .ForContext(nameof(logDetail.Protocol), logDetail.Protocol)
                .ForContext(nameof(logDetail.Method), logDetail.Method)
                .ForContext(nameof(logDetail.Path), logDetail.Path)
                .ForContext(nameof(logDetail.PathAndQuery), logDetail.PathAndQuery)
                .ForContext(nameof(logDetail.StatusCode), logDetail.StatusCode)
                .ForContext(nameof(logDetail.ElapsedMilliseconds), logDetail.ElapsedMilliseconds)
                .ForContext(nameof(logDetail.Headers), logDetail.Headers)
                .ForContext(nameof(logDetail.Culture), logDetail.Culture)
                .ForContext(nameof(logDetail.Ip), logDetail.Ip)
                .ForContext(nameof(logDetail.MachineName), logDetail.MachineName);

            if (!string.IsNullOrEmpty(logDetail.Body))
            {
                logger = logger.ForContext(nameof(logDetail.Body), logDetail.Body);
            }

            if (logDetail.UserId != Guid.Empty)
            {
                logger = logger.ForContext(nameof(logDetail.UserId), logDetail.UserId);
            }

            if (logDetail.Exception != null)
            {
                logger = logger
                    .ForContext(nameof(logDetail.Exception), logDetail.Exception)
                    .ForContext(nameof(logDetail.ExceptionType), logDetail.ExceptionType);
            }

            if (logDetail.Errors != null && (logDetail.Errors != null || logDetail.Errors.Count > 0))
            {
                logger = logger.ForContext(nameof(logDetail.Errors), logDetail.Errors);
            }

            return logger;
        }
    }
}