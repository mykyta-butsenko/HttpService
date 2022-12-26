using HttpServiceServer.SocketWrappers;
using System.Threading.Channels;

namespace HttpServiceServer.MessageQueue
{
    internal class MessageQueue : IMessageQueue
    {
        private readonly Channel<(ISocket, string)> _messageQueue;

        public MessageQueue(int capacity)
        {
            BoundedChannelOptions options = new(capacity)
            {
                FullMode = BoundedChannelFullMode.Wait
            };
            _messageQueue = Channel.CreateBounded<(ISocket, string)>(options);
        }

        public async Task QueueMessageAsync((ISocket handler, string receivedMessage) workItem)
        {
            if (workItem.handler is null || workItem.receivedMessage is null)
            {
                throw new ArgumentNullException(nameof(workItem));
            }

            await _messageQueue.Writer.WriteAsync(workItem).ConfigureAwait(false);
        }

        public IAsyncEnumerable<(ISocket handler, string receivedMessage)> DequeueAllMessagesAsync(
            CancellationToken cancellationToken)
        {
            return _messageQueue.Reader.ReadAllAsync(cancellationToken);
        }
    }
}
