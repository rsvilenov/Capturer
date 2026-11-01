using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WinApiHelpers
{
    public static class Win32Enums
    {
        public enum HookType : int
        {
            WH_JOURNALRECORD = 0,
            WH_JOURNALPLAYBACK = 1,
            WH_KEYBOARD = 2,
            WH_GETMESSAGE = 3,
            WH_CALLWNDPROC = 4,
            WH_CBT = 5,
            WH_SYSMSGFILTER = 6,
            WH_MOUSE = 7,
            WH_HARDWARE = 8,
            WH_DEBUG = 9,
            WH_SHELL = 10,
            WH_FOREGROUNDIDLE = 11,
            WH_CALLWNDPROCRET = 12,
            WH_KEYBOARD_LL = 13,
            WH_MOUSE_LL = 14
        }

        // https://msdn.microsoft.com/en-us/library/windows/desktop/ms646280(v=vs.85).aspx
        public enum KeyboardMessages
        {
            WM_KEYDOWN = 0x0100,
            WM_KEYUP = 0x0101
        }

        public enum MouseMessages
        {
            WM_LBUTTONDOWN = 0x0201,
            WM_LBUTTONUP = 0x0202,
            WM_MOUSEMOVE = 0x0200,
            WM_MOUSEWHEEL = 0x020A,
            WM_MOUSEHWHEEL = 0x020E,
            WM_RBUTTONDOWN = 0x0204,
            WM_RBUTTONUP = 0x0205
        }

        public enum ScrollMessages
        {
            /// <summary>
            /// Horizontal scroll
            /// </summary>
            WM_SCROLL = 276,
            /// <summary>
            /// Vertical scroll
            /// </summary>
            WM_VSCROLL = 277 
        }

        public enum WindowMessages : uint
        {
            WM_DPICHANGED = 0x02E0,
            WM_SIZING = 0x0214,
            WM_MOVING = 0x0216,
            WM_ENTERSIZEMOVE = 0x0231,
            WM_EXITSIZEMOVE = 0x0232,
            WM_WINDOWPOSCHANGING = 0x0046,
            WM_WINDOWPOSCHANGED = 0x0047
        }
    }
}
