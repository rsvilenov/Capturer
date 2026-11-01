using System;
using WinApiHelpers;
using Shared;
using Capturer.Engine.ScreenAnalyzer.Shared.DTO;
using Capturer.Engine.ScreenAnalyzer.Shared.Enums;
using Capturer.Engine.AutoScroll.Shared.Interfaces;

namespace Capturer.Engine.AutoScroll
{
    public class WMScrollHelper : ObjectWithSuppressableExceptions, IScrollHelper
    {
        IntPtr _hWnd;
        ScrollbarOrientation _orientation;

        public ScrollAreaInfo GetScrollAreaInfo()
        {
            return null;
        }

        public void ScrollByStep(double step)
        {
           int result = WindowApis.Win32.SendMessage(_hWnd, (int)Win32Enums.ScrollMessages.WM_VSCROLL, Win32Constants.SB_PAGEDOWN, 0);
           
        }

        public void ScrollToBeginning()
        {
            //WindowApis.Win32.SendMessage(_hWnd, (int)Win32Enums.ScrollMessages.WM_VSCROLL, Win32Constants.SB_PAGETOP, 0);
        }

        public void SetParameters(object accessHandleObject, ScrollbarOrientation orientation)
        {
            _hWnd = (IntPtr)accessHandleObject;
            if (!(accessHandleObject is IntPtr))
            {
                var exception = new ScrollHelperException(new ArgumentException("accessHandleObject is not IntPtr"));
                base.HandleException(exception);
            }
            _orientation = orientation;
        }
    }
}
