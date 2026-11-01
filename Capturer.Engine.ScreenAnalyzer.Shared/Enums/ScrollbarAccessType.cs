using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Capturer.Engine.ScreenAnalyzer.Shared.Enums
{
    public enum ScrollbarAccessType
    {
        /// <summary>
        /// No access
        /// </summary>
        None,
        /// <summary>
        /// access the scrollbar by hWnd
        /// </summary>
        HWND,

        /// <summary>
        /// access the scrollbar via UI Automation's ScrollPattern
        /// </summary>
        ScrollPattern,

        /// <summary>
        /// access the scrollbar via WM_SCROLL
        /// </summary>
        WindowMessage,

        /// <summary>
        /// access the scrollbar via WM_MOUSEWHEEL
        /// </summary>
        MouseWheel
    }
}
