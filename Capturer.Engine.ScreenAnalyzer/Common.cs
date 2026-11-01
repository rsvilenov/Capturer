using System;
using System.Collections.Generic;
using System.Drawing;

namespace Capturer.Engine.ScreenAnalyzer
{
    internal static class Common
    {
        internal static Rectangle GetRectangleIntersectedWithParent(IntPtr hWnd, int levels)
        {
            Rectangle rect = WinApiHelpers.Window.GetWindowRectangle(hWnd);

            for (int i = 0; i < levels; i++)
            {
                var parentHWnd = WinApiHelpers.Window.GetParentWindow(hWnd);
                if (parentHWnd != IntPtr.Zero)
                {
                    Rectangle parentRect = WinApiHelpers.Window.GetWindowRectangle(parentHWnd);
                    rect.Intersect(parentRect);
                }
                hWnd = parentHWnd;
            }

            return rect;
        }

        public static HashSet<string> WindowsToSkip = new HashSet<string>()
        {
            "VisualStudioGlowWindow", // vs 2015 invisible border windows
            "MSO_BORDEREFFECT_WINDOW_CLASS", // ms office invisible border windows
            "CtrlNotifySink", // common wrapper window in native windows apps (especially in microsoft ones)
            "SHELLDLL_DefView", // one of the windows in Explorer where we can't easily detect the scrollbars
            "NamespaceTreeControl", // another window with hard scrollbar detection
        };

        public static HashSet<string> WindowsWithScrolls = new HashSet<string>()
        {
            "_WwG", // ms office applications main window
            "Chrome_RenderWidgetHostHWND",
            "MozillaWindowClass"
        };
    }
}
