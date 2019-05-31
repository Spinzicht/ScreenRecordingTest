using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Core.Networking
{
    public abstract class Connection
    {
        public static Connection Server { get; set; }

        public static void Split(string endpoint, out string address, out int port)
        {
            if(endpoint == null)
            {
                address = null;
                port = -1;
                return;
            }

            var split = endpoint.Split(':');
            if (split.Length != 2)
            {
                address = endpoint;
                port = Constants.SERVERPORT + (Constants.ISLAN ? 1 : 0);
                return;
            }

            address = split[0];
            port = int.Parse(split[1]);
        }

        protected Socket _socket = null;
        protected byte[] _buffer = new byte[1024];

        public int Port { get; set; } = -1;
        public int Index { get; protected set; } = -1;

        public string IP { get; protected set; }
        public string LAN { get; protected set; }
        public string Address { get => IsLAN ? LAN : IP; }

        public bool IsLAN { get; protected set; } = false;
        public virtual bool IsConnected { get; protected set; } = false;

        public bool CanSwitch { get => CanReconnect && Retries > 0 && LAN != null && IP != null; }
        public bool CanReconnect { get; protected set; } = true;
        public int Retries { get; set; } = 3;

        protected Connection() { }

        public Connection(int index, string ip, string lan = null, bool isLan = false)
        {
            Index = index;

            Split(ip, out ip, out int port);
            IP = ip;

            Split(lan, out lan, out int port2);
            LAN = lan;

            Port = port;

            IsLAN = lan != null && isLan;
            
            PacketHandler.I.AddPacket(Packet.BufferSize, SetBufferSize, Index);
        }

        ~Connection()
        {
            Close();
        }

        private void SetBufferSize(IPPacket data)
        {
            data.Read(out int size, out bool s);
            if (_buffer.Length != size && size > 1024)
                _buffer = new byte[size];
        }

        public void Connect(object obj = null)
        {
            if (_socket == null) return;
            try
            {
                SetSocketOptions();
                _socket.Bind(new IPEndPoint(IPAddress.Any, Port));
                _socket.BeginConnect(Address, Constants.SERVERPORT, new AsyncCallback(Connected), _socket);
            } catch(Exception e)
            {
                HandleException(e, "Could not start connecting");
                SwitchHost();
            }
        }

        protected virtual void SetSocketOptions()
        {
            _socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
        }

        private void Connected(IAsyncResult ar)
        {
            try
            {
                _socket.EndConnect(ar);
                Connected();
                Listen();
            }
            catch (Exception e)
            {
                HandleException(e, "Connection to server lost");
                SwitchHost();
            }
        }

        protected virtual void Connected()
        {
            Retries = 3;
            IsConnected = true;
        }

        public virtual void Start()
        {
            if (IsConnected)
                Listen();
            else
                Connect();
        }

        private SocketError error;
        private void Listen()
        {
            try
            {
                _socket.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None, out error, new AsyncCallback(MessageReceived), _socket);
            }
            catch (Exception e)
            {
                HandleException(e, "error receiving package");
            }
        }

        private void MessageReceived(IAsyncResult ar)
        {
            Socket socket = (Socket)ar.AsyncState;

            try
            {
                int received = socket.EndReceive(ar);
                if (received > 0)
                {
                    byte[] buffer = new byte[received];
                    Array.Copy(_buffer, buffer, received);

                    var packet = new IPPacket(buffer);
                    PacketHandler.I.Invoke(packet, Index);
                }
            }
            catch (Exception e)
            {
                if (!HandleException(e, "error processing received package")) return;
            }

            Listen();
        }

        public virtual bool Send(IPPacket packet)
        {
            if (!IsConnected)
            {
                "Can't Send: Server not connected".Print();
                return false;
            }

            try
            {
                _socket.Send(packet.ToArray());
                ((Packet)packet.Command).Print("Sending");
            }
            catch (Exception e)
            {
                HandleException(e, "error sending packet");
                return false;
            }
            return true;
        }

        private bool HandleException(Exception e, string msg = null, bool restart = false)
        {
            msg?.Print();
            if (e.Message.Contains("forcibly closed") || restart)
            {
                Reconnect();
                return false;
            }

            e.Print();
            return true;
        }

        protected void SwitchHost()
        {
            if (!CanSwitch)
                "Can't switch hosts".Print();
            else
                IsLAN = !IsLAN;
            Reconnect();
        }

        private void Reconnect()
        {
            Close();
            if(CanReconnect && Retries > 0)
            {
                Retries--;
                Address.Print("Connecting again in 2 seconds to");
                Timer t = new Timer(Connect, null, 2000, Timeout.Infinite);
            }
            else
            {
                if (Retries > 0)
                    Address.Print("Not allowed to reconnect to");
                else
                    Address.Print("Stopped trying to reconnect to");

                Close();
            }
        }

        public virtual void Close()
        {
            IsConnected = false;
            _socket.Close();
        }
    }
}
