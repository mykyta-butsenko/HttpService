namespace HttpServiceServer.SocketWrappers
{
    public interface ISocket : IDisposable
    {
        Task<int> SendAsync(byte[] buffer);

        Task<ISocket> AcceptAsync(CancellationToken cancellationToken);

        Task<int> ReceiveAsync(byte[] buffer);

        void Shutdown();

        void Close();
    }
}
