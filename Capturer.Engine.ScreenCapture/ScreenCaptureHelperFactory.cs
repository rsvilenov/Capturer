using System;
using System.Collections.Generic;
using System.Linq;
using Capturer.Engine.ScreenCapture.Shared.Enums;
using Capturer.Engine.ScreenCapture.Shared.Interfaces;

namespace Capturer.Engine.ScreenCapture
{
    public class ScreenCaptureHelperFactory
    {
        private static Dictionary<ScreenCaptureMethods, Type> _captureHelpers = new Dictionary<ScreenCaptureMethods, Type>
        {
            {ScreenCaptureMethods.GDI,  typeof(GDIScreenCaptureHelper)},
            {ScreenCaptureMethods.DirectX11,  typeof(DX11ScreenCaptureHelper)}
        };

        public static IScreenCaptureHelper GetScreenCaptureHelper(ScreenCaptureMethods captureMethod)
        {
            var helperType = _captureHelpers.
                Where(h => h.Key == captureMethod).
                Select(h => h.Value).
                SingleOrDefault();

            IScreenCaptureHelper instance = null;
            if (helperType != null)
            {
                instance = (IScreenCaptureHelper)Activator.CreateInstance(helperType);
            }
            return instance;
        }
    }
}
