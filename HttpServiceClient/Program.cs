using System.Net;
using System.Net.Sockets;
using System.Text;

namespace HttpServiceClient
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            await Send();
        }

        private static async Task Send()
        {
            var ipHostInfo = await Dns.GetHostEntryAsync(Dns.GetHostName());
            var ipAddress = ipHostInfo.AddressList[0];
            IPEndPoint ipEndPoint = new(ipAddress, 11_000);

            using Socket client = new(
                ipEndPoint.AddressFamily,
                SocketType.Stream,
                ProtocolType.Tcp);

            await client.ConnectAsync(ipEndPoint);
            while (true)
            {
                // Send message.
                const string message = "Hi friends 👋!<|EOM|>";
                var messageBytes = Encoding.UTF8.GetBytes(message);
                _ = await client.SendAsync(messageBytes, SocketFlags.None);
                Console.WriteLine($"Socket client sent message: \"{message}\"");

                // Receive ack.
                var buffer = new byte[1_024];
                var received = await client.ReceiveAsync(buffer, SocketFlags.None);
                var response = Encoding.UTF8.GetString(buffer, 0, received);
                if (response != "<|ACK|>")
                {
                    continue;
                }

                Console.WriteLine(
                    $"Socket client received acknowledgment: \"{response}\"");
                break;
            }

            client.Shutdown(SocketShutdown.Both);
        }
    }
}