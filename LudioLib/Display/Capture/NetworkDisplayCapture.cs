using Core.Networking;
using System.Drawing;
using System.IO;

namespace Chronos.Display
{
    public class NetworkDisplayCapture : DisplayCapture
    {
        public NetworkDisplayCapture()
        {
            PacketHandler.I.AddPacket(Packet.Video, ScreenReceived);
        }

        public void ScreenReceived(IPPacket p)
        {
            p.Read(out int l, out bool s);
            byte[] arr = new byte[l];
            p.Read(ref arr, out s);

            _syncContext.Post(o =>
            {
                var stream = new MemoryStream(arr);
                var bmp = (Bitmap)Image.FromStream(stream);
                Display = bmp;
            }, null);
        }
    }
}
