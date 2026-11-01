using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static WinApiHelpers.Win32Structures;

namespace WinApiHelpers
{
    public static class WindowApis
    {
        public class Win32
        {
            [DllImport("user32.dll")]
            public static extern IntPtr GetActiveWindow();

            // always call setlasterror before this function: https://msdn.microsoft.com/en-us/library/windows/desktop/ms633591(v=vs.85).aspx
            [DllImport("user32.dll", SetLastError = true)]
            public static extern int SetWindowLong(IntPtr hWnd, int nIndex, UInt32 dwNewLong);

            [DllImport("user32.dll")]
            public static extern bool ShowWindowAsync(IntPtr hWnd, int nCmdShow);

            [DllImport("user32.dll", EntryPoint = "SetLayeredWindowAttributes")]
            public static extern int SetLayeredWindowAttributes(IntPtr hwnd, int crKey, byte bAlpha, int dwFlags);

            [DllImport("user32.dll", EntryPoint = "SetWindowPos")]
            public static extern int SetWindowPos(IntPtr hwnd, int hwndInsertAfter, int x, int y, int cx, int cy, int uFlags);

            [DllImport("Dwmapi.dll")]
            public static extern uint DwmExtendFrameIntoClientArea(IntPtr hWnd, ref MARGINS margins);

            public const int WM_NCLBUTTONDOWN = 0xA1;
            public const int HT_CAPTION = 0x2;

            [DllImportAttribute("user32.dll")]
            public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
            [DllImportAttribute("user32.dll")]
            public static extern bool ReleaseCapture();

            [DllImport("user32.dll", SetLastError = true)]
            public static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string lpszClass, string lpszWindow);

            [DllImport("user32.dll", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool GetWindowRect(IntPtr hwnd, out RECT lpRect);

            [DllImport("user32.dll")]
            public static extern bool DrawAnimatedRects(IntPtr hwnd, int idAni, [In] ref RECT lprcFrom, [In] ref RECT lprcTo);


            public struct MARGINS
            {
                public int cxLeftWidth;
                public int cxRightWidth;
                public int cyTopHeight;
                public int cyBottomHeight;
            }

            public enum IDANI : Int32
            {
                /// <summary>
                /// This constant is not implemented on any Windows platform!
                /// </summary>
                IDANI_OPEN = 1,

                /// <summary>
                /// The window caption will animate from lprcFrom to lprcTo.
                /// </summary>
                IDANI_CAPTION = 3
        }

            public enum WindowLongFlags : int
            {
                GWL_EXSTYLE = -20,
                GWLP_HINSTANCE = -6,
                GWLP_HWNDPARENT = -8,
                GWL_ID = -12,
                GWL_STYLE = -16,
                GWL_USERDATA = -21,
                GWL_WNDPROC = -4,
                DWLP_USER = 0x8,
                DWLP_MSGRESULT = 0x0,
                DWLP_DLGPROC = 0x4
            }

            public const int LWA_ALPHA = 0x2;
            public const int LWA_COLORKEY = 0x1;

            /// <summary>
            ///     Special window handles
            /// </summary>
            public enum SpecialWindowHandles
            {
                // ReSharper disable InconsistentNaming
                /// <summary>
                ///     Places the window at the top of the Z order.
                /// </summary>
                HWND_TOP = 0,
                /// <summary>
                ///     Places the window at the bottom of the Z order. If the hWnd parameter identifies a topmost window, the window loses its topmost status and is placed at the bottom of all other windows.
                /// </summary>
                HWND_BOTTOM = 1,
                /// <summary>
                ///     Places the window above all non-topmost windows. The window maintains its topmost position even when it is deactivated.
                /// </summary>
                HWND_TOPMOST = -1,
                /// <summary>
                ///     Places the window above all non-topmost windows (that is, behind all topmost windows). This flag has no effect if the window is already a non-topmost window.
                /// </summary>
                HWND_NOTOPMOST = -2
                // ReSharper restore InconsistentNaming
            }

            [Flags]
            public enum SetWindowPosFlags : uint
            {
                // ReSharper disable InconsistentNaming

                /// <summary>
                ///     If the calling thread and the thread that owns the window are attached to different input queues, the system posts the request to the thread that owns the window. This prevents the calling thread from blocking its execution while other threads process the request.
                /// </summary>
                SWP_ASYNCWINDOWPOS = 0x4000,

                /// <summary>
                ///     Prevents generation of the WM_SYNCPAINT message.
                /// </summary>
                SWP_DEFERERASE = 0x2000,

                /// <summary>
                ///     Draws a frame (defined in the window's class description) around the window.
                /// </summary>
                SWP_DRAWFRAME = 0x0020,

                /// <summary>
                ///     Applies new frame styles set using the SetWindowLong function. Sends a WM_NCCALCSIZE message to the window, even if the window's size is not being changed. If this flag is not specified, WM_NCCALCSIZE is sent only when the window's size is being changed.
                /// </summary>
                SWP_FRAMECHANGED = 0x0020,

                /// <summary>
                ///     Hides the window.
                /// </summary>
                SWP_HIDEWINDOW = 0x0080,

                /// <summary>
                ///     Does not activate the window. If this flag is not set, the window is activated and moved to the top of either the topmost or non-topmost group (depending on the setting of the hWndInsertAfter parameter).
                /// </summary>
                SWP_NOACTIVATE = 0x0010,

                /// <summary>
                ///     Discards the entire contents of the client area. If this flag is not specified, the valid contents of the client area are saved and copied back into the client area after the window is sized or repositioned.
                /// </summary>
                SWP_NOCOPYBITS = 0x0100,

                /// <summary>
                ///     Retains the current position (ignores X and Y parameters).
                /// </summary>
                SWP_NOMOVE = 0x0002,

                /// <summary>
                ///     Does not change the owner window's position in the Z order.
                /// </summary>
                SWP_NOOWNERZORDER = 0x0200,

                /// <summary>
                ///     Does not redraw changes. If this flag is set, no repainting of any kind occurs. This applies to the client area, the nonclient area (including the title bar and scroll bars), and any part of the parent window uncovered as a result of the window being moved. When this flag is set, the application must explicitly invalidate or redraw any parts of the window and parent window that need redrawing.
                /// </summary>
                SWP_NOREDRAW = 0x0008,

                /// <summary>
                ///     Same as the SWP_NOOWNERZORDER flag.
                /// </summary>
                SWP_NOREPOSITION = 0x0200,

                /// <summary>
                ///     Prevents the window from receiving the WM_WINDOWPOSCHANGING message.
                /// </summary>
                SWP_NOSENDCHANGING = 0x0400,

                /// <summary>
                ///     Retains the current size (ignores the cx and cy parameters).
                /// </summary>
                SWP_NOSIZE = 0x0001,

                /// <summary>
                ///     Retains the current Z order (ignores the hWndInsertAfter parameter).
                /// </summary>
                SWP_NOZORDER = 0x0004,

                /// <summary>
                ///     Displays the window.
                /// </summary>
                SWP_SHOWWINDOW = 0x0040,

                // ReSharper restore InconsistentNaming
            }



            /// <summary>
            /// SetWindowPos Flags
            /// </summary>
            public static class SWP
            {
                public static readonly int
                NOSIZE = 0x0001,
                NOMOVE = 0x0002,
                NOZORDER = 0x0004,
                NOREDRAW = 0x0008,
                NOACTIVATE = 0x0010,
                DRAWFRAME = 0x0020,
                FRAMECHANGED = 0x0020,
                SHOWWINDOW = 0x0040,
                HIDEWINDOW = 0x0080,
                NOCOPYBITS = 0x0100,
                NOOWNERZORDER = 0x0200,
                NOREPOSITION = 0x0200,
                NOSENDCHANGING = 0x0400,
                DEFERERASE = 0x2000,
                ASYNCWINDOWPOS = 0x4000;
            }

            [Flags]
            public enum WindowStyles : uint
            {
                WS_OVERLAPPED = 0x00000000,
                WS_POPUP = 0x80000000,
                WS_CHILD = 0x40000000,
                WS_MINIMIZE = 0x20000000,
                WS_VISIBLE = 0x10000000,
                WS_DISABLED = 0x08000000,
                WS_CLIPSIBLINGS = 0x04000000,
                WS_CLIPCHILDREN = 0x02000000,
                WS_MAXIMIZE = 0x01000000,
                WS_BORDER = 0x00800000,
                WS_DLGFRAME = 0x00400000,
                WS_VSCROLL = 0x00200000,
                WS_HSCROLL = 0x00100000,
                WS_SYSMENU = 0x00080000,
                WS_THICKFRAME = 0x00040000,
                WS_GROUP = 0x00020000,
                WS_TABSTOP = 0x00010000,

                WS_MINIMIZEBOX = 0x00020000,
                WS_MAXIMIZEBOX = 0x00010000,

                WS_CAPTION = WS_BORDER | WS_DLGFRAME,
                WS_TILED = WS_OVERLAPPED,
                WS_ICONIC = WS_MINIMIZE,
                WS_SIZEBOX = WS_THICKFRAME,
                WS_TILEDWINDOW = WS_OVERLAPPEDWINDOW,

                WS_OVERLAPPEDWINDOW = WS_OVERLAPPED | WS_CAPTION | WS_SYSMENU | WS_THICKFRAME | WS_MINIMIZEBOX | WS_MAXIMIZEBOX,
                WS_POPUPWINDOW = WS_POPUP | WS_BORDER | WS_SYSMENU,
                WS_CHILDWINDOW = WS_CHILD,

                //Extended Window Styles

                WS_EX_DLGMODALFRAME = 0x00000001,
                WS_EX_NOPARENTNOTIFY = 0x00000004,
                WS_EX_TOPMOST = 0x00000008,
                WS_EX_ACCEPTFILES = 0x00000010,
                WS_EX_TRANSPARENT = 0x00000020,

                //#if(WINVER >= 0x0400)

                WS_EX_MDICHILD = 0x00000040,
                WS_EX_TOOLWINDOW = 0x00000080,
                WS_EX_WINDOWEDGE = 0x00000100,
                WS_EX_CLIENTEDGE = 0x00000200,
                WS_EX_CONTEXTHELP = 0x00000400,

                WS_EX_RIGHT = 0x00001000,
                WS_EX_LEFT = 0x00000000,
                WS_EX_RTLREADING = 0x00002000,
                WS_EX_LTRREADING = 0x00000000,
                WS_EX_LEFTSCROLLBAR = 0x00004000,
                WS_EX_RIGHTSCROLLBAR = 0x00000000,

                WS_EX_CONTROLPARENT = 0x00010000,
                WS_EX_STATICEDGE = 0x00020000,
                WS_EX_APPWINDOW = 0x00040000,

                WS_EX_OVERLAPPEDWINDOW = (WS_EX_WINDOWEDGE | WS_EX_CLIENTEDGE),
                WS_EX_PALETTEWINDOW = (WS_EX_WINDOWEDGE | WS_EX_TOOLWINDOW | WS_EX_TOPMOST),
                //#endif /* WINVER >= 0x0400 */

                //#if(WIN32WINNT >= 0x0500)

                WS_EX_LAYERED = 0x00080000,
                //#endif /* WIN32WINNT >= 0x0500 */

                //#if(WINVER >= 0x0500)

                WS_EX_NOINHERITLAYOUT = 0x00100000, // Disable inheritence of mirroring by children
                WS_EX_LAYOUTRTL = 0x00400000, // Right to left mirroring
                                              //#endif /* WINVER >= 0x0500 */

                //#if(WIN32WINNT >= 0x0500)

                WS_EX_COMPOSITED = 0x02000000,
                WS_EX_NOACTIVATE = 0x08000000
                //#endif /* WIN32WINNT >= 0x0500 */

            }

            public static class Icon
            {

                [DllImport("user32.dll")]
                public static extern IntPtr CopyIcon(IntPtr pcur);

            }

            public static class Cursor
            {

                [DllImport("user32.dll")]
                public static extern bool SetSystemCursor(IntPtr hcur, uint id);

                [DllImport("user32.dll")]
                public static extern IntPtr LoadCursor(IntPtr hInstance, int lpCursorName);

                [DllImport("user32.dll", CharSet = CharSet.Auto)]
                public static extern Int32 SystemParametersInfo(UInt32 uiAction, UInt32
                uiParam, String pvParam, UInt32 fWinIni);

                public enum OCR_SYSTEM_CURSORS : uint
                {
                    /// <summary>
                    /// Standard arrow and small hourglass
                    /// </summary>
                    OCR_APPSTARTING = 32650,
                    /// <summary>
                    /// Standard arrow
                    /// </summary>
                    OCR_NORMAL = 32512,
                    /// <summary>
                    /// Crosshair
                    /// </summary>
                    OCR_CROSS = 32515,
                    /// <summary>
                    /// Windows 2000/XP: Hand
                    /// </summary>
                    OCR_HAND = 32649,
                    /// <summary>
                    /// Arrow and question mark
                    /// </summary>
                    OCR_HELP = 32651,
                    /// <summary>
                    /// I-beam
                    /// </summary>
                    OCR_IBEAM = 32513,
                    /// <summary>
                    /// Slashed circle
                    /// </summary>
                    OCR_NO = 32648,
                    /// <summary>
                    /// Four-pointed arrow pointing north, south, east, and west
                    /// </summary>
                    OCR_SIZEALL = 32646,
                    /// <summary>
                    /// Double-pointed arrow pointing northeast and southwest
                    /// </summary>
                    OCR_SIZENESW = 32643,
                    /// <summary>
                    /// Double-pointed arrow pointing north and south
                    /// </summary>
                    OCR_SIZENS = 32645,
                    /// <summary>
                    /// Double-pointed arrow pointing northwest and southeast
                    /// </summary>
                    OCR_SIZENWSE = 32642,
                    /// <summary>
                    /// Double-pointed arrow pointing west and east
                    /// </summary>
                    OCR_SIZEWE = 32644,
                    /// <summary>
                    /// Vertical arrow
                    /// </summary>
                    OCR_UP = 32516,
                    /// <summary>
                    /// Hourglass
                    /// </summary>
                    OCR_WAIT = 32514
                }


                [DllImport("User32.dll", CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
                public static extern IntPtr LoadCursorFromFile(String str);
                
            }
        }

        public static void MakeTheWindowAPopup(IntPtr hwnd)
        {
            int ret = Win32.SetWindowLong(hwnd, (int)Win32.WindowLongFlags.GWL_STYLE, (uint)Win32.WindowStyles.WS_POPUP | (uint)Win32.WindowStyles.WS_VISIBLE);
            if (ret == 0)
                throw new WinApiException("SetWindowLong failed");
        }

        public static void SetWindowEventPassThrough(IntPtr hwnd)
        {
            // Transparent windows with click through
            // https://blogs.msdn.microsoft.com/oldnewthing/20121217-00/?p=5823
            int ret = Win32.SetWindowLong(hwnd, (int)Win32.WindowLongFlags.GWL_EXSTYLE, (uint)Win32.WindowStyles.WS_EX_LAYERED | (uint)Win32.WindowStyles.WS_EX_TRANSPARENT);//GWL_EXSTYLE=-20; WS_EX_LAYERED=524288=&h80000, WS_EX_TRANSPARENT=32=0x00000020L
            if (ret == 0)
                throw new WinApiException("SetWindowLong failed");
        }

        public static void SetTransparency(IntPtr hwnd, byte alpha)
        {
            if (alpha < 255)
            {
                int ret = Win32.SetLayeredWindowAttributes(hwnd, 0, alpha, Win32.LWA_ALPHA);// Transparency=51=20%, LWA_ALPHA=2
                if (ret == 0)
                    throw new WinApiException("SetLayeredWindowAttributes failed");
            }
        }

        public static void ExtendIntoClientArea(IntPtr hwnd)
        {
            var margins = new Win32.MARGINS() { cxLeftWidth = -1 };
            uint ret = Win32.DwmExtendFrameIntoClientArea(hwnd, ref margins);
            if (ret != 0)
                throw new WinApiException("DwmExtendFrameIntoClientArea failed");

        }
        public static void SetTopMost(IntPtr hwnd, int x, int y, int width, int height)
        {
            int ret = Win32.SetWindowPos(hwnd, (int)Win32.SpecialWindowHandles.HWND_TOPMOST, x, y, width, height, (int)Win32.SetWindowPosFlags.SWP_FRAMECHANGED | (int)Win32.SetWindowPosFlags.SWP_SHOWWINDOW); //SWP_FRAMECHANGED = 0x0020 (32); //SWP_SHOWWINDOW = 0x0040 (64)
            if (ret == 0)
                throw new WinApiException("SetWindowPos failed");
        }

        public static void SetWindowsPosition(IntPtr hWnd, int x, int y, int width, int height, bool keepZOrder = true)
        {
            int flags = 0;
            if (keepZOrder)
                flags = (int)Win32.SetWindowPosFlags.SWP_NOZORDER | (int)Win32.SetWindowPosFlags.SWP_NOOWNERZORDER | (int)Win32.SetWindowPosFlags.SWP_NOACTIVATE;

            int ret = Win32.SetWindowPos(hWnd, 0, x, y, width, height, flags);
            if (ret == 0)
                throw new WinApiException("SetWindowPos failed");
        }


        public static void AnimateWindowToNotificationArea(IntPtr handle)
        {

            WinApiHelpers.Win32Structures.RECT animateFrom = new WinApiHelpers.Win32Structures.RECT();
            Window.GetWindowRect(handle, out animateFrom);

            WinApiHelpers.Win32Structures.RECT animateTo = new WinApiHelpers.Win32Structures.RECT();
                IntPtr notifyAreaHandle = GetNotificationAreaHandle();

            if (notifyAreaHandle != IntPtr.Zero)
            {
                if (Win32.GetWindowRect(notifyAreaHandle, out animateTo) == true)
                {
                   bool success = Win32.DrawAnimatedRects(handle,
                        (int)Win32.IDANI.IDANI_CAPTION, ref animateFrom, ref animateTo);
                    if (!success)
                        throw new WinApiException("DrawAnimatedRects failed");

                }
            }
            else
                throw new WinApiException("could't find the notification area");
            
        }

        public static IntPtr GetNotificationAreaHandle()
        {
            IntPtr hwnd = Win32.FindWindowEx(IntPtr.Zero, IntPtr.Zero, "Shell_TrayWnd", null);
            hwnd = Win32.FindWindowEx(hwnd, IntPtr.Zero, "TrayNotifyWnd", null);
            hwnd = Win32.FindWindowEx(hwnd, IntPtr.Zero, "SysPager", null);

            //if (hwnd != IntPtr.Zero)
            //    hwnd = Win32.FindWindowEx(hwnd, IntPtr.Zero, null, "Notification Area"); // will this work on Windows in French?

            return hwnd;
        }
    }
}
