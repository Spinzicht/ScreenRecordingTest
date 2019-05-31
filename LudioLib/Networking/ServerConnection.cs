using System;
using Core.Util;
using Core;
using Core.Data;
using Core.Networking;

namespace Chronos.Networking
{
    public class ServerConnection : TCPClient
    {
        protected readonly WeakEventSource<PacketEventArgs> _onUserReceived = new WeakEventSource<PacketEventArgs>();
        public event EventHandler<PacketEventArgs> OnUserReceived
        {
            add { _onUserReceived.Subscribe(value); }
            remove { _onUserReceived.Unsubscribe(value); }
        }

        protected readonly WeakEventSource<PacketEventArgs> _onConnected = new WeakEventSource<PacketEventArgs>();
        public event EventHandler<PacketEventArgs> OnConnected
        {
            add { _onConnected.Subscribe(value); }
            remove { _onConnected.Unsubscribe(value); }
        }

        protected readonly WeakEventSource<PacketEventArgs> _onUsersReceived = new WeakEventSource<PacketEventArgs>();
        public event EventHandler<PacketEventArgs> OnUsersReceived
        {
            add { _onUsersReceived.Subscribe(value); }
            remove { _onUsersReceived.Unsubscribe(value); }
        }

        private static ServerConnection _i;
        public static ServerConnection I
        {
            get => _i ?? (_i = new ServerConnection(Constants.SERVERIP, "127.0.0.1", Constants.ISLAN));
        }

        public ServerConnection(string ip, string lan = null, bool isLan = true) : base(ip, lan, isLan)
        {
            PacketHandler.I.AddPacket(Packet.Hello, Confirmed);
            PacketHandler.I.AddPacket(Packet.User, UserReceived);
            PacketHandler.I.AddPacket(Packet.Users, UsersReceived);

            Server = this;
        }


        private void UsersReceived(IPPacket data)
        {
            _onUsersReceived.Raise(this, new PacketEventArgs(data));
        }

        private void UserReceived(IPPacket data)
        {
            data.Read(out int id, out bool s, true);
            if (id == User.I.ID)
                User.I.Update(data);
            else 
                _onUserReceived.Raise(this, new PacketEventArgs(data));
        }

        public void Confirmed(IPPacket data)
        {
            data.Read(out int id, out bool s);
            User.I.Init(id);

            id.Print("connection confirmed");

            _onConnected.Raise(this, new PacketEventArgs(data));

            IPPacket packet = User.I.Pack();
            packet.Send(this);
        }
    }

    public class PacketEventArgs : EventArgs
    {
        public IPPacket Data { get; private set; }
        public PacketEventArgs(IPPacket data)
        {
            Data = data;
        }
    }
}
