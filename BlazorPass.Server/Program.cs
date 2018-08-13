using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Linq;

namespace BlazorPass.Server
{
    public class Program
    {
        public const string HostingSettingsOptional = "hosting.json";
        public const string HostingSettingsEnvPrefix = "HOSTING_";
        public const string HostingSettingsCliPrefix = "+h:";

        public const string AppSettingsRequired = "appsettings.json";
        public const string AppSettingsOptional = "appsettings.local.json";
        public const string AppSettingsEnvPrefix = "BLZRPASS_";

        public static IConfiguration HostingConfig { get; set;  }

        public static IReadOnlyDictionary<string, string> BaseAppSettings { get; set; }

        public static void Main(string[] args)
        {
            var hostingArgs = args.Where(x => x.StartsWith(HostingSettingsCliPrefix));
            var notHostingArgs = args.Where(x => !x.StartsWith(HostingSettingsCliPrefix));

            HostingConfig = new ConfigurationBuilder()
                .AddJsonFile(HostingSettingsOptional, optional: false)
                .AddEnvironmentVariables(HostingSettingsEnvPrefix)
                .AddCommandLine(hostingArgs.ToArray())
                .Build();

            BuildWebHost(notHostingArgs.ToArray()).Run();
        }

        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseConfiguration(HostingConfig)
                .ConfigureAppConfiguration(builder =>
                {
                    builder
                        .AddInMemoryCollection(BaseAppSettings)
                        .AddJsonFile(AppSettingsRequired, optional: false)
                        .AddJsonFile(AppSettingsOptional, optional: true)
                        .AddEnvironmentVariables(AppSettingsEnvPrefix)
                        .AddCommandLine(args);
                })
                .UseStartup<Startup>()
                .Build();
    }
}
