using System.Net;
using System.Net.Sockets;
using System.Text;
using Microsoft.Extensions.Logging;

namespace HttpServiceServer.Listener
{
    internal class ListenerService : IListenerService
    {
        private readonly ILogger<ListenerService> _logger;

        public ListenerService(ILogger<ListenerService> logger)
        {
            _logger = logger;
        }

        private static async Task<IPAddress> GetIpAddress(string hostName)
        {
            var host = await Dns.GetHostEntryAsync(hostName).ConfigureAwait(false);
            return host.AddressList.First(address => address.AddressFamily == AddressFamily.InterNetwork) ??
                   throw new Exception("No network adapters with an IPv4 address in the system!");
        }

        public async Task<IEnumerable<string>> Listen(ListenerServiceConfig serviceConfig)
        {
            _logger.LogInformation("Service {service} has started.", nameof(ListenerService));
            _logger.LogInformation("Listening to host name = {hostName} and port = {port}...", serviceConfig.HostName,
                serviceConfig.Port);

            var ipAddress = await GetIpAddress(serviceConfig.HostName);
            var ipEndPoint = new IPEndPoint(ipAddress, serviceConfig.Port);

            using Socket listener = new(
                ipEndPoint.AddressFamily,
                SocketType.Stream,
                ProtocolType.Tcp);

            listener.Bind(ipEndPoint);
            listener.Listen(serviceConfig.Backlog);

            var receivedMessages = new List<string>();
            var handler = await listener.AcceptAsync().ConfigureAwait(false);

            while (true)
            {
                var buffer = new byte[1_024];
                var bytesReceived = await handler.ReceiveAsync(buffer, SocketFlags.None).ConfigureAwait(false);
                var receivedMessage = Encoding.UTF8.GetString(buffer, 0, bytesReceived);
                receivedMessages.Add(receivedMessage);

                var indexPagePath = FilePath.IndexPagePath;
                var indexPageContent = await File.ReadAllTextAsync(indexPagePath).ConfigureAwait(false);
                var indexPageContentBytes = Encoding.UTF8.GetBytes(indexPageContent);

                const string statusLine = "HTTP/1.1 200 OK\r\n";
                const string responseHeader = "Content-Type: text/html\r\n";

                await handler.SendAsync(Encoding.UTF8.GetBytes(statusLine));
                await handler.SendAsync(Encoding.UTF8.GetBytes(responseHeader));
                await handler.SendAsync("\r\n"u8.ToArray());
                await handler.SendAsync(indexPageContentBytes);
                await handler.SendAsync("\r\n"u8.ToArray());

                handler.Close();
                break;
            }


            _logger.LogInformation("Service {service} has finished.", nameof(ListenerService));
            return receivedMessages;
        }
    }
}
