using Core.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Core.Networking
{
    public class UDPClient : Connection
    {
        public UDPClient(int index, string ip, string lan) : base(index, ip, lan, true)
        {
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        }
    }
}
