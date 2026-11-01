using System.Windows;
using System.Windows.Input;

namespace Capturer.UI.Input.MouseInputs
{
    /// <summary>
    /// Use WPF's ability to redirect all mouse event to the control that holds a capture of the mouse
    /// </summary>
    internal class WPFCaptureMouseInput : WPFBaseMouseInput
    {
        public override void SetWindow(Window window)
        {
            base.SetWindow(window);
            Mouse.Capture(window);
        }

        protected override void Dispose(bool disposing)
        {
            Mouse.Capture(null);
            base.Dispose(disposing);
        }
    }
}
