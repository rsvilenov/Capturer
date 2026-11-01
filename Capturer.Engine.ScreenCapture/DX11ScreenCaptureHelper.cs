using System;
using System.Drawing.Imaging;

using SharpDX;
using SharpDX.DXGI;
using Device = SharpDX.Direct3D11.Device;
using MapFlags = SharpDX.Direct3D11.MapFlags;
using SharpDX.Direct3D11;
using System.Drawing;
using System.Linq;
using System.Collections.Generic;
using Capturer.Engine.ScreenCapture.Shared.Interfaces;
using WinApiHelpers;

namespace Capturer.Engine.ScreenCapture
{
    /// <summary>
    ///   Screen capture of the desktop using DXGI OutputDuplication.
    /// </summary>
    public class DX11ScreenCaptureHelper : IScreenCaptureHelper
    {
        PixelFormat _pixelFormat = PixelFormat.Format32bppRgb; // Format32bppArgb causes some artefacts in the image
        
        public Bitmap GetScreenshot()
        {
            Bitmap finalScreenshot = null;

            var adaptersAndOutputs = GetDXOutputs();
            if (adaptersAndOutputs.Count == 1)
            {
                var ao = adaptersAndOutputs.Single();
                finalScreenshot = GetDXScreenshot(ao.Adapter, ao.Output);
            }
            else
            {
                var orderedAdaptersAndOutputs = adaptersAndOutputs.OrderBy(o => o.Output.Description.DesktopBounds.Left).
                    ThenBy(o => o.Output.Description.DesktopBounds.Top);

                var shotImages = new Dictionary<OutputAndAdapter, Bitmap>();
                foreach (var ao in orderedAdaptersAndOutputs)
                {
                    var screenShot = GetDXScreenshot(ao.Adapter, ao.Output);
                    shotImages.Add(ao, screenShot);
                }

                finalScreenshot = new Bitmap(shotImages.Sum(img => img.Value.Width), shotImages.Sum(img => img.Value.Height));

                using (Graphics g = Graphics.FromImage(finalScreenshot))
                {
                    foreach (var shot in shotImages)
                    {
                        // the screens appear in adaptersAndOutputs not in the order by which they are displayed in Control Panel (and used as first, second screen, etc...)
                        // we want to get the actual order of the screens, in order for out screenshot to be accurate
                        var actualScreen = DisplayMonitors.AllMonitors.First(scr => scr.DeviceName == shot.Key.Output.Description.DeviceName);
                        g.DrawImage(shot.Value, actualScreen.Bounds.Left, actualScreen.Bounds.Top);
                    }
                }
            }
            return finalScreenshot;
        }

        private struct OutputAndAdapter
        {
            public Output Output;
            public Adapter Adapter; 
        }

        private List<OutputAndAdapter> GetDXOutputs()
        {
            var outputs = new List<OutputAndAdapter>();
            // Create DXGI Factory1
            using (var factory = new Factory1())
            {
                foreach (var adapter in factory.Adapters)
                {
                    foreach (var output in adapter.Outputs)
                    {
                        outputs.Add(new OutputAndAdapter { Output = output, Adapter = adapter });
                    }
                }
            }
            return outputs;
        }

        private List<OutputAndAdapter> FindDXOutputsForRectangle(Rectangle rect)
        {
            var outputsContainingRect = new List<OutputAndAdapter>();
            var adaptersAndOutputs = GetDXOutputs();
            foreach (var ao in adaptersAndOutputs)
            {
                var desktopRect = new Rectangle
                {
                    Width = ao.Output.Description.DesktopBounds.Right - ao.Output.Description.DesktopBounds.Left,
                    Height = ao.Output.Description.DesktopBounds.Bottom - ao.Output.Description.DesktopBounds.Top,
                    X = ao.Output.Description.DesktopBounds.Left,
                    Y = ao.Output.Description.DesktopBounds.Top
                };

                if (Rectangle.Intersect(desktopRect, rect).IsEmpty == false)
                {
                    outputsContainingRect.Add(ao);
                }
            }
             
            return outputsContainingRect;
        }

        private Output GetOutputByDeviceName(string outputDeviceName)
        {
            using (var factory = new Factory1())
            {
                return factory.Adapters.
                    SelectMany(a => a.Outputs).
                    FirstOrDefault(op => op.Description.DeviceName == outputDeviceName);
            }
        }
                
        private Bitmap GetDXScreenshot(Adapter adapter, Output output)
        {
            Bitmap capturedImage = null;

            // Create device from Adapter
            using (var device = new Device(adapter))
            {
                using (var output1 = output.QueryInterface<Output1>())
                {
                    // Width/Height of desktop to capture
                    int width = output.Description.DesktopBounds.Right - output.Description.DesktopBounds.Left;
                    int height = output.Description.DesktopBounds.Bottom - output.Description.DesktopBounds.Top;

                    capturedImage = GetImageFromDuplicateOutput(device, output1, width, height);
                }
            }
            
            // TODO: We should cleanp up all allocated COM objects here

            return capturedImage;
        }

        private Bitmap GetImageFromDuplicateOutput(Device device, Output1 output1, int width, int height)
        {
            Bitmap capturedImage = null;

            // Create Staging texture CPU-accessible
            var textureDesc = new Texture2DDescription
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

            using (var screenTexture = new Texture2D(device, textureDesc))
            {
                // Duplicate the output
                using (var duplicatedOutput = output1.DuplicateOutput(device))
                { 

                    bool captureDone = false;
                    for (int i = 0; !captureDone; i++)
                    {
                        try
                        {
                            SharpDX.DXGI.Resource screenResource;
                            OutputDuplicateFrameInformation duplicateFrameInformation;

                            // Try to get duplicated frame within given time
                            duplicatedOutput.AcquireNextFrame(10000, out duplicateFrameInformation, out screenResource);

                            if (i > 0)
                            {
                                // copy resource into memory that can be accessed by the CPU
                                using (var screenTexture2D = screenResource.QueryInterface<Texture2D>())
                                    device.ImmediateContext.CopyResource(screenTexture2D, screenTexture);

                                // Get the desktop capture texture
                                var mapSource = device.ImmediateContext.MapSubresource(screenTexture, 0, MapMode.Read, MapFlags.None);

                                // Create Drawing.Bitmap
                                // old windows - Format32bppARgb
                                var bitmap = new System.Drawing.Bitmap(width, height, _pixelFormat);
                                var boundsRect = new System.Drawing.Rectangle(0, 0, width, height);

                                // Copy pixels from screen capture Texture to GDI bitmap
                                var mapDest = bitmap.LockBits(boundsRect, ImageLockMode.WriteOnly, bitmap.PixelFormat);
                                var sourcePtr = mapSource.DataPointer;
                                var destPtr = mapDest.Scan0;
                                for (int y = 0; y < height; y++)
                                {
                                    // Copy a single line 
                                    Utilities.CopyMemory(destPtr, sourcePtr, width * 4);

                                    // Advance pointers
                                    sourcePtr = IntPtr.Add(sourcePtr, mapSource.RowPitch);
                                    destPtr = IntPtr.Add(destPtr, mapDest.Stride);
                                }

                                // Release source and dest locks
                                bitmap.UnlockBits(mapDest);
                                device.ImmediateContext.UnmapSubresource(screenTexture, 0);

                                capturedImage = bitmap;
                                // Capture done
                                captureDone = true;
                            }

                            screenResource.Dispose();
                            duplicatedOutput.ReleaseFrame();

                        }
                        catch (SharpDXException e)
                        {
                            if (e.ResultCode.Code != SharpDX.DXGI.ResultCode.WaitTimeout.Result.Code)
                            {
                                throw e;
                            }
                        }
                    }
                }
            }

            return capturedImage;
        }

        public void SetPixelFormat(PixelFormat format)
        {
            _pixelFormat = format;
        }

        public Bitmap GetScreenshot(Rectangle rect)
        {
            throw new NotImplementedException();
        }

        //private int GetScreenNumber() 
    }
}
