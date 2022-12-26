using System.Text;
using HttpServiceServer.MessageQueue;
using HttpServiceServer.SocketWrappers;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace HttpServiceServer.MessageListener
{
    internal class ListenerService : IListenerService
    {
        private readonly ILogger<ListenerService> _logger;
        private readonly IMessageQueue _messageQueue;
        private readonly CancellationToken _cancellationToken;

        public ListenerService(ILogger<ListenerService> logger, IMessageQueue messageQueue,
            IHostApplicationLifetime applicationLifetime)
        {
            _logger = logger;
            _messageQueue = messageQueue;
            _cancellationToken = applicationLifetime.ApplicationStopping;
        }

        public void StartListening(ISocket listener)
        {
            _logger.LogInformation($"{nameof(ListenerService)} is starting.");
            Task.Run(async () => await Listen(listener).ConfigureAwait(false), _cancellationToken);
        }

        private async Task Listen(ISocket listener)
        {
            while (!_cancellationToken.IsCancellationRequested)
            {
                var handler = await listener.AcceptAsync(_cancellationToken).ConfigureAwait(false);
                var buffer = new byte[1_024];
                var bytesReceived = await handler.ReceiveAsync(buffer).ConfigureAwait(false);
                var receivedMessage = Encoding.UTF8.GetString(buffer, 0, bytesReceived);
                _logger.LogInformation("Received a message = {message}", receivedMessage);

                await _messageQueue.QueueMessageAsync((handler, receivedMessage)).ConfigureAwait(false);
            }
        }
    }
}
