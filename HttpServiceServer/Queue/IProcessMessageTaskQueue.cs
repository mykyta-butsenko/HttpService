using System.Net.Sockets;

namespace HttpServiceServer.Queue
{
    internal interface IProcessMessageTaskQueue
    {
        ValueTask QueueMessageAsync((Socket handler, string receivedMessage) workItem);

        IAsyncEnumerable<(Socket handler, string receivedMessage)> DequeueAllMessagesAsync(CancellationToken cancellationToken);
    }
}
