using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;

namespace WinApiHelpers
{
    public static class DPIHelper
    {

        public static void EnsurePerMonitorDPIAwareness()
        {
            // TODO: make this windows 7 compatible
            if (Environment.OSVersion.Version.Major < 6)
            {
                throw new PlatformNotSupportedException();
            }

            ProcessDPIAwareness awareness = GetCurrentDPIAwareness();
            if (awareness == ProcessDPIAwareness.ProcessSystemDPIAware)
            {
                throw new InvalidOperationException("DPI awareness setting of ProcessSystemDPIAware (the default for WPF apps) was detected. However, we need ProcessPerMonitorDPIAware. Please edit your AssemblyInfo.cs and add the following line there: [assembly: DisableDpiAwareness].");
            }
            else if (awareness == ProcessDPIAwareness.ProcessDPIUnaware)
            {
                int result = SetProcessDpiAwareness(ProcessDPIAwareness.ProcessPerMonitorDPIAware);
                if (result != 0)
                {
                    throw new Win32Exception("SetProcessDpiAwareness failed. Please do not call this method twice and ensure that there is the following line in AssemblyInfo.cs: [assembly: DisableDpiAwareness].");
                }
            }

            //awareness = GetCurrentDPIAwareness();
            //if (awareness == ProcessDPIAwareness.ProcessDPIUnaware)
            //{
            //    throw new Exception("Could not enter ProcessPerMonitorDPIAware mode.");
            //}

        }

        public static ProcessDPIAwareness GetCurrentDPIAwareness()
        {
            var currentProcess = Process.GetCurrentProcess();
            ProcessDPIAwareness awareness;
            GetProcessDpiAwareness(currentProcess.Handle, out awareness);
            return awareness;
        }

        public static double GetDPIForScreen(IntPtr hMonitor)
        {
            uint dpiX = 0;
            uint dpiY = 0;
            var hResult = GetDpiForMonitor(hMonitor, MONITOR_DPI_TYPE.MDT_EFFECTIVE_DPI, out dpiX, out dpiY);
            if (hResult != 0)
            {
                dpiX = 96;
            }

            return (double)dpiX;
        }


        public static double GetDPIForWindow(IntPtr hWnd)
        {
            var hMonitor = MonitorFromWindow(hWnd, MONITOR_DEFAULTTONEAREST);
            return GetDPIForScreen(hMonitor);
        }

        public static double GetSystemDPI()
        {
            double dpiX = 96;
            using (Graphics g = Graphics.FromHwnd(IntPtr.Zero))
            {
                IntPtr desktopHdc = g.GetHdc();
                dpiX = GetDeviceCaps(desktopHdc, (int)DeviceCap.LOGPIXELSX);
                g.ReleaseHdc(desktopHdc);
            }

            return dpiX;
        }

        public enum ProcessDPIAwareness
        {
            ProcessDPIUnaware = 0,
            ProcessSystemDPIAware = 1,
            ProcessPerMonitorDPIAware = 2
        }

        [DllImport("SHCore.dll", SetLastError = true)]
        private static extern void GetProcessDpiAwareness(IntPtr hprocess, out ProcessDPIAwareness awareness);

        [DllImport("SHCore.dll", SetLastError = true)]
        private static extern int SetProcessDpiAwareness(ProcessDPIAwareness value);


        const int MONITOR_DEFAULTTONULL = 0;
        const int MONITOR_DEFAULTTOPRIMARY = 1;
        const int MONITOR_DEFAULTTONEAREST = 2;

        [DllImport("user32.dll")]
        static extern IntPtr MonitorFromWindow(IntPtr hwnd, uint dwFlags);

        private enum MONITOR_DPI_TYPE
        {
            MDT_EFFECTIVE_DPI = 0,
            MDT_ANGULAR_DPI = 1,
            MDT_RAW_DPI = 2,
            MDT_DEFAULT = MDT_EFFECTIVE_DPI
        }


        [DllImport("SHCore.dll", SetLastError = true)]
        static extern int GetDpiForMonitor(IntPtr hmonitor,
                                        MONITOR_DPI_TYPE dpiType,
                                        out uint dpiX,
                                        out uint dpiY);

        [DllImport("gdi32.dll")]
        static extern int GetDeviceCaps(IntPtr hdc, int nIndex);
        public enum DeviceCap
        {
            VERTRES = 10,
            DESKTOPVERTRES = 117,
            LOGPIXELSX = 88

            // http://pinvoke.net/default.aspx/gdi32/GetDeviceCaps.html
        }


    }
}
