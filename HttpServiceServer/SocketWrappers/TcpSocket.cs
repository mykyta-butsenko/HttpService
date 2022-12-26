using System.Net;
using System.Net.Sockets;

namespace HttpServiceServer.SocketWrappers
{
    internal class TcpSocket : ISocket
    {
        private readonly Socket _socket;

        private TcpSocket(Socket socket)
        {
            _socket = socket;
        }

        public TcpSocket(AddressFamily addressFamily)
        {
            _socket = new Socket(addressFamily, SocketType.Stream, ProtocolType.Tcp);
        }

        public Task<int> SendAsync(byte[] buffer)
        {
            return _socket.SendAsync(buffer);
        }

        public void Bind(IPEndPoint ipEndPoint)
        {
            _socket.Bind(ipEndPoint);
        }

        public void Listen(int backlog)
        {
            _socket.Listen(backlog);
        }

        public async Task<ISocket> AcceptAsync(CancellationToken cancellationToken)
        {
            return new TcpSocket(await _socket.AcceptAsync(cancellationToken));
        }

        public async Task<int> ReceiveAsync(byte[] buffer)
        {
            return await _socket.ReceiveAsync(buffer, SocketFlags.None);
        }

        public void Shutdown()
        {
            _socket.Shutdown(SocketShutdown.Both);
        }

        public void Close()
        {
            _socket.Close();
        }

        public void Dispose()
        {
            _socket.Dispose();
        }
    }
}
