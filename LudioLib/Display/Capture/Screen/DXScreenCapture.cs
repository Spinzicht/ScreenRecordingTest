using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Threading.Tasks;

using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using Device = SharpDX.Direct3D11.Device;
using MapFlags = SharpDX.Direct3D11.MapFlags;

namespace Chronos.Display
{
    public class DXScreenCapture : DisplayCapture
    {
        const int numAdapter = 0;

        // # of output device (i.e. monitor)
        const int numOutput = 0;

        Factory1 factory;
        Adapter1 adapter;
        static Device device;

        // Width/Height of desktop to capture
        int width = 1920;
        int height = 1080;

        static Output1 output1;
        Texture2DDescription textureDesc;
        private Texture2D screenTexture;
        private static OutputDuplication duplicatedOutput;

        public DXScreenCapture()
        {
            // Create DXGI Factory1
            if (device == null)
            {
                factory = new Factory1();
                adapter = factory.GetAdapter1(numAdapter);

                // Create device from Adapter
                device = new Device(adapter);
            }
            if(output1 == null)
            {

            // Get DXGI.Output
            var output = adapter.GetOutput(numOutput);
            output1 = output.QueryInterface<Output1>();

                // Duplicate the output
                duplicatedOutput = output1.DuplicateOutput(device);
            }

            // Create Staging texture CPU-accessible
            textureDesc = new Texture2DDescription
            {
                CpuAccessFlags = CpuAccessFlags.Read,
                BindFlags = BindFlags.None,
                Format = Format.B8G8R8A8_UNorm,
                Width = width,
                Height = height,
                OptionFlags = ResourceOptionFlags.None,
                MipLevels = 1,
                ArraySize = 1,
                SampleDescription = { Count = 1, Quality = 0 },
                Usage = ResourceUsage.Staging
            };

            screenTexture = new Texture2D(device, textureDesc);

        }

        int targetCaptureTime = 20;

        public override async Task Run()
        {
            await base.Run();
            bool captureDone = false;
            while (!captureDone)
            {
                SharpDX.DXGI.Resource screenResource = null;
                OutputDuplicateFrameInformation duplicateFrameInformation;

                try
                {
                    // Try to get duplicated frame within given time
                    duplicatedOutput.AcquireNextFrame(targetCaptureTime, out duplicateFrameInformation, out screenResource);

                    // copy resource into memory that can be accessed by the CPU
                    using (var screenTexture2D = screenResource.QueryInterface<Texture2D>())
                        device.ImmediateContext.CopyResource(screenTexture2D, screenTexture);

                    // Get the desktop capture texture
                    var mapSource = device.ImmediateContext.MapSubresource(screenTexture, 0, MapMode.Read, MapFlags.None);
                    var boundsRect = new Rectangle(0, 0, width / Zoom, height / Zoom);

                    await Task.Run(() => {

                        int nWidth = width / Zoom;
                        int nHeight = height / Zoom;

                        int middleX = (int)mousePos.X - nWidth / 2;
                        int startX = Math.Max(Math.Min(middleX, width - nWidth - 1), 0);

                        int middleY = (int)mousePos.Y - nHeight / 2;
                        int startY = Math.Max(Math.Min(middleY, height - nHeight - 1), 0);

                        try
                        {
                            // Create Drawing.Bitmap
                            var bitmap = new Bitmap(nWidth, nHeight, PixelFormat.Format32bppArgb);

                            // Copy pixels from screen capture Texture to GDI bitmap
                            var mapDest = bitmap.LockBits(boundsRect, ImageLockMode.WriteOnly, bitmap.PixelFormat);
                            var sourcePtr = mapSource.DataPointer;
                            var destPtr = mapDest.Scan0;

                            sourcePtr = IntPtr.Add(sourcePtr, mapSource.RowPitch * startY + startX * 4);

                            for (int y = startY; y < nHeight + startY && y < height; y++)
                            {
                                // Copy a single line 
                                Utilities.CopyMemory(destPtr, sourcePtr, nWidth * 4);

                                // Advance pointers
                                sourcePtr = IntPtr.Add(sourcePtr, mapSource.RowPitch);
                                destPtr = IntPtr.Add(destPtr, mapDest.Stride);
                            }

                            // Release source and dest locks
                            bitmap.UnlockBits(mapDest);
                            device.ImmediateContext.UnmapSubresource(screenTexture, 0);

                            // Save the output
                            Display = (Bitmap)bitmap.Clone();

                            // Capture done
                            captureDone = true;
                        }
                        catch(Exception e)
                        {
                            targetCaptureTime++;
                        }
                    });
                }
                catch (SharpDXException e)
                {
                    if (e.ResultCode.Code != SharpDX.DXGI.ResultCode.WaitTimeout.Result.Code)
                    {
                        throw e;
                    }
                    else
                        targetCaptureTime++;
                }
                finally
                {
                    if (screenResource != null)
                    {
                        screenResource.Dispose();
                        screenResource = null;
                        duplicatedOutput.ReleaseFrame();
                    }
                }
            }
        }
    }
}