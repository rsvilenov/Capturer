using Capturer.Engine.ScreenAnalyzer.Shared.DTO;
using Capturer.Engine.ScreenAnalyzer.Shared.Enums;

namespace Capturer.Engine.AutoScroll.Shared.Interfaces
{
    /// <summary>
    /// Represents a type that can perform automatic scrolling of third-party windows
    /// </summary>
    public interface IScrollHelper
    {
        /// <summary>
        /// Sets the parameters for the operation
        /// </summary>
        /// <param name="accessHandleObject">An object that will be used to access the scrollbar</param>
        /// <param name="orientation">Horizontal or Vertical</param>
        void SetParameters(object accessHandleObject, ScrollbarOrientation orientation);

        /// <summary>
        /// Resets the scrollbar position
        /// </summary>
        void ScrollToBeginning();

        /// <summary>
        /// Scroll by a step
        /// </summary>
        /// <param name="step"></param>
        void ScrollByStep(double step);

        /// <summary>
        /// Gets the properties for this scroll area
        /// </summary>
        /// <returns></returns>
        ScrollAreaInfo GetScrollAreaInfo();
    }
}
