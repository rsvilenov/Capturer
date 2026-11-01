using System;
using System.Windows;
using System.Windows.Input;
using Capturer.UI.Input.Interfaces;

namespace Capturer.UI.Input.CursorControllers
{
    internal class SystemCursorController : ICursorController
    {
        private WinApiHelpers.Mouse.Cursor _systemCursorObject;
        private string _transparentPixelPath;
        private Window _currentWindow;

        public SystemCursorController()
        {
            _transparentPixelPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources/FFFFFF-0.cur");
        }

        public void SetWindow(Window window)
        {
            _currentWindow = window;
            _currentWindow.Closing += _currentWindow_Closing;
        }

        public void HideCursor()
        {
            _systemCursorObject = WinApiHelpers.Mouse.Cursor.FromFile(_transparentPixelPath);
        }

        public void RestoreDefaultCursor()
        {
            if (_systemCursorObject != null)
            {
                _systemCursorObject.Dispose();
                _systemCursorObject = null;
            }
        }

        public void SetCursor(string cursorFile)
        {
            _systemCursorObject = WinApiHelpers.Mouse.Cursor.FromFile(cursorFile);
        }

        public void SetCursor(Cursor cursor)
        {
            var nativeCursor = WinApiHelpers.WindowApis.Win32.Cursor.OCR_SYSTEM_CURSORS.OCR_NORMAL;

            // translate the WPF cursor into a native one
            if (cursor == Cursors.AppStarting)
                nativeCursor = WinApiHelpers.WindowApis.Win32.Cursor.OCR_SYSTEM_CURSORS.OCR_APPSTARTING;
            else if (cursor == Cursors.Arrow)
                nativeCursor = WinApiHelpers.WindowApis.Win32.Cursor.OCR_SYSTEM_CURSORS.OCR_NORMAL;
            else if (cursor == Cursors.Cross)
                nativeCursor = WinApiHelpers.WindowApis.Win32.Cursor.OCR_SYSTEM_CURSORS.OCR_CROSS;
            else if (cursor == Cursors.Hand)
                nativeCursor = WinApiHelpers.WindowApis.Win32.Cursor.OCR_SYSTEM_CURSORS.OCR_HAND;
            else if (cursor == Cursors.Help)
                nativeCursor = WinApiHelpers.WindowApis.Win32.Cursor.OCR_SYSTEM_CURSORS.OCR_HELP;
            else if (cursor == Cursors.IBeam)
                nativeCursor = WinApiHelpers.WindowApis.Win32.Cursor.OCR_SYSTEM_CURSORS.OCR_IBEAM;
            else if (cursor == Cursors.No)
                nativeCursor = WinApiHelpers.WindowApis.Win32.Cursor.OCR_SYSTEM_CURSORS.OCR_NO;
            else if (cursor == Cursors.SizeAll)
                nativeCursor = WinApiHelpers.WindowApis.Win32.Cursor.OCR_SYSTEM_CURSORS.OCR_SIZEALL;
            else if (cursor == Cursors.SizeNESW)
                nativeCursor = WinApiHelpers.WindowApis.Win32.Cursor.OCR_SYSTEM_CURSORS.OCR_SIZENESW;
            else if (cursor == Cursors.SizeNS)
                nativeCursor = WinApiHelpers.WindowApis.Win32.Cursor.OCR_SYSTEM_CURSORS.OCR_SIZENS;
            else if (cursor == Cursors.SizeNWSE)
                nativeCursor = WinApiHelpers.WindowApis.Win32.Cursor.OCR_SYSTEM_CURSORS.OCR_SIZENWSE;
            else if (cursor == Cursors.SizeWE)
                nativeCursor = WinApiHelpers.WindowApis.Win32.Cursor.OCR_SYSTEM_CURSORS.OCR_SIZEWE;
            else if (cursor == Cursors.Wait)
                nativeCursor = WinApiHelpers.WindowApis.Win32.Cursor.OCR_SYSTEM_CURSORS.OCR_WAIT;
            else if (cursor == Cursors.UpArrow)
                nativeCursor = WinApiHelpers.WindowApis.Win32.Cursor.OCR_SYSTEM_CURSORS.OCR_UP;
            else
                throw new ArgumentException("This cursor cannot be translated to a system cursor");

            WinApiHelpers.Mouse.Cursor.FromOcrCursor(nativeCursor);
        }

        private void _currentWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Dispose(false);
        }
        
        #region IDisposable Support
        private bool disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                RestoreDefaultCursor();
                if (_currentWindow != null)
                {
                    _currentWindow.Closing -= _currentWindow_Closing;
                }

                if (disposing)
                {
                    _transparentPixelPath = null;
                    _currentWindow = null;
                }

                disposedValue = true;
            }
        }
        
        ~SystemCursorController()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
        
    }
}
