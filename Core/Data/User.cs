using System;
using Core.Networking;
using Core.Util;

namespace Core.Data
{
    public enum Role
    {
        All,
        None,
        Host,
        Client,
        Spectator,
        User
    }

    public class User : DataObject  
    {
        protected readonly WeakEventSource<EventArgs> _onChannelChanged = new WeakEventSource<EventArgs>();
        public event EventHandler<EventArgs> OnChannelChanged
        {
            add { _onChannelChanged.Subscribe(value); }
            remove { _onChannelChanged.Unsubscribe(value); }
        }

        private static User _i;

        public static User I { get => _i ?? (_i = new User()); }

        public string IP { get; set; }
        public string LAN { get; set; }

        private Role _role = Role.Host;   
        public Role Role
        {
            get => _role;
            set => Sync(ref _role, value);
        }

        private string _name;
        public string Name
        {
            get => _name;
            set => Sync(ref _name, value);
        }

        private int _channel;
        public int Channel
        {
            get => _channel;
            set
            {
                if (Sync(ref _channel, value))
                    _onChannelChanged.Raise(this, null);
            }
           
        }

        private User() { }

        public User(IPPacket data)
        {
            Init(data);
            //StartSync();
        }

        public User(int id, Role role = Role.None)
        {
            ID = id;
            Channel = id;
            Role = role;

            StartSync();
        }

        public User(int id, string name, Role role = Role.None)
        {
            ID = id;
            Channel = id;
            Role = role;
            Name = name;

            StartSync();
        }

        internal bool HasRole(Role role)
        {
            return role == Role.All || role == Role || 
                   (role == Role.Client && (Role == Role.User || Role == Role.Spectator));
        }

        public void Load(string ip, string lan, string name)
        {
            IP = ip;
            LAN = lan;
            Name = name;

            StartSync();
        }

        public void Init(int id)
        {
            id.Print("init user");
            ID = Channel = id;
        }

        private void Init(IPPacket packet)
        {
            packet.Read(out int id, out bool s);
            packet.Read(out int channel, out s);
            packet.Read(out byte role, out s);
            packet.Read(out string name, out s);
            packet.Read(out string ip, out s);
            packet.Read(out string lan, out s);
            if (!s) return;

            ID = id;
            Channel = channel;
            Role = (Role)role;
            Name = name;
            IP = ip;
            LAN = lan;
        }

        public void Update(IPPacket packet, bool sync = false)
        {
            packet.Read(out int id, out bool s);
            if (!s || ID != id) return;

            var _state = SyncState;
            if (!sync && SyncState == SyncState.Syncing)
                SyncState = SyncState.Paused;

            packet.Read(out int channel, out s);
            if(s) Channel = channel;
            packet.Read(out byte role, out s);
            if(s) Role = (Role)role;
            packet.Read(out string name, out s);
            if(s) Name = name ?? Name;

            packet.Read(out string ip, out s);
            if(s) IP = ip;
            packet.Read(out string lan, out s);
            if(s) LAN = lan;

            SyncState = _state;
        }

        public override IPPacket Pack()
        {
            var packet = new IPPacket(Packet.User);
            packet.Write(ID);
            packet.Write(Channel);
            packet.Write((byte)Role);
            packet.Write(Name);

            packet.Write(IP);
            packet.Write(LAN);

            return packet;
        }
    }
}
