using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Core.Context;
using Core.Dtos;
using Core.Enums;
using Core.Helpers;
using Core.Models;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Serilog;

namespace Core.Middlewares
{
    public class ExceptionMiddleware(RequestDelegate next, ICoreAppContext coreAppContext)
    {
        private Stopwatch _sw;
        private static readonly ILogger Logger = Log.ForContext<ExceptionMiddleware>();

        public async Task Invoke(HttpContext context)
        {
            try
            {
                _sw = Stopwatch.StartNew();

                await next(context);

                _sw.Stop();
            }
            catch (ValidationException exception)
            {
                var errors = new Dictionary<string, string>();

                if (exception.Errors.Any())
                {
                    foreach (var error in exception.Errors)
                    {
                        if (errors.ContainsKey(error.PropertyName)) continue;

                        errors.Add(error.PropertyName, error.ErrorMessage);
                    }
                }

                await WriteLog(context, exception, HttpStatusCode.BadRequest, errors);
                await BuildExceptionResponse(context, new ApiErrorDto("request_model_is_invalid", ExceptionType.Validation), HttpStatusCode.BadRequest);
            }
            catch (CustomException exception)
            {
                await WriteLog(context, exception, exception.StatusCode);
                await BuildExceptionResponse(context, new ApiErrorDto(exception.Message, ExceptionType.Info), exception.StatusCode);
            }
            catch (Exception exception)
            {
                await WriteLog(context, exception, HttpStatusCode.InternalServerError);
                await BuildExceptionResponse(context, new ApiErrorDto("internal_server_error", ExceptionType.Undefined), HttpStatusCode.InternalServerError);
            }
        }

        private async Task WriteLog(HttpContext context, Exception exception, HttpStatusCode statusCode, Dictionary<string, string> errors = null)
        {
            var request = context.Request;

            var logDto = new LogDetail
            {
                Host = request.Host.Host,
                Protocol = request.Protocol,
                Method = request.Method,
                Path = request.Path,
                PathAndQuery = request.GetEncodedPathAndQuery(),
                StatusCode = (int) statusCode,
                ElapsedMilliseconds = _sw?.ElapsedMilliseconds ?? 0,
                Headers = coreAppContext.Headers(),
                Culture = coreAppContext.Culture(),
                MachineName = Environment.MachineName,
                Exception = exception,
                ExceptionType = exception.GetType().ToString(),
                Errors = errors,
                Ip = coreAppContext.Ip()
            };

            if (coreAppContext.HasToken())
            {
                logDto.IsAuthenticated = coreAppContext.IsAuthenticated();
                logDto.UserId = coreAppContext.UserId();
                logDto.SessionId = coreAppContext.SessionId();
            }

            LogHelper.GetPreparedLogger(Logger, logDto).Error(LogHelper.LogTemplate);

            await Task.FromResult(true);
        }

        private static Task BuildExceptionResponse(HttpContext context, ApiErrorDto errorDto, HttpStatusCode statusCode)
        {
            errorDto.Version = ActionResultHelper.Version;

            var response = JsonSerializer.Serialize(errorDto);

            context.Response.Clear();
            context.Response.StatusCode = (int) statusCode;
            context.Response.ContentType = "application/json";
            context.Response.Headers.Append("Access-Control-Allow-Origin", "*");

            return context.Response.WriteAsync(response, Encoding.UTF8);
        }
    }
}