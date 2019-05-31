using System.Net.Sockets;

namespace Core.Networking
{
    public class TCPClient : Connection
    {
        public TCPClient(Socket socket, int index) : base(index, socket.RemoteEndPoint.ToString())
        {
            _socket = socket;
        }

        public TCPClient(string ip, string lan = null, bool isLan = false) : base(-1, ip, lan, isLan)
        {
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }

        protected override void SetSocketOptions()
        {
            base.SetSocketOptions();
            _socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
        }
    }
}
