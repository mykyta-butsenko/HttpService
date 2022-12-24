using System.Net.Sockets;
using System.Threading.Channels;

namespace HttpServiceServer.MessageQueue
{
    internal class MessageQueue : IMessageQueue
    {
        private readonly Channel<(Socket, string)> _messageQueue;

        public MessageQueue(int capacity)
        {
            BoundedChannelOptions options = new(capacity)
            {
                FullMode = BoundedChannelFullMode.Wait
            };
            _messageQueue = Channel.CreateBounded<(Socket, string)>(options);
        }

        public async ValueTask QueueMessageAsync((Socket handler, string receivedMessage) workItem)
        {
            if (workItem.handler is null || workItem.receivedMessage is null)
            {
                throw new ArgumentNullException(nameof(workItem));
            }

            await _messageQueue.Writer.WriteAsync(workItem).ConfigureAwait(false);
        }

        public IAsyncEnumerable<(Socket handler, string receivedMessage)> DequeueAllMessagesAsync(
            CancellationToken cancellationToken)
        {
            return _messageQueue.Reader.ReadAllAsync(cancellationToken);
        }
    }
}
