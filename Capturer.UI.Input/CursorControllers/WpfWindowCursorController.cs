using System.Windows;
using System.Windows.Input;
using Capturer.UI.Input.Interfaces;

namespace Capturer.UI.Input.CursorControllers
{
    internal class WpfWindowCursorController : ICursorController
    {
        private Window _currentWindow;

        public void SetWindow(Window window)
        {
            _currentWindow = window;
        }
        
        public void HideCursor()
        {
            _currentWindow.Cursor = Cursors.None;
        }

        public void RestoreDefaultCursor()
        {
            _currentWindow.Cursor = Cursors.Arrow;
        }

        public void SetCursor(Cursor cursor)
        {
            _currentWindow.Cursor = cursor;
        }

        public void SetCursor(string cursorFile)
        {
            _currentWindow.Cursor = new Cursor(cursorFile);
        }
        
        public void Dispose()
        {
            if (_currentWindow != null && _currentWindow.IsVisible)
            {
                RestoreDefaultCursor();
            }
            _currentWindow = null;
        }
    }
}
