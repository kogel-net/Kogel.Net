using Kogel.Net.Test.Command;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;

namespace Kogel.Net.Test
{
    class Program
    {
        static void Main(string[] args)
        {
            BuilHost();
            var provider = Services.BuildServiceProvider();
            ICommand command = provider.GetService<WebSocketServerCommand>();
            command.Start();
        }

        private static IServiceCollection Services;
        static void BuilHost()
        {
            new HostBuilder()
              .ConfigureAppConfiguration((hostingContext, configBuilder) =>
              {
              })
              .ConfigureServices((hostContext, services) =>
              {
                  services.AddKogelHttpClient();
                  services.AddTransient<HttpClientCommand>();
                  services.AddTransient<WebSocketServerCommand>();
                  services.AddTransient<WebSocketClientCommand>();
                  Services = services;
              })
              .UseConsoleLifetime()
              .Build();
        }
    }
}
