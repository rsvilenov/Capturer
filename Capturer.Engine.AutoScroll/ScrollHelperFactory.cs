using System;
using System.Collections.Generic;
using System.Linq;
using Capturer.Engine.ScreenAnalyzer.Shared.Enums;
using Capturer.Engine.ScreenAnalyzer.Shared.DTO;
using Capturer.Engine.AutoScroll.Shared.Interfaces;

namespace Capturer.Engine.AutoScroll
{
    public class ScrollHelperFactory
    {
        private static Dictionary<ScrollbarAccessType, Type> _scrollHelpers = new Dictionary<ScrollbarAccessType, Type>
        {
            {ScrollbarAccessType.None,  null},
            {ScrollbarAccessType.ScrollPattern,  typeof(UIAutomationScrollHelper)},
            {ScrollbarAccessType.WindowMessage,  typeof(WMScrollHelper)}
        };

        public static IScrollHelper GetScrollHelper(ScrollbarInfo sbInfo)
        {
            var helperType = _scrollHelpers.
                Where(h => h.Key == sbInfo.AccessType).
                Select(h => h.Value).
                SingleOrDefault();

            IScrollHelper instance = null;
            if (helperType != null)
            {
                instance = (IScrollHelper)Activator.CreateInstance(helperType);
                if (instance != null)
                {
                    instance.SetParameters(sbInfo.AccessHandleObject, sbInfo.Orientation);
                }
            }
            return instance;
        }
    }
}
