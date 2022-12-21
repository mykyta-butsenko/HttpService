using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using HttpServiceServer.Listener;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace HttpServiceServer
{
    internal class Program
    {
        private static ListenerServiceConfig GetListenerServiceConfig()
        {
            var configurationBuilder = new ConfigurationBuilder().AddJsonFile("serviceConfig.json");
            var configurationRoot = configurationBuilder.Build();
            var listenerServiceConfig = configurationRoot.GetRequiredSection("Settings").Get<ListenerServiceConfig>();
            return listenerServiceConfig ??
                   throw new ArgumentNullException(nameof(listenerServiceConfig), "Parameter cannot be null!");
        }

        private static IServiceCollection ConfigureServices()
        {
            IServiceCollection services = new ServiceCollection();
            services.AddLogging(loggingBuilder => loggingBuilder.AddConsole());
            services.AddScoped<IListenerService, ListenerService>();
            return services;
        }

        static async Task Main()
        {
            var listenerServiceConfig = GetListenerServiceConfig();

            var services = ConfigureServices();
            var serviceProvider = services.BuildServiceProvider();

            var listenerService = serviceProvider.GetRequiredService<IListenerService>();
            await listenerService.Listen(listenerServiceConfig).ConfigureAwait(false);
        }
    }
}