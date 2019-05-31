using System;
using System.Collections.ObjectModel;
using System.Linq;
using Core.Networking;

namespace Core.Data
{
    public enum ChannelState
    {
        Open,
        Streaming,
        Closed
    };

    public class Channel : DataObject
    {
        public ObservableCollection<User> Users { get; } = new ObservableCollection<User>();

        private string _name;

        public string Name
        {
            get => _name;
            set => Sync(ref _name, value);
        }

        public int UserCount { get => Users.Count; }

        private int _max;

        public int MaxUsers
        {
            get => _max;
            set => Sync(ref _max, value);
        }

        private ChannelState _state = ChannelState.Closed;
        public ChannelState ChannelState
        {
            get => _state;
            set => Sync(ref _state, value);
        }

        public Channel(ref IPPacket packet, bool sync = false)
        {
            packet.Read(out int id, out bool s, true);
            if (!s) return;

            ID = id;
            StartSync();
            Update(packet, sync);
        }

        public Channel(User host, int max)
        {
            Users.Add(host);
            ID = host.ID;
            Name = host.Name;
            host.Role = Role.Host;
            host.Channel = ID;
            MaxUsers = max;
        }

        public void Open()
        {
            StartSync();
            ChannelState = ChannelState.Open;
        }

        public void Close()
        {
            ChannelState = ChannelState.Closed;
            StopSync();
        }

        public void Start()
        {
            ChannelState = ChannelState.Streaming;
        }

        public void Stop()
        {
            Open();
        }

        public void AddUser(User user)
        {
            if (UserCount < MaxUsers && !Users.Any(x => x.ID == user.ID))
            {
                Users.Add(user);
                user.Channel = ID;
                user.Role = user.ID == ID ? Role.Host : Role.User;
            }
        }

        public void RemoveUser(User user)
        {
            var u = Users.FirstOrDefault(x => x.ID == user.ID);
            if (Users.Remove(u))
            {
                user.Channel = user.ID;
            }

            if (User.I.ID == user.ID)
                Users.Clear();
        }

        public void Update(IPPacket packet, bool sync)
        {
            packet.Read(out int id, out bool s);
            if (!s || ID != id) return;

            var _syncstate = SyncState;
            if (!sync && SyncState == SyncState.Syncing)
                SyncState = SyncState.Paused;

            packet.Read(out string name, out s);
            if (s) Name = name;
            packet.Read(out int max, out s);
            if (s) MaxUsers = max;
            packet.Read(out byte state, out s);
            if (s) ChannelState = (ChannelState)state;

            if (ChannelState == ChannelState.Closed)
                Close();

            SyncState = _syncstate;
        }

        public override IPPacket Pack()
        {
            var packet = new IPPacket(Packet.Channel);
            packet.Write(ID);
            packet.Write(Name);
            packet.Write(MaxUsers);
            packet.Write((byte)ChannelState);
            return packet;
        }
    }
}