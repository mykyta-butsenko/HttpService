using System.Net.Sockets;

namespace HttpServiceServer.MessageProcessing
{
    internal interface IMessageProcessingService
    {
        public Task ProcessReceivedMessage(Socket handler, string receivedMessage);
    }
}
