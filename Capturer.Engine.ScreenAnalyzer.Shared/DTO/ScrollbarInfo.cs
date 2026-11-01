using System.Drawing;
using Capturer.Engine.ScreenAnalyzer.Shared.Enums;

namespace Capturer.Engine.ScreenAnalyzer.Shared.DTO
{
    public class ScrollbarInfo
    {
        /// <summary>
        /// Horizontal or Vertical
        /// </summary>
        public ScrollbarOrientation Orientation { get; set; }

        /// <summary>
        /// The type of access to the scrollbar
        /// </summary>
        public ScrollbarAccessType AccessType { get; set; }

        /// <summary>
        /// The object that should be used to access the scrollbar.
        /// It could be a UI Automation Pattern, a hWnd, some kind of WinApi structure, etc.
        /// </summary>
        public object AccessHandleObject { get; set; }

        /// <summary>
        /// The actual area that gets scrolled (excluding the scrollbar itself)
        /// </summary>
        public Rectangle InnerRectangle { get; set; }
    }
}
