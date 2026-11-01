using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using System.Threading;
using System.Diagnostics;
using System.Collections.ObjectModel;
using Capturer.Engine.ScreenAnalyzer.Shared.DTO;
using Capturer.Engine.ScreenAnalyzer.Shared.Interfaces;
using Capturer.Engine.ScreenAnalyzer.Shared.Enums;
using WinApiHelpers;
using System.Windows.Forms;

namespace Capturer.Engine.ScreenAnalyzer
{
    public class PassiveScreenAnalyzer : IScreenAnalyzer
    {
        private List<WindowInfo> _windowInfos;
        private bool _ready;

        public double AnalysisTime { get; set; }

        public Rectangle _taskbarRectangle;
        public IntPtr _taskbarHWND;

        public void Analyze()
        {
            _ready = false;
            // TODO: replace this with Task.Run if we are going to use .net >=4.5
            (new Thread(AnalyzeWorker)).Start();
        }

        private void AnalyzeWorker()
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            try
            {
                //_taskbarHWND = WinApiHelpers.Windows.GetTaskbarWindow();
                //_taskbarRectangle = WinApiHelpers.Window.GetWindowRectangle(_taskbarHWND);

                
                var infos = GetVisibleWindows();

                var taskbarRectangles = infos.
                    Where(i => i.WindowType == WindowTypes.TaskBar).
                    Select(i => i.Rectangle);

                _windowInfos = new List<WindowInfo>();
                foreach (var info in infos)
                {
                    // some programs, visual studio included, displays its fullscreen window below the taskbar
                    CutOutRectanglePartsIntersectingWithTaskbar(taskbarRectangles, info);

                    info.Pid = WinApiHelpers.Window.GetPIDByHWND(info.HWND);

                    _windowInfos.Add(info);
                }

                // allow processing while we are detecting the scrollbars
                _ready = true;
                sw.Stop();
                this.AnalysisTime = sw.ElapsedMilliseconds;
                //Thread.Sleep(3000);
                DetectScrollbars();

            }
            finally
            {
                
            }
        }

        private static void CutOutRectanglePartsIntersectingWithTaskbar(IEnumerable<Rectangle> taskbarRectangles, WindowInfo info)
        {
            var intersectingTaskbarRectangle = taskbarRectangles.
                Where(tr => tr.IntersectsWith(info.Rectangle)).
                FirstOrDefault();

            // if we have an intersection and the window is not entirely contained within the taskbar,
            // cut out the intersecting part of the window's rectangle
            if (!intersectingTaskbarRectangle.IsEmpty && !intersectingTaskbarRectangle.Contains(info.Rectangle))
            {
                var screen = DisplayMonitors.GetByWindowHandle(info.HWND);
                var inter = Rectangle.Intersect(info.Rectangle, screen.DPICalibratedWorkingArea);
                info.Rectangle = inter;
            }
        }

        public WindowInfo GetWindowInfoFromPoint(Point p, Func<IEnumerable<WindowInfo>, IEnumerable<WindowInfo>> filterFunction = null)
        {
            if (!_ready)
                return null;

            var collectionToSearch = _windowInfos as IEnumerable<WindowInfo>;
            if (filterFunction != null)
            {
                collectionToSearch = filterFunction(collectionToSearch);
            }
            //switch (level)
            //{
            //    case ScreenAnalyzerSensitivity.Low:
            //        collectionToSearch = collectionToSearch.Where(wi => wi.WindowType == WindowTypes.TopLevelWindow);
            //        break;
            //}

            var firstMatch = collectionToSearch.
                    Where(w => w.Rectangle.Contains(p)).
                    FirstOrDefault();

            if (firstMatch != null && firstMatch.WindowType == WindowTypes.TitleBar)
            {
                firstMatch = collectionToSearch.First(w => w.WindowType != WindowTypes.TitleBar && w.HWND == firstMatch.HWND);
                //if (firstMatch.HWND == _taskbarHWND)
                //{
                //    firstMatch = collectionToSearch.
                //    Where(w => w.Rectangle.Contains(p) && w.HWND != _taskbarHWND).
                //    FirstOrDefault();
                //}
            }

            return firstMatch;
        }

        private IEnumerable<WindowInfo> GetVisibleWindows()
        {
            //var children = GetAllChildHandles(new IntPtr(461270));
            var allWindows = WinApiHelpers.Windows.GetAllWindows(true);

            // windows should be ordered by z-index already, top windows first.

            // We will load the regions that each window occupies

            var windowWithRegions = allWindows.
                Where(hWnd => !WinApiHelpers.Window.IsCloaked(hWnd)).
                Select(hwnd => new
                {
                    HWND = hwnd,
                    WindowClassName = WinApiHelpers.Window.GetWindowClassName(hwnd)
                }).
                Where(wi => !Common.WindowsToSkip.Any(className => className == wi.WindowClassName)).
                Select(wi => new WindowInfo
                {
                    HWND = wi.HWND,
                    WindowClassName = wi.WindowClassName,
                    Rectangle = Common.GetRectangleIntersectedWithParent(wi.HWND, 5),
                    WindowType = GetTopLevelWindowType(wi.HWND, wi.WindowClassName)
                }).
                // some windows e.g. those with with class "Internet Explorer_Hidden" has empty rectangles
                Where(wi => !wi.Rectangle.IsEmpty);
            
                
            var visibleWindows = new List<WindowInfo>();
            
            // Then we will check the intersection of each window against all windows with lower z-order.
            int zOrder = 0;
            foreach (var windowWithRegion in windowWithRegions)
            {
                var rectanglesOnTop = windowWithRegions.
                            Take(zOrder).
                            Select(wi => wi.Rectangle);
                windowWithRegion.OverlapState = DetectOverlap(windowWithRegion.Rectangle, rectanglesOnTop);
                
                if (windowWithRegion.OverlapState != RectangleOverlapState.Full)
                {
                    //// get the titlebar
                    //windowWithRegion.TitleBarArea = WinApiHelpers.Window.GetTitleBarRectangle(windowWithRegion.HWND);
                    //// insert a fictive window info, representing the titlebar, so that it would be matched first
                    //WindowInfo titleBarWindowInfo = new WindowInfo
                    //{
                    //    HWND = windowWithRegion.HWND,
                    //    Rectangle = new Rectangle
                    //    {
                    //        Height = windowWithRegion.TitleBarArea.Height,
                    //        Width = windowWithRegion.Rectangle.Width,
                    //        X = windowWithRegion.TitleBarArea.X,
                    //        Y = windowWithRegion.TitleBarArea.Y,
                    //    },
                    //    WindowType = WindowTypes.TitleBar
                    //};
                    //visibleWindows.Add(titleBarWindowInfo);

                    //windowWithRegion.TitleBarArea = WinApiHelpers.Window.GetTitleBarRectangle(windowWithRegion.HWND);
                    // insert a fictive window info, representing the titlebar, so that it would be matched first
                    
                    WindowInfo titleBarWindowInfo = new WindowInfo
                    {
                        HWND = windowWithRegion.HWND,
                        Rectangle = new Rectangle
                        {
                            Height = SystemInformation.CaptionHeight,
                            Width = windowWithRegion.Rectangle.Width,
                            X = windowWithRegion.Rectangle.X,
                            Y = windowWithRegion.Rectangle.Y,
                        },
                        WindowType = WindowTypes.TitleBar
                    };
                    

                    // get the children
                    var childWindowsWithRegions = WinApiHelpers.Windows.GetAllChildHandles(windowWithRegion.HWND, false).
                        Where(hwnd => WinApiHelpers.Window.IsVisible(hwnd)).
                        AsParallel().
                        Select(hwnd => new WindowInfo
                        {
                            HWND = hwnd,
                            Rectangle = WinApiHelpers.Window.GetWindowRectangle(hwnd),
                            WindowClassName = WinApiHelpers.Window.GetWindowClassName(hwnd),
                            WindowType = WindowTypes.ChildWindow
                        }).
                        // filter out the wrapper controls of the default windows applications like explorer
                        Where(wi => !Common.WindowsToSkip.Any(className => className == wi.WindowClassName)).
                        // let the smallest controls go first in the list as they are more likely
                        // not to be containers. These controls will be matched first by GetWindowInfoFromPoint(p)
                        OrderBy(x => x.Rectangle.Height * x.Rectangle.Width).ToList();

                    if (windowWithRegion.WindowType != WindowTypes.TaskBar)
                    {
                        var titleLst = new List<WindowInfo>();
                        titleLst.Add(titleBarWindowInfo);
                        childWindowsWithRegions = titleLst.Concat(childWindowsWithRegions).ToList();
                    }


                    foreach (var childWindowWithRegion in childWindowsWithRegions)
                    {
                        var childRectanglesOnTop = windowWithRegions.
                            Take(zOrder).
                            Select(wi => wi.Rectangle);
                        var childOverlapState = DetectOverlap(childWindowWithRegion.Rectangle, childRectanglesOnTop);
                        
                        if (childOverlapState != RectangleOverlapState.Full)
                        {
                            if (childWindowWithRegion.WindowType != WindowTypes.TitleBar)
                            {
                                Rectangle intersectedRect = Common.GetRectangleIntersectedWithParent(childWindowWithRegion.HWND, 5);
                                childWindowWithRegion.Rectangle = intersectedRect;
                            }
                            childWindowWithRegion.OverlapState = childOverlapState;
                            visibleWindows.Add(childWindowWithRegion);
                        }
                    }
                }

                if (windowWithRegion.OverlapState != RectangleOverlapState.Full)
                {
                    visibleWindows.Add(windowWithRegion);
                }

                zOrder++;
            }

            return visibleWindows;
        }

        private WindowTypes GetTopLevelWindowType(IntPtr hWnd, string windowClassName)
        {
            if (windowClassName == "Shell_SecondaryTrayWnd" || windowClassName == "Shell_TrayWnd")
                return WindowTypes.TaskBar;
            if (WinApiHelpers.Window.IsTopLevelWindow(hWnd))
                return WindowTypes.TopLevelWindow;
            return WindowTypes.ChildWindow;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="rect">The rectangle to be checked for overlapping</param>
        /// <param name="rectLst">A collection of rectangles to be checked against</param>
        /// <returns></returns>
        private RectangleOverlapState DetectOverlap(Rectangle rect, IEnumerable<Rectangle> rectLst)
        {
            var intersections = rectLst.
                            Select(w => Rectangle.Intersect(rect, w)).
                            // filter out the empty rectangles (where no intersection were detected)
                            Where(r => r != Rectangle.Empty);

            var overlapState = RectangleOverlapState.None;
            if (intersections.Any())
            {
                // check if there is a rectangle which is equal in size or bigger than the current one
                bool isFullyOverlapped = intersections.Any(w => w.Height >= rect.Height && w.Width >= rect.Width);
                if (isFullyOverlapped)
                {
                    overlapState = RectangleOverlapState.Full;
                }
                else
                {
                    overlapState = RectangleOverlapState.Partial;
                }
            }

            return overlapState;
        }

        private void DetectScrollbars()
        {
           
            foreach (var wnd in _windowInfos)
            {
                wnd.ScrollbarInfos = new ObservableCollection<ScrollbarInfo>();
                // skip windows with already discovered scrollbars
                //if (wnd.ScrollbarInfo != null)
                //    continue;
                // skip partially visible windows
                // TODO: some windows get reported as Partially overlapped, while they are not
                //if (wnd.OverlapState == RectangleOverlapState.Partial)
                //   continue;

                // try to match by className first
                //if (Common.WindowsWithScrolls.Any(c => c == wnd.WindowClassName))
                //{
                //    wnd.ScrollbarInfo = new ScrollbarInfo
                //    {
                //        HasHorizontalScrollBar = true,
                //        HasVerticalScrollBar = true,
                //        HScrollAccessType = ScrollbarAccessType.WindowMessage,
                //        VScrollAccessType = ScrollbarAccessType.WindowMessage
                //    };
                //    continue;
                //}

                // then try UIAutomation
                var automationInfo = UIAutomationHelpers.Objects.GetScrollPattern(wnd.HWND);
                if (automationInfo != null)
                {
                    if (automationInfo.HasHorizontalScrollbar)
                    {
                        var horizontalInfo = new ScrollbarInfo
                        {
                            AccessType = ScrollbarAccessType.ScrollPattern,
                            AccessHandleObject = automationInfo.ScrollPattern,
                            Orientation = ScrollbarOrientation.Horizontal
                        };
                        wnd.ScrollbarInfos.Add(horizontalInfo);
                    }

                    if (automationInfo.HasVerticalScrollbar)
                    {
                        var verticalInfo = new ScrollbarInfo
                        {
                            AccessType = ScrollbarAccessType.ScrollPattern,
                            AccessHandleObject = automationInfo.ScrollPattern,
                            Orientation = ScrollbarOrientation.Vertical
                        };
                        wnd.ScrollbarInfos.Add(verticalInfo);
                    }

                    continue;
                }

                // this gives a lot of false-positives
                //// test with GetScrollBarInfo
                //if (wnd.WindowType == WindowTypes.ChildWindow)
                //{
                //    bool hasVertScroll2 = WinApiHelpers.Window.TestWindowForScroll(wnd.HWND, true);
                //    bool hasHorizScroll2 = WinApiHelpers.Window.TestWindowForScroll(wnd.HWND, true);
                //    if (hasVertScroll2 || hasHorizScroll2)
                //    {
                //        wnd.ScrollbarInfo = new ScrollbarInfo
                //        {
                //            HasHorizontalScrollBar = hasHorizScroll2,
                //            HasVerticalScrollBar = hasVertScroll2,
                //            HScrollAccessType = ScrollbarAccessType.WindowMessage,
                //            VScrollAccessType = ScrollbarAccessType.WindowMessage
                //        };
                //        continue;
                //    }
                //}

                // finally, search for WS_HSCROLL and WS_VSCROLL
                bool hasVertScroll = WinApiHelpers.Window.TestWindowForScrollbarStyles(wnd.HWND, true);
                bool hasHorizScroll = WinApiHelpers.Window.TestWindowForScrollbarStyles(wnd.HWND, false);
                if (hasVertScroll)
                {
                    var verticalInfo = new ScrollbarInfo
                    {
                        AccessType = ScrollbarAccessType.WindowMessage,
                        AccessHandleObject = wnd.HWND, // maybe the scrollbarinfo stcuture from winapi
                        Orientation = ScrollbarOrientation.Vertical
                    };
                    wnd.ScrollbarInfos.Add(verticalInfo);
                }

                if (hasHorizScroll)
                {
                    var horizontalInfo = new ScrollbarInfo
                    {
                        AccessType = ScrollbarAccessType.WindowMessage,
                        AccessHandleObject = wnd.HWND, // maybe the scrollbarinfo stcuture from winapi
                        Orientation = ScrollbarOrientation.Horizontal
                    };
                    wnd.ScrollbarInfos.Add(horizontalInfo);
                }
            }
        }
    }
}
