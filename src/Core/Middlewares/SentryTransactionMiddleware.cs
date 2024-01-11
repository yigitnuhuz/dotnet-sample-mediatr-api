using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Core.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Sentry;

namespace Core.Middlewares
{
    public class SentryTransactionMiddleware(RequestDelegate next, IOptions<CoreSettings> options)
    {
        private readonly CoreSettings _coreSettings = options.Value;

        private static readonly Regex GuidPattern =
            new Regex(@"\b[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}\b",
                RegexOptions.Compiled);

        public async Task InvokeAsync(HttpContext context)
        {
            if (_coreSettings.Sentry.IsActive)
            {
                var transactionName = $"{context.Request.Method}-{RemoveGuidFromUrl(context.Request.Path)}".ToLower();
                var operationName = context.Request.Path.HasValue
                    ? context.Request.Path.ToString()?.ToLower()
                    : "operation";

                var transaction = SentrySdk.StartTransaction(transactionName, operationName);

                SentrySdk.ConfigureScope(scope => scope.Transaction = transaction);

                try
                {
                    await next(context);
                }
                catch (Exception ex)
                {
                    SentrySdk.CaptureException(ex);
                    throw;
                }
                finally
                {
                    transaction.Finish();
                }
            }
            else
            {
                await next(context);
            }
        }

        private static string RemoveGuidFromUrl(string url)
        {
            //removes GUID from the URL
            string modifiedUrl = GuidPattern.Replace(url, "");

            // removes extra "/" char
            modifiedUrl = modifiedUrl.Replace("//", "/");

            // removes "/" char at the end of the url
            modifiedUrl = modifiedUrl.TrimEnd('/');

            return modifiedUrl;
        }
    }
}