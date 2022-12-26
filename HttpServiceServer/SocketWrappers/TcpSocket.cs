using System.Net;
using System.Net.Sockets;

namespace HttpServiceServer.SocketWrappers
{
    // ReSharper disable once ClassWithVirtualMembersNeverInherited.Global
    public class TcpSocket : ISocket
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

        public virtual Task<int> SendAsync(byte[] buffer)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException(nameof(buffer));
            }

            return _socket.SendAsync(buffer);
        }

        public void Bind(IPEndPoint ipEndPoint)
        {
            if (ipEndPoint == null)
            {
                throw new ArgumentNullException(nameof(ipEndPoint));
            }

            _socket.Bind(ipEndPoint);
        }

        public void Listen(int backlog)
        {
            if (backlog <= 0)
            {
                throw new ArgumentException("Value of the parameter should be bigger that 0 (zero)", nameof(backlog));
            }

            _socket.Listen(backlog);
        }

        public async Task<ISocket> AcceptAsync(CancellationToken cancellationToken)
        {
            return new TcpSocket(await _socket.AcceptAsync(cancellationToken));
        }

        public async Task<int> ReceiveAsync(byte[] buffer)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException(nameof(buffer));
            }

            return await _socket.ReceiveAsync(buffer, SocketFlags.None);
        }

        public virtual void Shutdown()
        {
            _socket.Shutdown(SocketShutdown.Both);
        }

        public virtual void Close()
        {
            _socket.Close();
        }

        public void Dispose()
        {
            _socket.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
