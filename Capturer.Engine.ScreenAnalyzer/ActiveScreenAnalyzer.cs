using System;
using System.Collections.Generic;
using System.Drawing;
using Capturer.Engine.ScreenAnalyzer.Shared.DTO;
using Capturer.Engine.ScreenAnalyzer.Shared.Interfaces;

namespace Capturer.Engine.ScreenAnalyzer
{
    public class ActiveScreenAnalyzer : IScreenAnalyzer
    {
        public double AnalysisTime
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        public WindowInfo GetWindowInfoFromPoint(Point p, Func<IEnumerable<WindowInfo>, IEnumerable<WindowInfo>> filterFunction = null)
        {
            IntPtr hWnd = WinApiHelpers.Window.GetWindowFromPoint(p);
            //Rectangle rect = WinApiHelpers.Window.GetWindowRectangle(hWnd);

            Rectangle rect = Common.GetRectangleIntersectedWithParent(hWnd, 5);


            //Rectangle clientRect = WinApiHelpers.Window.GetClientArea(hWnd);
            var wInfo = new WindowInfo { HWND = hWnd, Rectangle = rect };
            //wInfo.ClientArea = clientRect;

            return wInfo;
        }

        
    }
}
