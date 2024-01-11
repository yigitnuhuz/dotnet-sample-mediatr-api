using Core.Models;
using Serilog;
using Serilog.Events;

namespace Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var settings = GetConfigurationRoot(args).GetSection("CoreSettings").Get<CoreSettings>();

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Override("System", LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .Enrich.FromLogContext()
                .Enrich.WithProperty("Application",
                    $"{settings.Auth.System.ToLower().Replace(" ", "-")}-{settings.Name.ToLower().Replace(" ", "-")}")
                .Enrich.WithProperty("Environment", Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"))
                .WriteTo.Seq(settings.Seq.SeqServerUrl)
                .WriteTo.Console()
                .CreateLogger();

            try
            {
                Log.Information($"{settings.Name} starting..");
                CreateHostBuilder(args).Build().Run();
            }
            catch (Exception exception)
            {
                Log.Fatal(exception, $"{settings.Name} terminated unexpectedly..");
            }
            finally
            {
                Log.Warning($"{settings.Name} stopping..");
                Log.CloseAndFlush();
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    config.AddJsonFile($"appsettings.{hostingContext.HostingEnvironment.EnvironmentName}.json",
                        false, true).AddEnvironmentVariables();
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder
                        .UseKestrel(o => o.AllowSynchronousIO = true)
                        .UseStartup<Startup>();
                })
                .UseSerilog();

        private static IConfigurationRoot GetConfigurationRoot(string[] args)
        {
            return new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.json", false,
                    true)
                .AddCommandLine(args)
                .Build();
        }
    }
}