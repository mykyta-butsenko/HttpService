using System.Net.Sockets;

namespace HttpServiceServer.MessageQueue
{
    internal interface IMessageQueue
    {
        ValueTask QueueMessageAsync((Socket handler, string receivedMessage) workItem);

        IAsyncEnumerable<(Socket handler, string receivedMessage)> DequeueAllMessagesAsync(CancellationToken cancellationToken);
    }
}
