using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Threading;
using System.Windows.Forms;

namespace Chronos.Display
{
    public class CompressedDisplay
    {
        public CompressedDisplay(int size)
        {
            this.Data = new byte[size];
            this.Size = 4;
        }

        public int Size;
        public byte[] Data;
    }

    public class DisplayCompressor
    {
        public DisplayCompressor(GDIScreenCapture capture)
        {
            _capture = capture;
            _syncContext = SynchronizationContext.Current;

            // Initialize with black screen; get bounds from screen.
            this.screenBounds = Screen.PrimaryScreen.Bounds;

            // Initialize 2 buffers - 1 for the current and 1 for the previous image
            cur = new Bitmap(screenBounds.Width, screenBounds.Height, PixelFormat.Format32bppArgb);

            InitPrev(screenBounds);

            // Compression buffer -- we don't really need this but I'm lazy today.
            compressionBuffer = new byte[screenBounds.Width * screenBounds.Height * 4];

            // Compressed buffer -- where the data goes that we'll send.
            int backbufSize = LZ4.LZ4Codec.MaximumOutputLength(this.compressionBuffer.Length) + 4;
            backbuf = new CompressedDisplay(backbufSize);

            _capture.ScreenCaptured += Capture_ScreenCaptured;
        }

        private void InitPrev(Rectangle rect)
        {
            prev = new Bitmap(rect.Width, rect.Height, PixelFormat.Format32bppArgb);
            // Clear the 'prev' buffer - this is the initial state
            using (Graphics g = Graphics.FromImage(prev))
            {
                g.Clear(Color.Black);
            }
        }

        private void Capture_ScreenCaptured(object sender, EventArgs e)
        {
                cur = (Bitmap)_capture.Display.Clone();
                if(cur != null)
                    Iterate();
        }

        private GDIScreenCapture _capture;
        private SynchronizationContext _syncContext;
        private Rectangle screenBounds;
        private Bitmap prev;
        private Bitmap cur;
        private byte[] compressionBuffer;

        private int backbufSize;
        private CompressedDisplay backbuf;

        private int n = 0;

        private unsafe void ApplyXor(BitmapData previous, BitmapData current)
        {
            byte* prev0 = (byte*)previous.Scan0.ToPointer();
            byte* cur0 = (byte*)current.Scan0.ToPointer();

            int height = previous.Height;
            int width = previous.Width;
            int halfwidth = width / 2;

            fixed (byte* target = this.compressionBuffer)
            {
                ulong* dst = (ulong*)target;

                for (int y = 0; y < height; ++y)
                {
                    ulong* prevRow = (ulong*)(prev0 + previous.Stride * y);
                    ulong* curRow = (ulong*)(cur0 + current.Stride * y);

                    for (int x = 0; x < halfwidth; ++x)
                    {
                        *(dst++) = curRow[x] ^ prevRow[x];
                    }
                }
            }
        }

        private int Compress()
        {
            // Grab the backbuf in an attempt to update it with new data
            var backbuf = this.backbuf;

            backbuf.Size = LZ4.LZ4Codec.Encode(
                this.compressionBuffer, 0, this.compressionBuffer.Length,
                backbuf.Data, 4, backbuf.Data.Length - 4);

            Buffer.BlockCopy(BitConverter.GetBytes(backbuf.Size), 0, backbuf.Data, 0, 4);

            return backbuf.Size;
        }

        public void Iterate()
        {
            Stopwatch sw = Stopwatch.StartNew();

            TimeSpan timeToCapture = sw.Elapsed;

            if (cur.Size != prev.Size)
                InitPrev(new Rectangle(cur.Size));

            // Lock both images:
            var locked1 = cur.LockBits(new Rectangle(cur.Size),
                                       ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
            var locked2 = prev.LockBits(new Rectangle(cur.Size),
                                        ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
            try
            {
                // Xor screen:
                ApplyXor(locked2, locked1);

                TimeSpan timeToXor = sw.Elapsed;

                // Compress screen:
                int length = Compress();

                TimeSpan timeToCompress = sw.Elapsed;

                if ((++n) % 50 == 0)
                {
                    Console.WriteLine("Iteration: {0:0.00}s, {1:0.00}s, {2:0.00}s " +
                                  "{3} Kb => {4:0.0} FPS     \r",
                        timeToCapture.TotalSeconds, timeToXor.TotalSeconds,
                        timeToCompress.TotalSeconds, length / 1024,
                        1.0 / sw.Elapsed.TotalSeconds);
                }

                // Swap buffers:
            }
            finally
            {
                cur.UnlockBits(locked1);
                prev.UnlockBits(locked2);

                prev = cur;
            }
        }

    }
}
