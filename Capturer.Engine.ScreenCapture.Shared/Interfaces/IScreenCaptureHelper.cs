using System.Drawing;

namespace Capturer.Engine.ScreenCapture.Shared.Interfaces
{
    public interface IScreenCaptureHelper
    {
        Bitmap GetScreenshot();
        Bitmap GetScreenshot(Rectangle rect);
        void SetPixelFormat(System.Drawing.Imaging.PixelFormat format);
    }
}
