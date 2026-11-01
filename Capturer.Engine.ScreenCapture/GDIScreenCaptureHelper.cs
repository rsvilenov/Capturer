using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using Capturer.Engine.ScreenCapture.Shared.Interfaces;
using WinApiHelpers;

namespace Capturer.Engine.ScreenCapture
{
    public class GDIScreenCaptureHelper : IScreenCaptureHelper
    {
        PixelFormat _pixelFormat = PixelFormat.Format32bppArgb;

        public Bitmap GetScreenshot()
        {
            var left = DisplayMonitors.AllMonitors.Min(scr => scr.DPICalibratedBounds.Left);
            var top = DisplayMonitors.AllMonitors.Min(scr => scr.DPICalibratedBounds.Top);
            var width = DisplayMonitors.AllMonitors.Sum(scr => Math.Abs(scr.DPICalibratedBounds.Width));
            var height = DisplayMonitors.AllMonitors.Sum(scr => Math.Abs(scr.DPICalibratedBounds.Height));
            
            return GetScreenshot(new Rectangle(left, top, width, height));
        }

        public Bitmap GetScreenshot(Rectangle rect)
        {
            var screenBmp = new Bitmap(rect.Width, rect.Height, _pixelFormat);
            using (var bmpGraphics = Graphics.FromImage(screenBmp))
            {
                bmpGraphics.CopyFromScreen(rect.Left, rect.Top, 0, 0, new System.Drawing.Size(rect.Width, rect.Height), CopyPixelOperation.SourceCopy);// | CopyPixelOperation.CaptureBlt);
            }
            return screenBmp;
        }

        public void SetPixelFormat(PixelFormat format)
        {
            _pixelFormat = format;
        }
    }
}
