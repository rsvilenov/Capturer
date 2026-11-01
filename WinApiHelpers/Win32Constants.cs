using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WinApiHelpers
{
    public static class Win32Constants
    {
        // these are used as wParam for SendMessage with WM_SCROLL
        //Scroll page up
        //SendMessage(textBox1.Handle, WM_VSCROLL,(IntPtr)SB_PAGEUP,IntPtr.Zero);
        //Scroll page down
        //SendMessage(textBox1.Handle, WM_VSCROLL,(IntPtr)SB_PAGEDOWN,IntPtr.Zero);

        public const int SB_LINEUP = 0; // Scrolls one line up
        public const int SB_LINELEFT = 0;// Scrolls one cell left
        public const int SB_LINEDOWN = 1; // Scrolls one line down
        public const int SB_LINERIGHT = 1;// Scrolls one cell right
        public const int SB_PAGEUP = 2; // Scrolls one page up
        public const int SB_PAGELEFT = 2;// Scrolls one page left
        public const int SB_PAGEDOWN = 3; // Scrolls one page down
        public const int SB_PAGERIGTH = 3; // Scrolls one page right
        public const int SB_PAGETOP = 6; // Scrolls to the upper left
        public const int SB_LEFT = 6; // Scrolls to the left
        public const int SB_PAGEBOTTOM = 7; // Scrolls to the upper right
        public const int SB_RIGHT = 7; // Scrolls to the right
        public const int SB_ENDSCROLL = 8; // Ends scroll
    }
}
