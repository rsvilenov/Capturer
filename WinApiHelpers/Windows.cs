using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace WinApiHelpers
{
    public static class Windows
    {
        //http://stackoverflow.com/questions/19867402/how-can-i-use-enumwindows-to-find-windows-with-a-specific-caption-title

        [DllImport("user32.dll")]
        private static extern bool EnumWindows(EnumWindowsProc enumProc, IntPtr lParam);

        [DllImport("user32")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool EnumChildWindows(IntPtr window, EnumWindowsProc callback, IntPtr lParam);

        // Delegate to filter which windows to include 
        public delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

        [DllImport("user32.dll", EntryPoint = "FindWindowEx",
  CharSet = CharSet.Auto)]
        static extern IntPtr FindWindowEx(IntPtr hwndParent,
  IntPtr hwndChildAfter, string lpszClass, string lpszWindow);

        // For Windows Mobile, replace user32.dll with coredll.dll
        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr FindWindow(string lpClassName, string lpWindowName);


        public static IntPtr GetTaskbarWindow()
        {
            return FindWindow("Shell_TrayWnd", null);
        }

        public static List<IntPtr> GetAllChildHandles(IntPtr hWnd, bool visibleOnly)
        {
            List<IntPtr> childHandles = new List<IntPtr>();

            GCHandle gcChildhandlesList = GCHandle.Alloc(childHandles);
            IntPtr pointerChildHandlesList = GCHandle.ToIntPtr(gcChildhandlesList);
            
            try
            {
                // http://blog.ralch.com/2015/04/pinvoke-getting-all-child-handles-of-window/
                EnumChildWindows(hWnd, delegate (IntPtr wnd, IntPtr param)
                    {
                        GCHandle dGcChildhandlesList = GCHandle.FromIntPtr(param);

                        if (dGcChildhandlesList == null || dGcChildhandlesList.Target == null)
                        {
                            return false;
                        }

                        List<IntPtr> dChildHandles = dGcChildhandlesList.Target as List<IntPtr>;

                        if (visibleOnly)
                        {
                            if (Window.IsVisible(wnd))
                            {
                                dChildHandles.Add(wnd);
                            }
                        }
                        else
                        {
                            dChildHandles.Add(wnd);
                        }

                        return true;
                    }
                    , pointerChildHandlesList);
                
            }
            finally
            {
                gcChildhandlesList.Free();
            }

            return childHandles;
        }
        
    

    /// <summary>
    /// Get all windows
    /// </summary>
    /// <param name="visibleOnly"></param>
    /// <returns></returns>
    public static List<IntPtr> GetAllWindows(bool visibleOnly)
        {
            IntPtr found = IntPtr.Zero;
            var windows = new List<IntPtr>();

            EnumWindows(delegate (IntPtr wnd, IntPtr param)
            {
                if (visibleOnly)
                {
                    if (Window.IsVisible(wnd))
                    {
                        windows.Add(wnd);
                    }
                }
                else
                {
                    windows.Add(wnd);
                }
                
                // but return true here so that we iterate all windows
                return true;
            }, IntPtr.Zero);

            return windows;
        }

        /// <summary>
        /// Gets the z-order for one or more windows atomically with respect to each other. In Windows, smaller z-order is higher. If the window is not top level, the z order is returned as -1. 
        /// </summary>
        static int[] GetZOrder(params IntPtr[] hWnds)
        {
            // http://stackoverflow.com/questions/825595/how-to-get-the-z-order-in-windows

            var z = new int[hWnds.Length];
            for (var i = 0; i < hWnds.Length; i++) z[i] = -1;

            var index = 0;
            var numRemaining = hWnds.Length;
            EnumWindows((wnd, param) =>
            {
                var searchIndex = Array.IndexOf(hWnds, wnd);
                if (searchIndex != -1)
                {
                    z[searchIndex] = index;
                    numRemaining--;
                    if (numRemaining == 0) return false;
                }
                index++;
                return true;
            }, IntPtr.Zero);

            return z;
        }

        static List<IntPtr> GetAllChildrenWindowHandles(IntPtr hParent,
  int maxCount)
        {
            //https://jamesmccaffrey.wordpress.com/2013/02/03/getting-all-child-window-handles-using-c-pinvoke-findwindowex/
            var result = new List<IntPtr>();
            int ct = 0;
            IntPtr prevChild = IntPtr.Zero;
            IntPtr currChild = IntPtr.Zero;
            while (true && ct < maxCount)
            {
                currChild = FindWindowEx(hParent, prevChild, null, null);
                if (currChild == IntPtr.Zero) break;
                result.Add(currChild);
                
                prevChild = currChild;
                ++ct;
            }
            return result;
        }

        

        public static Dictionary<IntPtr, Rectangle> GetVisibleWindows()
        {
            //var children = GetAllChildHandles(new IntPtr(461270));
            var allWindows = GetAllWindows(true);

            // windows should be ordered by z-index already, top windows first.
            
            // We will load the regions that each window occupies
            var windowWithRegions = allWindows.ToDictionary(key => key, val => Window.GetWindowRectangle(val));

            var visibleWindows = new Dictionary<IntPtr, Rectangle>();

            // Then we will check the intersection of each window against all windows with lower z-order.
            int zOrder = 0;
            foreach (var windowWithRegion in windowWithRegions)
            {
                bool overlapped = windowWithRegions.
                    Take(zOrder). // take just the windows with lower z-order (the ones on top of the current one)
                    AsParallel(). // try to save some time - we don't need the order anymore
                    Select(w => Rectangle.Intersect(windowWithRegion.Value, w.Value)).
                    // check if there is a window above which is equal in size or bigger than the current one
                    Any(w => w.Height >= windowWithRegion.Value.Height && w.Width >= windowWithRegion.Value.Width);

                //overlapped = false;
                if (!overlapped)
                {
                    var childWindowsWithRegions = GetAllChildHandles(windowWithRegion.Key, true).
                        ToDictionary(key => key, val => Window.GetWindowRectangle(val)).
                        OrderBy(x => x.Value.Height * x.Value.Width);
                    foreach (var childWindowWithRegion in childWindowsWithRegions)
                    {

                        bool childOverlapped = windowWithRegions.
                            Take(zOrder). // take just the windows with lower z-order (the ones on top of the current one)
                            AsParallel(). // try to save some time - we don't need the order anymore
                            Select(w => Rectangle.Intersect(childWindowWithRegion.Value, w.Value)).
                            // check if there is a window above which is equal in size or bigger than the current one
                            Any(w => w.Height >= childWindowWithRegion.Value.Height && w.Width >= childWindowWithRegion.Value.Width);
                        if (!childOverlapped)
                        {
                            visibleWindows.Add(childWindowWithRegion.Key, childWindowWithRegion.Value);
                        }
                    }
                }

                if (!overlapped)
                {
                    visibleWindows.Add(windowWithRegion.Key, windowWithRegion.Value);
                }

                zOrder++;
            }

            return visibleWindows;
        }

        public static Dictionary<IntPtr, Rectangle> GetWindowsByClassName(string className)
        {
            var allWindows = GetVisibleWindows();
            var windowsByClassName = new Dictionary<IntPtr, Rectangle>();
            foreach (var wnd in allWindows)
            {
                string currentWndClassName = Window.GetWindowClassName(wnd.Key);
                if (currentWndClassName == className)
                {
                    windowsByClassName.Add(wnd.Key, wnd.Value);
                }
            }
            return windowsByClassName;
        }

        //private static List<IntPtr> GetChildWindows()
        //{
        //    List<IntPtr> ch
        //    if (includeChildren)
        //    {
        //        var windowsCopy = new List<IntPtr>(windows);
        //        for (int i = 0; i < windowsCopy.Count; i++)
        //        {
        //            var childWindowsLevel1 = GetAllChildHandles(windows[i]);
        //            //GetAllChildrenWindowHandles(windowsCopy[i], 100);//
        //            //childWindowsLevel1.Reverse();

        //            childWindowsLevel1 = childWindowsLevel1.Where(c => !windows.Contains(c)).ToList();

        //            int insertAt = windows.FindIndex(x => x == windowsCopy[i]);
        //            windows.InsertRange(insertAt, childWindowsLevel1);
        //        }

        //    }

        //}

    }
}
