using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static WinApiHelpers.WindowApis.Win32;

namespace WinApiHelpers
{
    /// <summary>
    /// https://www.code4copy.com/csharp/getting-window-rect-using-handle-on-windows-8-and-other/
    /// </summary>
    public static class Window
    {
        [DllImport("user32.dll"), SuppressUnmanagedCodeSecurity]
        static extern IntPtr WindowFromPoint(System.Drawing.Point p);

        public static IntPtr GetWindowFromPoint(System.Drawing.Point p)
        {
            return WindowFromPoint(p);
        }

        [DllImport("user32.dll", ExactSpelling = true, CharSet = CharSet.Auto)]
        static extern IntPtr GetParent(IntPtr hWnd);


        public static IntPtr GetParentWindow(IntPtr hWnd)
        {
            return GetParent(hWnd);
        }


        [DllImport("user32.dll")]
        static extern bool GetClientRect(IntPtr hWnd, out Win32Structures.RECT lpRect);

        public static Rectangle GetClientArea(IntPtr hWnd)
        {
            Win32Structures.RECT r;
            bool success = GetClientRect(hWnd, out r);
            //if (!success)

            //    throw new WinApiException("GetClientRect failed");

            return r.ToRectangle();
        }

        [DllImport("user32.dll")]
        static extern int GetClassName(IntPtr hWnd, StringBuilder buf, int nMaxCount);

        public static string GetWindowClassName(IntPtr hWnd)
        {
            StringBuilder className = new StringBuilder(256);
            GetClassName(hWnd, className, className.Capacity);
            return className.ToString();
        }

        [DllImport("user32.dll", SetLastError = true)]
        static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        // When you don't want the ProcessId, use this overload and pass IntPtr.Zero for the second parameter
        [DllImport("user32.dll")]
        static extern uint GetWindowThreadProcessId(IntPtr hWnd, IntPtr ProcessId);

        public static int GetPIDByHWND(IntPtr hWnd)
        {
            uint processID = 0;
            GetWindowThreadProcessId(hWnd, out processID);
            return (int)processID; // this should be safe
        }

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool IsWindowVisible(IntPtr hWnd);

        public static bool IsVisible(IntPtr hWnd)
        {
            return IsWindowVisible(hWnd);
        }

        [DllImport(@"dwmapi.dll")]
        private static extern int DwmGetWindowAttribute(IntPtr hwnd, int dwAttribute, out Win32Structures.RECT pvAttribute, int cbAttribute);

        [DllImport("user32.dll", SetLastError = false)]
        public static extern IntPtr GetDesktopWindow();

        [DllImport(@"user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool GetWindowRect(IntPtr hWnd, out Win32Structures.RECT lpRect);

        private static Rectangle GetWindowRect(IntPtr handle)
        {
            Win32Structures.RECT rect;
            bool success = GetWindowRect(handle, out rect);
            //if (!success)
            //    throw new WinApiException("GetClientRect failed");
            return rect.ToRectangle();
        }
        /// <summary>
        /// Get real window size, no matter whether Win XP, Win Vista, 7 or 8.
        /// </summary>
        public static Rectangle GetWindowRectangle(IntPtr handle)
        {
            if (Environment.OSVersion.Version.Major < 6)
            {
                return GetWindowRect(handle);
            }
            else
            {
                Rectangle rectangle;
                return DWMWA_EXTENDED_FRAME_BOUNDS(handle, out rectangle) ? rectangle : GetWindowRect(handle);
            }
        }

        private enum Dwmwindowattribute
        {
            DwmwaExtendedFrameBounds = 9
        }

        private static bool DWMWA_EXTENDED_FRAME_BOUNDS(IntPtr handle, out Rectangle rectangle)
        {
            Win32Structures.RECT rect;
            var result = DwmGetWindowAttribute(handle, (int)Dwmwindowattribute.DwmwaExtendedFrameBounds,
                out rect, Marshal.SizeOf(typeof(Win32Structures.RECT)));
            rectangle = rect.ToRectangle();
            return result >= 0;
        }

        [DllImport("dwmapi.dll")]
        static extern int DwmGetWindowAttribute(IntPtr hwnd, DWMWINDOWATTRIBUTE dwAttribute, out bool pvAttribute, int cbAttribute);

        enum DWMWINDOWATTRIBUTE : uint
        {
            NCRenderingEnabled = 1,
            NCRenderingPolicy,
            TransitionsForceDisabled,
            AllowNCPaint,
            CaptionButtonBounds,
            NonClientRtlLayout,
            ForceIconicRepresentation,
            Flip3DPolicy,
            ExtendedFrameBounds,
            HasIconicBitmap,
            DisallowPeek,
            ExcludedFromPeek,
            Cloak,
            Cloaked,
            FreezeRepresentation
        }

        [DllImport("dwmapi.dll")]
        public static extern int DwmGetWindowAttribute(IntPtr hwnd, int attr, ref int pvAttr, int attrSize);

        /// <summary>
        /// Checks if the window is cloacked
        /// </summary>
        /// <param name="hWnd"></param>
        /// <returns></returns>
        public static bool IsCloaked(IntPtr hWnd)
        {
            // http://stackoverflow.com/questions/32149880/how-to-identify-windows-10-background-store-processes-that-have-non-displayed-wi
            int value = 1;
            int hr = DwmGetWindowAttribute(hWnd, (int)DWMWINDOWATTRIBUTE.Cloaked, ref value, sizeof(int));
            if (hr != 0) // returns S_OK (which is zero) on success. Otherwise, it returns an HRESULT error code
            {
                value = 0;
            }
            return value != 0 ? true : false;
        }
        

        /// <summary>
        /// Retrieves the handle to the ancestor of the specified window. 
        /// </summary>
        /// <param name="hwnd">A handle to the window whose ancestor is to be retrieved. 
        /// If this parameter is the desktop window, the function returns NULL. </param>
        /// <param name="flags">The ancestor to be retrieved.</param>
        /// <returns>The return value is the handle to the ancestor window.</returns>
        [DllImport("user32.dll", ExactSpelling = true)]
        static extern IntPtr GetAncestor(IntPtr hwnd, GetAncestorFlags flags);

        enum GetAncestorFlags
        {
            /// <summary>
            /// Retrieves the parent window. This does not include the owner, as it does with the GetParent function. 
            /// </summary>
            GetParent = 1,
            /// <summary>
            /// Retrieves the root window by walking the chain of parent windows.
            /// </summary>
            GetRoot = 2,
            /// <summary>
            /// Retrieves the owned root window by walking the chain of parent and owner windows returned by GetParent. 
            /// </summary>
            GetRootOwner = 3
        }

        public static bool IsTopLevelWindow(IntPtr hWnd)
        {
            return GetAncestor(hWnd, GetAncestorFlags.GetRoot) == hWnd;
        }

        internal const int WM_GETTITLEBARINFOEX = 0x033F;
        internal const int CCHILDREN_TITLEBAR = 5;

        [StructLayout(LayoutKind.Sequential)]
        internal struct TITLEBARINFOEX
        {
            public int cbSize;
            public Rectangle rcTitleBar;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = CCHILDREN_TITLEBAR + 1)]
            public int[] rgstate;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = CCHILDREN_TITLEBAR + 1)]
            public Rectangle[] rgrect;
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        internal static extern IntPtr SendMessage(
                                          IntPtr hWnd,
                                          int uMsg,
                                          IntPtr wParam,
                                          ref TITLEBARINFOEX lParam);

        internal static TITLEBARINFOEX GetTitleBarInfoEx(IntPtr hWnd)
        {
            // Create and initialize the structure
            TITLEBARINFOEX tbi = new TITLEBARINFOEX();
            tbi.cbSize = Marshal.SizeOf(typeof(TITLEBARINFOEX));

            // Send the WM_GETTITLEBARINFOEX message
            SendMessage(hWnd, WM_GETTITLEBARINFOEX, IntPtr.Zero, ref tbi);

            // Return the filled-in structure
            return tbi;
        }

        public static Rectangle GetTitleBarRectangle(IntPtr hWnd)
        {
            var titleBarInfoEx = GetTitleBarInfoEx(hWnd);
            return titleBarInfoEx.rcTitleBar;
        }

        [DllImport("user32.dll", SetLastError = true, EntryPoint = "GetScrollBarInfo")]
        private static extern int GetScrollBarInfo(IntPtr hWnd, uint idObject, ref SCROLLBARINFO psbi);

        [StructLayout(LayoutKind.Sequential)]
        public struct SCROLLBARINFO
        {
            public int cbSize;
            public Rectangle rcScrollBar;
            public int dxyLineButton;
            public int xyThumbTop;
            public int xyThumbBottom;
            public int reserved;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)]
            public int[] rgstate;
        }

        private const uint OBJID_HSCROLL = 0xFFFFFFFA;
        private const uint OBJID_VSCROLL = 0xFFFFFFFB;
        private const uint OBJID_CLIENT = 0xFFFFFFFC;

        private const uint STATE_SYSTEM_INVISIBLE = 0x00008000;

        public static bool TestWindowForScroll(IntPtr hWnd, bool vertical)
        {
            // http://stackoverflow.com/questions/9751740/how-i-can-check-if-a-window-has-visible-scrollbars-using-his-hwnd
            SCROLLBARINFO psbi = new SCROLLBARINFO();
            psbi.cbSize = Marshal.SizeOf(psbi);

            int nResult = GetScrollBarInfo(hWnd, vertical ? OBJID_VSCROLL : OBJID_HSCROLL, ref psbi); // "this" is a scrollbar

            if (nResult == 0)
            {
                //int nLatError = GetLastError(); // in kernel32.dll
            }

            return psbi.rcScrollBar != Rectangle.Empty;
            //return !psbi.rgstate.Contains((int)STATE_SYSTEM_INVISIBLE);
        }

        [DllImport("user32.dll", SetLastError = true)]
        static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        public static bool TestWindowForScrollbarStyles(IntPtr hWnd, bool vertical)
        {
            int scrollStyle = vertical ? (int)WindowStyles.WS_VSCROLL : (int)WindowStyles.WS_HSCROLL;
            return (GetWindowLong(hWnd, (int)WindowLongFlags.GWL_STYLE) & scrollStyle) != 0;
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        static extern IntPtr SendMessage(IntPtr hWnd, UInt32 Msg, Int32 wParam, ref Point lParam);

        public static void ScrollWindowWithMouseWheel(IntPtr hwnd, Point p, bool vertical, int scrolls = -1)
        {
            int recommendedDelta = SystemInformation.MouseWheelScrollDelta;
            Win32Enums.MouseMessages message = vertical ? Win32Enums.MouseMessages.WM_MOUSEWHEEL : Win32Enums.MouseMessages.WM_MOUSEHWHEEL;
            SendMessage(hwnd, (int)Win32Enums.MouseMessages.WM_MOUSEWHEEL, (recommendedDelta * scrolls) << 16, ref p);
        }

        [DllImport("user32.dll")]
        public static extern bool SetForegroundWindow(IntPtr hWnd);
    }
}
