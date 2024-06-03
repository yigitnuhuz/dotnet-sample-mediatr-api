using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.RateLimiting;
using Asp.Versioning;
using Core.Behaviours;
using Core.Context;
using Core.Helpers;
using Core.Middlewares;
using Core.Models;
using Core.Providers;
using Core.Swagger;
using Datadog.Trace;
using Datadog.Trace.Configuration;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Sentry;

namespace Core.Utils
{
    public static class CoreApiBuilder
    {
        public static void ConfigureServices(this IServiceCollection services, CoreSettings coreSettings,
            IEnumerable<Assembly> assemblies)
        {
            #region Common

            services.AddControllers();

            services.AddHttpContextAccessor();

            services.AddMemoryCache();

            services.AddLocalization();

            var assemblyList = assemblies.ToList();

            #endregion

            #region Versioning

            ActionResultHelper.Version = coreSettings.Version;

            services.AddApiVersioning(o =>
            {
                o.ReportApiVersions = true;
                o.AssumeDefaultVersionWhenUnspecified = true;
                o.DefaultApiVersion = new ApiVersion(1, 0);
            });

            #endregion

            #region Authentication

            services.AddAuthentication(o =>
                {
                    o.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                    o.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    o.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(o =>
                {
                    o.SaveToken = true;
                    o.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey =
                            new SymmetricSecurityKey(Encoding.ASCII.GetBytes(coreSettings.Auth.JwtSecret)),
                        ValidateIssuer = false,
                        ValidateAudience = false,
                        RequireExpirationTime = true,
                        ValidateLifetime = true
                    };
                });

            #endregion

            #region Authorization

            services.AddAuthorization(options =>
            {
                options.AddPolicy(AuthorizationPolicies.GuestPolicy,
                    policy => policy.RequireClaim("System", coreSettings.Auth.System));
                options.AddPolicy(AuthorizationPolicies.LoginPolicy,
                    policy => policy.RequireClaim("System", coreSettings.Auth.System)
                        .RequireClaim("IsAuthenticated", "True"));
            });

            #endregion

            #region Swagger

            services.AddSwaggerGen(o =>
            {
                 o.OperationFilter<HeaderOptionFilter>();

                o.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = $"{coreSettings.Name}", Version = "v1.0"
                });

                o.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
                {
                    Name = "Authorization",
                    Description = "JWT Authorization header using the Bearer scheme. Example: \"Bearer {token}\"",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey
                });

                o.AddSecurityRequirement(new OpenApiSecurityRequirement()
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            },
                            Name = "Bearer",
                            Scheme = SecuritySchemeType.ApiKey.ToString(),
                            In = ParameterLocation.Header,
                        },
                        new List<string>()
                    }
                });

                // o.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, "Api.xml"));
            });

            #endregion

            #region Redis

            services.AddStackExchangeRedisCache(o =>
            {
                o.InstanceName = coreSettings.Cache.RedisInstanceName;
                o.Configuration = coreSettings.Cache.RedisConfiguration;
            });

            #endregion

            #region Dependency

            services.AddSingleton<ICoreAppContext, CoreAppContext>();
            services.AddSingleton<ISerializerHelper, SerializerHelper>();

            #endregion

            #region Validation

            AssemblyScanner.FindValidatorsInAssemblies(assemblyList).ForEach(result =>
                services.AddScoped(result.InterfaceType, result.ValidatorType));

            #endregion

            #region Mediatr

            services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(assemblyList.ToArray()));

            #endregion
            
            //    
            // services.AddRateLimiter(options =>
            // {
            //     options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            //     
            //     options.AddPolicy("LOGIN_LIMITER", httpContext =>
            //         RateLimitPartition.GetFixedWindowLimiter(
            //             partitionKey: httpContext.Connection.RemoteIpAddress?.ToString(),
            //             factory: _ => new FixedWindowRateLimiterOptions()
            //             {
            //                 PermitLimit = 3,
            //                 Window = TimeSpan.FromSeconds(10)
            //             }));
            //
            // });

            #region Pipeline

            services.AddScoped(typeof(IPipelineBehavior<,>), typeof(CacheBehaviour<,>));
            services.AddScoped(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

            #endregion

            #region Sentry

            if (coreSettings.Sentry.IsActive)
            {
                SentrySdk.Init(options =>
                {
                    options.Dsn = coreSettings.Sentry.Dsn;
                    options.Debug = coreSettings.Sentry.Debug;
                    options.AutoSessionTracking = true;
                    options.EnableTracing = true;
                });
            }

            #endregion
        }

        public static void ConfigureApplication(this IApplicationBuilder app, IWebHostEnvironment env,
            CoreSettings coreSettings)
        {
            #region Middlewares

            app.UseMiddleware<SentryTransactionMiddleware>();
            app.UseMiddleware<ExceptionMiddleware>();
            app.UseMiddleware<LoggingMiddleware>();

            #endregion

            #region Localization

            var localizationOptions = new RequestLocalizationOptions()
            {
                DefaultRequestCulture = new RequestCulture(coreSettings.Localization.DefaultCulture),
                SupportedCultures =
                    coreSettings.Localization.SupportedCultures.Select(x => new CultureInfo(x)).ToList(),
                SupportedUICultures =
                    coreSettings.Localization.SupportedCultures.Select(x => new CultureInfo(x)).ToList()
            };

            localizationOptions.RequestCultureProviders.Clear();

            localizationOptions.RequestCultureProviders.Add(new CultureProvider(coreSettings.Localization));

            app.UseRequestLocalization(localizationOptions);

            #endregion

            #region Swagger

            if (!env.IsProduction())
            {
                app.UseSwagger();

                app.UseSwaggerUI(c => { c.SwaggerEndpoint("/swagger/v1/swagger.json", $"{coreSettings.Name} v1.0"); });
            }

            #endregion

            #region Datadog

            if (coreSettings.Datadog.IsActive)
            {
                var settings = TracerSettings.FromDefaultSources();
                settings.ServiceName = coreSettings.Datadog.ServiceName;
                settings.Environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
                settings.AnalyticsEnabled = true;
                settings.LogsInjectionEnabled = true;
                settings.TracerMetricsEnabled = true;
                var tracer = new Tracer(settings);
                Tracer.Instance = tracer;
            }

            #endregion

            #region Common

            app.UseRouting();
    
            app.UseRateLimiter();

            app.UseAuthentication();

            app.UseAuthorization();

            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });

            #endregion
        }
    }
}