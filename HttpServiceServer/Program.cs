using System.Net;
using System.Net.Sockets;
using HttpServiceServer.MessageListener;
using HttpServiceServer.MessageProcessing;
using HttpServiceServer.MessageQueue;
using HttpServiceServer.SocketWrappers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Hosting.Internal;
using Microsoft.Extensions.Logging;

namespace HttpServiceServer
{
    internal class Program
    {
        private static ListenerServiceConfig CreateListenerServiceConfig()
        {
            var configurationBuilder = new ConfigurationBuilder().AddJsonFile("serviceConfig.json");
            var configurationRoot = configurationBuilder.Build();
            var listenerServiceConfig = configurationRoot.GetRequiredSection("Settings").Get<ListenerServiceConfig>();
            return listenerServiceConfig ??
                   throw new ArgumentNullException(nameof(listenerServiceConfig), "Parameter cannot be null!");
        }

        private static async Task<ISocket> CreateListener(ListenerServiceConfig serviceConfig)
        {
            var ipAddress = await GetIpAddress(serviceConfig.HostName).ConfigureAwait(false);
            var ipEndPoint = new IPEndPoint(ipAddress, serviceConfig.Port);
            var listener = new TcpSocket(ipEndPoint.AddressFamily);
            listener.Bind(ipEndPoint);
            listener.Listen(serviceConfig.Backlog);
            return listener;
        }

        private static async Task<IPAddress> GetIpAddress(string hostName)
        {
            var host = await Dns.GetHostEntryAsync(hostName).ConfigureAwait(false);
            return host.AddressList.First(address => address.AddressFamily == AddressFamily.InterNetwork) ??
                   throw new Exception("No network adapters with an IPv4 address in the system!");
        }

        static async Task Main()
        {
            var config = CreateListenerServiceConfig();
            var hostBuilder = new HostBuilder()
                .ConfigureServices(services =>
                {
                    services.AddLogging(loggingBuilder => loggingBuilder.AddConsole());
                    services.AddSingleton<IHostApplicationLifetime, ApplicationLifetime>();
                    services.AddSingleton<IMessageQueue>(_ => new MessageQueue.MessageQueue(config.Backlog));
                    services.AddSingleton<IListenerService, ListenerService>();
                    services.AddSingleton<IMessageProcessingService, MessageProcessingService>();
                    services.AddHostedService<MessageQueueProcessingService>();
                }).Build();

            var logger = hostBuilder.Services.GetRequiredService<ILogger<Program>>();
            logger.LogInformation(
                "Starting {listeningService} and {processingService} service. " +
                "The first service will accept incoming requests and write them to a queue, " +
                "which will be processed by the second service in the background.",
                nameof(ListenerService), nameof(MessageQueueProcessingService));

            var listenerService = hostBuilder.Services.GetRequiredService<IListenerService>();
            var listener = await CreateListener(config).ConfigureAwait(false);
            try
            {
                // Run Listening of incoming requests in a background thread. These requests get written to a queue...
                listenerService.StartListening(listener);
                // ...which gets processed by our MessageQueueProcessingService.
                await hostBuilder.RunAsync().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred");
            }
            finally
            {
                logger.LogInformation("Program has finished.");
            }

            Console.CancelKeyPress += (_, _) => listener.Close();
        }
    }
}