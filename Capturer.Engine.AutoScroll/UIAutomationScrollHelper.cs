using System;
using System.Windows.Automation;
using Shared;
using Capturer.Engine.ScreenAnalyzer.Shared.Enums;
using Capturer.Engine.ScreenAnalyzer.Shared.DTO;
using Capturer.Engine.AutoScroll.Shared.Interfaces;

namespace Capturer.Engine.AutoScroll
{
    /// <summary>
    /// Perform scrolls using Microsoft UI Automation
    /// </summary>
    public class UIAutomationScrollHelper : ObjectWithSuppressableExceptions, IScrollHelper
    {
        ScrollPattern _scrollPattern;
        ScrollbarOrientation _orientation;

        private void ScrollToPercent(double percent)
        {
            if (_orientation == ScrollbarOrientation.Vertical && _scrollPattern.Current.VerticallyScrollable)
            {
                if (_scrollPattern.Current.VerticalViewSize >= 99.0 || _scrollPattern.Current.VerticalViewSize < 1.0)
                    return;
                try
                {
                    _scrollPattern.SetScrollPercent(_scrollPattern.Current.HorizontalScrollPercent, percent);
                }
                catch (Exception ex)
                {
                    base.HandleException(new ScrollHelperException("Vertical scroll failed", ex));
                }
            }
            else if (_orientation == ScrollbarOrientation.Horizontal && _scrollPattern.Current.HorizontallyScrollable)
            {
                if (_scrollPattern.Current.HorizontalViewSize >= 99.0 || _scrollPattern.Current.HorizontalViewSize < 1.0)
                    return;
                try
                {
                    _scrollPattern.SetScrollPercent(percent, _scrollPattern.Current.HorizontalScrollPercent);
                }
                catch (Exception ex)
                {
                    base.HandleException(new ScrollHelperException("Horizontal scroll failed", ex));
                }
            }
        }

        /// <summary>
        /// <see cref="IScrollHelper.ScrollByStep(double)"/>
        /// </summary>
        /// <param name="step"></param>
        public void ScrollByStep(double step)
        {
            double newPercent = 0;
            if (_orientation == ScrollbarOrientation.Horizontal)
                newPercent = _scrollPattern.Current.HorizontalScrollPercent + step;
            else
                newPercent = _scrollPattern.Current.VerticalScrollPercent + step;

            if (newPercent > 100)
                newPercent = 100;

            ScrollToPercent(newPercent);
        }

        /// <summary>
        /// <see cref="IScrollHelper.ScrollToBeginning"/>
        /// </summary>
        public void ScrollToBeginning()
        {
            ScrollToPercent(0);
        }

        /// <summary>
        /// <see cref="IScrollHelper.SetParameters(object, ScrollbarOrientation)"/>
        /// </summary>
        public void SetParameters(object accessHandleObject, ScrollbarOrientation orientation)
        {
            _scrollPattern = accessHandleObject as ScrollPattern;
            if (_scrollPattern == null)
            {
                var exception = new ScrollHelperException(new ArgumentException("UIAutomationPattern is null or its type is not ScrollPattern"));
                base.HandleException(exception);
            }
            _orientation = orientation;
        }

        /// <summary>
        /// <see cref="IScrollHelper.GetScrollAreaInfo"/>
        /// </summary>
        public ScrollAreaInfo GetScrollAreaInfo()
        {
            var scrollAreaInfo = new ScrollAreaInfo();
            // we can't determine the step, so we will just hard code it
            scrollAreaInfo.ScrollStepSize = 25;
            scrollAreaInfo.MaxScrollValue = 100;
            
            return scrollAreaInfo;
        }
    }
}
