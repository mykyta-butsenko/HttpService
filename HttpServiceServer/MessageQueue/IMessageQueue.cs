using HttpServiceServer.SocketWrappers;

namespace HttpServiceServer.MessageQueue
{
    internal interface IMessageQueue
    {
        Task QueueMessageAsync((ISocket handler, string receivedMessage) workItem);

        IAsyncEnumerable<(ISocket handler, string receivedMessage)> DequeueAllMessagesAsync(CancellationToken cancellationToken);
    }
}
