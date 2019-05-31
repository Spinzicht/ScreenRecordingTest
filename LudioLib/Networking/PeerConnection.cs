using Core;
using Core.Data;
using Core.Networking;
using System;
using System.Net;
using System.Net.Sockets;

namespace Chronos.Networking
{
    public class PeerConnection : UDPClient
    {
        public User User { get; private set; }

        public PeerConnection(User u, int index, string ip, string lan) : base(index, ip, lan) { User = u; }

        public static string GetLocalIPAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            throw new Exception("No network adapters with an IPv4 address in the system!");
        }

        public void SendSize(int s)
        {
            var packet = new IPPacket(Packet.BufferSize);
            try
            {
                packet.Write(s);
                Send(packet);
                packet.Dispose();
            }
            catch (Exception e)
            {
                "sending size error".Print();
            }
            finally
            {
                packet.Dispose();
            }
        }

        /*public void SendScreen(object data)
        {
            var packet = new IPPacket(100);
            try
            {
                var arr = ((Bitmap)data.Display.Clone()).ToByteArray(ImageFormat.Bmp);
                packet.Write(arr.Length);
                packet.Write(arr);
                SendSize(packet.ToArray().Length);
                Send(packet);
                ("sending screen" + packet.ToArray().Length).Print();
            }
            catch (Exception e)
            {
                e.Print();
                "sending screen error".Print();
            }
            finally
            {
                packet.Dispose();
            }
        }

        internal void InitAudio(int samples, int channels)
        {
            IPPacket packet = new IPPacket(201);
            try
            {
                packet.Write(samples);
                packet.Write(channels);
                SendSize(packet.ToArray().Length);
                Send(packet);
                "audio init sent".Print();
            }
            catch (Exception e)
            {
                "sending audio init error".Print();
            }
            finally
            {
                packet.Dispose();
            }
        }

        internal void SendAudio(byte[] buffer, int bytesRecorded, int frames = 0)
        {
            IPPacket packet = new IPPacket(200);
            try
            {
                packet.Write(frames);
                packet.Write(bytesRecorded);
                packet.Write(buffer);
                SendSize(packet.ToArray().Length);
                Send(packet);
            }
            catch (Exception e)
            {
                "sending audio error".Print();
            }
            finally
            {
                packet.Dispose();
            }
        }

        internal void SendAudioResponse()
        {
            var packet = new IPPacket(202);
            try
            {
                Send(packet);
            }
            catch (Exception e)
            {
                "sending audio init response error".Print();
            }
            finally
            {
                packet.Dispose();
            }
        }*/
    }
}
