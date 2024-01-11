using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Core.Context;
using Core.Helpers;
using Core.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Options;
using Serilog;

namespace Core.Middlewares
{
    public class LoggingMiddleware(RequestDelegate next, IOptions<CoreSettings> options, ICoreAppContext coreAppContext)
    {
        private readonly CoreSettings _coreSettings = options.Value;

        private Stopwatch _sw;

        private static readonly ILogger Logger = Log.ForContext<LoggingMiddleware>();


        public async Task Invoke(HttpContext context)
        {
            _sw = Stopwatch.StartNew();

            await next(context);

            _sw.Stop();

            if (_coreSettings.Seq.IsActive && !context.Request.Path.ToString()
                    .Contains("/swagger", StringComparison.OrdinalIgnoreCase))
            {
                await WriteLog(context);
            }
        }

        private async Task WriteLog(HttpContext context)
        {
            var request = context.Request;

            var logDto = new LogDetail
            {
                Host = request.Host.Host,
                Protocol = request.Protocol,
                Method = request.Method,
                Path = request.Path,
                PathAndQuery = request.GetEncodedPathAndQuery(),
                StatusCode = context.Response.StatusCode,
                ElapsedMilliseconds = _sw?.ElapsedMilliseconds ?? 0,
                Headers = coreAppContext.Headers(),
                Culture = coreAppContext.Culture(),
                MachineName = Environment.MachineName,
                Ip = coreAppContext.Ip()
            };

            if (coreAppContext.HasToken())
            {
                logDto.IsAuthenticated = coreAppContext.IsAuthenticated();
                logDto.UserId = coreAppContext.UserId();
                logDto.SessionId = coreAppContext.SessionId();
            }

            LogHelper.GetPreparedLogger(Logger, logDto).Information(LogHelper.LogTemplate);

            await Task.FromResult(true);
        }
    }
}