using HttpServiceServer.SocketWrappers;

namespace HttpServiceServer.MessageProcessing
{
    public interface IMessageProcessingService
    {
        public Task ProcessReceivedMessage(ISocket handler, string receivedMessage);
    }
}
