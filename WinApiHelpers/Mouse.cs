using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using static WinApiHelpers.Win32Structures;
using static WinApiHelpers.WindowApis;
using static WinApiHelpers.WindowApis.Win32.Cursor;

namespace WinApiHelpers
{
    public static class Mouse
    {
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool GetCursorPos(ref POINT pt);
        

        [StructLayout(LayoutKind.Sequential)]
        public struct CURSORINFO
        {
            public Int32 cbSize;        // Specifies the size, in bytes, of the structure. 
                                        // The caller must set this to Marshal.SizeOf(typeof(CURSORINFO)).
            public Int32 flags;         // Specifies the cursor state. This parameter can be one of the following values:
                                        //    0             The cursor is hidden.
                                        //    CURSOR_SHOWING    The cursor is showing.
            public IntPtr hCursor;          // Handle to the cursor. 
            public POINT ptScreenPos;       // A POINT structure that receives the screen coordinates of the cursor. 
        }
        
        /// <summary>Must initialize cbSize</summary>
        [DllImport("user32.dll")]
        public static extern bool GetCursorInfo(ref CURSORINFO pci);

        public const Int32 CURSOR_SHOWING = 0x00000001;

        public static System.Drawing.Point GetMousePosition()
        {
            POINT w32Mouse = new POINT();
            GetCursorPos(ref w32Mouse);
            return new System.Drawing.Point(w32Mouse.X, w32Mouse.Y);
        }

        public static int[] GetMousePositionArray()
        {
            POINT w32Mouse = new POINT();
            GetCursorPos(ref w32Mouse);
            return new int[] { w32Mouse.X, w32Mouse.Y };
        }


        //This is a replacement for Cursor.Position in WinForms
        [DllImport("user32.dll")]
        static extern bool SetCursorPos(int x, int y);
        
        [DllImport("user32.dll")]
        static extern void mouse_event(int dwFlags, int dx, int dy, int cButtons, int dwExtraInfo);

        [Flags]
        public enum MouseEventFlags : uint
        {
            LEFTDOWN = 0x00000002,
            LEFTUP = 0x00000004,
            MIDDLEDOWN = 0x00000020,
            MIDDLEUP = 0x00000040,
            MOVE = 0x00000001,
            ABSOLUTE = 0x00008000,
            RIGHTDOWN = 0x00000008,
            RIGHTUP = 0x00000010,
            WHEEL = 0x00000800,
            XDOWN = 0x00000080,
            XUP = 0x00000100
        }

        //Use the values of this enum for the 'dwData' parameter
        //to specify an X button when using MouseEventFlags.XDOWN or
        //MouseEventFlags.XUP for the dwFlags parameter.
        public enum MouseEventDataXButtons : uint
        {
            XBUTTON1 = 0x00000001,
            XBUTTON2 = 0x00000002
        }

        //This simulates a left mouse click
        public static void MouseEvent(MouseEventFlags flag, int xpos, int ypos)
        {
            SetCursorPos(xpos, ypos);
            mouse_event((int)flag, xpos, ypos, 0, 0);
        }

        public static void MouseEvent(MouseEventFlags flag)
        {
            POINT p = new POINT();
            GetCursorPos(ref p);
            MouseEvent(flag, p.X, p.Y);
        }



        #region Cursor
        /// <summary>
        /// Change the system cursor. Use only in using() { } blocks!
        /// </summary>
        public class Cursor : IDisposable
        {
            private List<uint> _cursorsToOverride;

            /// <summary>
            /// Restrict the constructor, so that the class
            /// cannot be instantiated with "new"
            /// </summary>
            /// <param name="cursorsToOverride"></param>
            private Cursor(List<uint> cursorsToOverride)
            {
                if (cursorsToOverride != null)
                {
                    _cursorsToOverride = cursorsToOverride;
                }
                else
                {
                    _cursorsToOverride = new List<uint>
                    {
                        (uint)OCR_SYSTEM_CURSORS.OCR_NORMAL,
                        (uint)OCR_SYSTEM_CURSORS.OCR_IBEAM
                    };
                }
            }

            /// <summary>
            /// Set the system cursor from an .cur file
            /// </summary>
            /// <param name="filePath">.cur file</param>
            /// <param name="cursorsToOverride">The list of system cursors to be replaced</param>
            /// <returns></returns>
            public static Cursor FromFile(string filePath, List<uint> cursorsToOverride = null)
            {
                var obj = new Cursor(cursorsToOverride);

                IntPtr loadedCursor = Win32.Cursor.LoadCursorFromFile(filePath);
                if (loadedCursor == IntPtr.Zero)
                {
                    throw new WinApiException(string.Format("Could not load cursor from file '{0}'", filePath));
                }

                obj.SetCursor(loadedCursor);
                return obj;
            }

            /// <summary>
            /// Set the system cursor by specifying a system cursor constant
            /// </summary>
            /// <param name="cursor">A member of the enum 'OCR_SYSTEM_CURSORS'</param>
            /// <param name="cursorsToOverride">The list of system cursors to be replaced</param>
            /// <returns></returns>
            public static Cursor FromOcrCursor(OCR_SYSTEM_CURSORS cursor, List<uint> cursorsToOverride = null)
            {
                var obj = new Cursor(cursorsToOverride);
                
                IntPtr loadedCursor = Win32.Icon.CopyIcon(LoadCursor(IntPtr.Zero, (int)cursor));
                if (loadedCursor == IntPtr.Zero)
                {
                    throw new WinApiException(string.Format("Could not load cursor from cursor constant '{0}'", cursor));
                }

                obj.SetCursor(loadedCursor);

                return obj;
            }
            
            /// <summary>
            /// Restore the default windows cursor
            /// </summary>
            public static void RestoreDefaultCursor()
            {
                Win32.Cursor.SystemParametersInfo(0x0057, 0, null, 0);
            }

            private void SetCursor(IntPtr loadedCursor)
            {
                foreach (var cursor in _cursorsToOverride)
                {
                    SetSystemCursor(Win32.Icon.CopyIcon(loadedCursor), cursor);
                }

            }

            #region IDisposable Support
            private bool disposedValue = false; // To detect redundant calls

            protected virtual void Dispose(bool disposing)
            {
                if (!disposedValue)
                {
                    if (disposing)
                    {
                        // TODO: dispose managed state (managed objects).
                    }

                    // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                    // TODO: set large fields to null.

                    RestoreDefaultCursor();
                    disposedValue = true;
                }
            }

            // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
            ~Cursor()
            {
                // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
                Dispose(false);
            }

            // This code added to correctly implement the disposable pattern.
            public void Dispose()
            {
                // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
                Dispose(true);
                // TODO: uncomment the following line if the finalizer is overridden above.
                GC.SuppressFinalize(this);
            }
            #endregion

        }

        #endregion

    }
}
