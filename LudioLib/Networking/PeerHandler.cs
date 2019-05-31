using Core.Data;
using Core.Networking;
using System;
using System.Collections.Generic;

namespace Chronos.Networking
{
   public class PeerHandler : Connection
    { 
        private static PeerHandler _i;
        public static PeerHandler I
        {
            get => _i ?? (_i = new PeerHandler());
        }

        List<PeerConnection> peers = new List<PeerConnection>();

        public void Connect(User user, string ip, string lan)
        {
            PeerConnection peer = new PeerConnection(user, peers.Count, ip, lan);
            peer.Connect();
            peers.Add(peer);
        }

        internal void Disconnect(PeerConnection peerClient)
        {
            peerClient.Close();
            peers.Remove(peerClient);
        }

        internal void CloseAll()
        {
            peers.ForEach(x => Disconnect(x));
        }

        public void ForEach(Role role, Action<PeerConnection> action)
        {
            peers.ForEach(x =>
            {
                if (x.IsConnected && x.User.HasRole(role))
                    action(x);
            });
        }

        public override bool Send(IPPacket packet)
        {
            var result = false;
            peers.ForEach(x => result &= x.Send(packet));
            return result;
        }

        public override bool IsConnected { get => peers.Count != 0; protected set { } }
    }
}
