using System.Net.Sockets;
using System.Text;
using HttpServiceServer.Queue;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace HttpServiceServer.Listener
{
    internal class ListenerService : IListenerService
    {
        private readonly ILogger<ListenerService> _logger;
        private readonly Socket _listener;
        private readonly IProcessMessageTaskQueue _messageQueue;
        private readonly CancellationToken _cancellationToken;

        public ListenerService(ILogger<ListenerService> logger, Socket listener, IProcessMessageTaskQueue messageQueue, IHostApplicationLifetime applicationLifetime)
        {
            _logger = logger;
            _listener = listener;
            _messageQueue = messageQueue;
            _cancellationToken = applicationLifetime.ApplicationStopping;
        }

        public void StartListening()
        {
            _logger.LogInformation($"{nameof(ListenerService)} loop is starting.");
            Task.Run(async () => await Listen().ConfigureAwait(false), _cancellationToken);
        }

        private async ValueTask Listen()
        {
            while (!_cancellationToken.IsCancellationRequested)
            {
                var handler = await _listener.AcceptAsync(_cancellationToken).ConfigureAwait(false);
                var buffer = new byte[1_024];
                var bytesReceived = await handler.ReceiveAsync(buffer, SocketFlags.None).ConfigureAwait(false);
                var receivedMessage = Encoding.UTF8.GetString(buffer, 0, bytesReceived);
                _logger.LogInformation("Received a message = {message}", receivedMessage);

                await _messageQueue.QueueMessageAsync((handler, receivedMessage)).ConfigureAwait(false);
            }
        }
    }
}
