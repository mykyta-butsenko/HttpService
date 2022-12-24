using System.Net.Sockets;
using System.Threading.Channels;

namespace HttpServiceServer.Queue
{
    internal class ProcessMessageTaskQueue : IProcessMessageTaskQueue
    {
        private readonly Channel<(Socket, string)> _messageQueue;

        public ProcessMessageTaskQueue(int capacity)
        {
            BoundedChannelOptions options = new(capacity)
            {
                FullMode = BoundedChannelFullMode.Wait
            };
            _messageQueue = Channel.CreateBounded<(Socket, string)>(options);
            //_messageQueue = Channel.CreateUnbounded<(Socket, string)>();
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
