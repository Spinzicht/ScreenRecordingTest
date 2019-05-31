using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using Chronos.Networking;
using Core;
using Core.Data;
using Core.Networking;
using Ludio.Base;

namespace Ludio.VM
{
    public class Overview : ViewModel
    {
        private static Overview _i;

        public static Overview I { get => _i ?? (_i = new Overview()); }

        public ObservableCollection<User> Users { get; } = new ObservableCollection<User>();
        public ObservableCollection<Channel> Channels { get; } = new ObservableCollection<Channel>();
        
        public RelayCommand JoinChannelCommand { get; private set; }

        public Overview()
        {
            JoinChannelCommand = new RelayCommand(JoinChannel);

            PacketHandler.I.AddPacket(Packet.Channel, SyncChannel);
            PacketHandler.I.AddPacket(Packet.Channels, SyncChannels);
            ServerConnection.I.OnConnected += OnConnected;
        }

        private void OnConnected(object sender, PacketEventArgs e)
        {
            var data = e.Data;
            data.Read(out byte command, out bool s);
            SyncChannels(data);
        }

        private void SyncChannel(IPPacket data)
        {
            _syncContext.Post(o =>
            {
                data.Read(out int id, out bool s, true);
                var channel = Channels.FirstOrDefault(x => x.ID == id);
                if (channel == null)
                    Channels.Add(new Channel(ref data));
                else
                {
                    channel.Update(data, false);
                    if (channel.ChannelState == ChannelState.Closed)
                        Channels.Remove(channel);
                }
                data.Dispose();
            }, null);
        }

        private void SyncChannels(IPPacket data)
        {
            _syncContext.Post(o =>
            {
                Channels.Clear();
                data.Read(out int channelcount, out bool s);
                for (int i = 0; i < channelcount; i++)
                {
                    data.Read(out byte command, out s);
                    Channels.Add(new Channel(ref data));
                }
                data.Dispose();
            }, null);
        }

        private void JoinChannel(object obj)
        {
            var channel = (Channel)obj;
            Lobbies.I.Open(channel);
        }
    }
}