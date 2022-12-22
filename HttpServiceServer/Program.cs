using System.Net;
using System.Net.Sockets;
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

        private static async Task<IServiceCollection> ConfigureServices(ListenerServiceConfig listenerServiceConfig)
        {
            IServiceCollection services = new ServiceCollection();
            services.AddLogging(loggingBuilder => loggingBuilder.AddConsole());

            var listener = await CreateListener(listenerServiceConfig).ConfigureAwait(false);
            services.AddScoped<IListenerService>(provider =>
                new ListenerService(provider.GetRequiredService<ILogger<ListenerService>>(), listener));
            return services;
        }

        private static async Task<IPAddress> GetIpAddress(string hostName)
        {
            var host = await Dns.GetHostEntryAsync(hostName).ConfigureAwait(false);
            return host.AddressList.First(address => address.AddressFamily == AddressFamily.InterNetwork) ??
                   throw new Exception("No network adapters with an IPv4 address in the system!");
        }

        private static async Task<Socket> CreateListener(ListenerServiceConfig serviceConfig)
        {
            var ipAddress = await GetIpAddress(serviceConfig.HostName).ConfigureAwait(false);
            var ipEndPoint = new IPEndPoint(ipAddress, serviceConfig.Port);

            Socket listener = new(ipEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            listener.Bind(ipEndPoint);
            listener.Listen(serviceConfig.Backlog);
            return listener;
        }

        static async Task Main()
        {
            var services = await ConfigureServices(GetListenerServiceConfig()).ConfigureAwait(false);
            var serviceProvider = services.BuildServiceProvider();

            var logger = serviceProvider.GetRequiredService<ILogger<Program>>();
            logger.LogInformation("Starting {service} service...", nameof(ListenerService));

            var listenerService = serviceProvider.GetRequiredService<IListenerService>();
            try
            {
                while (true)
                {
                    await listenerService.Listen().ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred");
            }
        }
    }
}