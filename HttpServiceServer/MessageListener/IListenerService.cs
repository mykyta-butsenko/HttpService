using HttpServiceServer.SocketWrappers;

namespace HttpServiceServer.MessageListener
{
    internal interface IListenerService
    {
        void StartListening(ISocket listener);
    }
}
