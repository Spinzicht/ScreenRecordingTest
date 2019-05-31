using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Core.Networking
{
    public enum Packet
    {
        Error = 0,
        Debug = 1,
        BufferSize = 2,

        Hello = 10,
        Goodbye = 11,

        User = 20,
        Channel = 21,
        HostInfo = 22,
        ClientInfo = 23,
        Channels = 24,

        AudioInit = 30,
        Audio = 31,
        Video = 32,
        Text = 33,
        Users = 34,
    }

    public class PacketHandler
    {
        private static PacketHandler _i;
        public static PacketHandler I
        {
            get => _i ?? (_i = new PacketHandler());
        }

        private PacketHandler()
        {
            AddPacket(Packet.Debug, DebugPacket);
        }

        public void DebugPacket(IPPacket data)
        {
            data.Read(out string msg, out bool s);
            if(s && msg != "") msg.Print();
        }

        public delegate void Command(IPPacket data);

        private Dictionary<(int, byte), Command> Packets = new Dictionary<(int, byte), Command>();

        public void AddPacket(Packet packet, Command command, int index = -1)
        {
            var key = (index, (byte)packet);
            if(!Packets.Keys.Contains(key))
            {
                Packets.Add(key, command);
            }
        }

        public void Invoke(IPPacket packet, int index = -1)
        {
            var name = Enum.GetName(typeof(Packet), packet.Command);
            name.Print("packet received");
            if (Packets.TryGetValue((index, packet.Command), out Command command))
            {
                command.Invoke(packet);
            }
            else
            {
                Console.WriteLine("command for index " + index + " " +  name + " not found");
            }
        }
    }
}