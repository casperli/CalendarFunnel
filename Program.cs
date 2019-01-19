using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CalendarFunnel
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = new WebHostBuilder()
                .UseKestrel()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseIISIntegration()
                .UseStartup<Startup>()
                .UseAzureAppServices()
                .ConfigureAppConfiguration(((context, builder) =>
                {
                    var env = context.HostingEnvironment;
                    builder.SetBasePath(env.ContentRootPath)
                        .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                        .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true);

                    builder.AddEnvironmentVariables();
                }))
                .ConfigureLogging((context, builder) =>
                {
                    context.Configuration.GetSection("Logging");
                    builder.AddConsole();
                    builder.AddDebug();
                })
                .Build();

            host.Run();
        }
    }
}