using System.Reflection;
using System.Threading.RateLimiting;
using Core.Helpers;
using Core.Models;
using Core.Utils;
using Data;
using Data.Utils;
using Service;

namespace Api
{
    public class Startup(IConfiguration configuration)
    {
        private CoreSettings _coreSettings;
        private IConfiguration Configuration { get; } = configuration;

        public void ConfigureServices(IServiceCollection services)
        {
            #region Settings

            var coreConfiguration = Configuration.GetSection("CoreSettings");

            services.Configure<CoreSettings>(coreConfiguration);

            _coreSettings = coreConfiguration.Get<CoreSettings>();

            #endregion

            #region Databases

            services.AddSingleton<IDbHelper>(s =>
                new DbHelper(Configuration.GetConnectionString("DbConnection"),
                    Configuration.GetConnectionString("DbReadOnlyConnection")));

            #endregion

            #region Dependency

            services.AddSingleton<ITokenHelper, TokenHelper>();

            #endregion
     
            services.AddRateLimiter(options =>
            {
                options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
                
                options.AddPolicy("LOGIN_LIMITER", httpContext =>
                    RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey: httpContext.Connection.RemoteIpAddress?.ToString(),
                        factory: _ => new FixedWindowRateLimiterOptions()
                        {
                            PermitLimit = 3,
                            Window = TimeSpan.FromSeconds(10)
                        }));
           
            });
            #region Core Configuration

            services.ConfigureServices(_coreSettings, GetAssemblies());

            #endregion
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            #region Core Configuration

            
            app.ConfigureApplication(env, _coreSettings);

            #endregion
        }

        private static IEnumerable<Assembly> GetAssemblies()
        {
            yield return typeof(Startup).GetTypeInfo().Assembly;
            yield return typeof(DataStartup).GetTypeInfo().Assembly;
            yield return typeof(ServiceStartup).GetTypeInfo().Assembly;
        }
    }
}