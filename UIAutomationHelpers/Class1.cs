using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Automation;

namespace UIAutomationHelpers
{
    public static class Objects
    {
        public static Rectangle GetRectAtPoint(System.Drawing.Point p)
        {
            Rectangle rect = new Rectangle();
            //System.Drawing.Point MousePoint = System.Windows.Forms.Cursor.Position;
           AutomationElement element = AutomationElement.FromPoint(new System.Windows.Point(p.X, p.Y));
            if (element != null)
            {
                var winRect = element.Current.BoundingRectangle;
                rect = new Rectangle(
                    x: (int)winRect.Left,
                    y: (int)winRect.Top,
                    width: (int)winRect.Width,
                    height: (int)winRect.Height);
            }
            element.GetCurrentPattern(ScrollPattern.Pattern);
            return rect;

        }
        static object _lock = new object();
        public static bool IsWindowScrollable(IntPtr hWnd)
        {
            if (hWnd == IntPtr.Zero)
                return false;

            lock (_lock)
            {

                AutomationElementCollection buttonCollection = AutomationElement.FromHandle(hWnd)?.FindAll(TreeScope.Descendants, new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.ScrollBar));
                if (buttonCollection.Count == 0)
                    return false;
                bool isVertical = buttonCollection[0].Current.AutomationId.StartsWith("Vertical")
                    ;
                return buttonCollection.Count != 0;
                
            }
        }

        public class UIAutomationScrollInfo
        {
            public object ScrollPattern { get; set; }
            public bool HasHorizontalScrollbar { get; set; }
            public bool HasVerticalScrollbar { get; set; }
        }

        public static UIAutomationScrollInfo GetScrollPattern(IntPtr hWnd)
        {
            AutomationElement aeForm = null;
            UIAutomationScrollInfo result = new UIAutomationScrollInfo();

            try
            {
                aeForm = AutomationElement.FromHandle(hWnd);
            }
            catch
            {
                return null;
            }

            object objScrollPattern;
            bool scrollSuccess = aeForm.TryGetCurrentPattern(ScrollPattern.Pattern, out objScrollPattern);
            if (scrollSuccess)
            {
                var scrollPattern = objScrollPattern as ScrollPattern;
                result.ScrollPattern = scrollPattern;

                if (scrollPattern.Current.VerticallyScrollable && (scrollPattern.Current.VerticalViewSize <= 99.0 && scrollPattern.Current.VerticalViewSize > 1.0))
                {
                    result.HasVerticalScrollbar = true;
                }
                if (scrollPattern.Current.HorizontallyScrollable && (scrollPattern.Current.HorizontalViewSize <= 99.0 && scrollPattern.Current.HorizontalViewSize > 1.0))
                {
                    result.HasHorizontalScrollbar = true;
                }

                return result;
            }
            return null;
        }

        public static bool IsWindowScrollable2(IntPtr hWnd)
        {
            AutomationElement aeForm = null;

            try
            {
                aeForm = AutomationElement.FromHandle(hWnd);
            }
            catch
            {
                return false;
            }

            object objScrollPattern;
            bool scrollSuccess = aeForm.TryGetCurrentPattern(ScrollPattern.Pattern, out objScrollPattern);
            if (scrollSuccess)
            {
                var scrollPattern = objScrollPattern as ScrollPattern;
                if (scrollPattern.Current.VerticallyScrollable)
                {
                    if (scrollPattern.Current.VerticalViewSize >= 99.0 || scrollPattern.Current.VerticalViewSize < 1.0)
                        return false;
                    try
                    {
                        scrollPattern.SetScrollPercent(scrollPattern.Current.HorizontalScrollPercent, 50);
                    }
                    catch { return false; }
                    return true;
                }
            }
            AutomationElementCollection buttonCollection = aeForm.FindAll(TreeScope.Children, new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.ScrollBar));
            
            if (buttonCollection.Count != 0)
            {
                RangeValuePattern range;
                object objPattern;
                bool success = buttonCollection[0].TryGetCurrentPattern(RangeValuePattern.Pattern, out objPattern);
                if (!success)
                    return false;
                if (objPattern is RangeValuePattern && (objPattern as RangeValuePattern).Current.IsReadOnly)
                    return false;
                var currentRangePattern = (objPattern as RangeValuePattern).Current;
                double max = currentRangePattern.Value + currentRangePattern.LargeChange;
                if (max > currentRangePattern.Maximum)
                    max = currentRangePattern.Maximum;
                (objPattern as RangeValuePattern).SetValue(max);
            }
            return buttonCollection.Count != 0;
        }

        public static bool TryScrollToPercent(UIAutomationScrollInfo patternInfo, int percent, bool verticalScroll)
        {
            var scrollPattern = patternInfo.ScrollPattern as ScrollPattern;
            if (scrollPattern == null)
                return false;

            if (scrollPattern.Current.VerticallyScrollable && verticalScroll)
            {
                if (scrollPattern.Current.VerticalViewSize >= 99.0 || scrollPattern.Current.VerticalViewSize < 1.0)
                    return false;
                try
                {
                    scrollPattern.SetScrollPercent(scrollPattern.Current.HorizontalScrollPercent, percent);
                }
                catch { return false; }
                return true;
            }
            else if (scrollPattern.Current.HorizontallyScrollable && !verticalScroll)
            {
                if (scrollPattern.Current.HorizontalViewSize >= 99.0 || scrollPattern.Current.HorizontalViewSize < 1.0)
                    return false;
                try
                {
                    scrollPattern.SetScrollPercent(percent, scrollPattern.Current.VerticalScrollPercent);
                }
                catch { return false; }
            }

            return true;
        }

        //public bool ScrollByValue(int scrollValue)
        //{
        //    RangeValuePattern rcpattern = (RangeValuePattern)aeButton.GetCurrentPattern(RangeValuePattern.Pattern);
        //    rcpattern.SetValue(50.00);
        //    //rcpattern.Current.Maximum;
        //    //rcpattern.Current.SmallChange;
        //    //rcpattern.Current.Value;
        //    //rcpattern.Current.LargeChange;
        //    return true;
        //}
    }
}
