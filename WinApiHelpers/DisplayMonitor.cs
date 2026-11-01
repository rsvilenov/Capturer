using System;
using System.Diagnostics;
using System.Drawing;

namespace WinApiHelpers
{
    [DebuggerDisplay("{_deviceNameNoPath}: {Bounds}")]
    public class DisplayMonitor
    {
        internal DisplayMonitor(IntPtr handle, string deviceName, string friendlyName, Rectangle bounds, Rectangle workingArea, double dpi, bool isPrimary, TaskbarPosition positionOfTaskbar)
        {
            Handle = handle;
            DeviceName = deviceName;
            FriendlyName = friendlyName;
            Bounds = bounds;
            WorkingArea = workingArea;
            DPI = dpi;
            IsPrimary = isPrimary;
            PositionOfTaskbar = positionOfTaskbar;

            // TODO: if looks like the monitor's dimentions shoud be corrected 
            // only if the system DPI is changed. Per monitor DPIs does not seem
            // to affect the reporting of the screen coordinates
            var dpiScaleFactor = DPIHelper.GetSystemDPI() / 96.0; //DPI / 96.0;

            DPICalibratedBounds = new Rectangle
            {
                X = (int)(Bounds.X / dpiScaleFactor),
                Y = (int)(Bounds.Y / dpiScaleFactor),
                Width = (int)(Bounds.Width / dpiScaleFactor),
                Height = (int)(Bounds.Height / dpiScaleFactor)
            };

            DPICalibratedWorkingArea = new Rectangle
            {
                X = (int)(WorkingArea.X / dpiScaleFactor),
                Y = (int)(WorkingArea.Y / dpiScaleFactor),
                Width = (int)(WorkingArea.Width / dpiScaleFactor),
                Height = (int)(WorkingArea.Height / dpiScaleFactor)
            };
        }

        public enum TaskbarPosition
        {
            NoTaskbar,
            Top,
            Left,
            Bottom,
            Right
        }

        public TaskbarPosition PositionOfTaskbar { get; private set; }

        public IntPtr Handle { get; private set; }

        public string DeviceName { get; private set; }

        public string FriendlyName { get; private set; }

        public Rectangle Bounds { get; private set; }

        public Rectangle DPICalibratedBounds { get; private set; }

        public Rectangle WorkingArea { get; private set; }

        public Rectangle DPICalibratedWorkingArea { get; private set; }

        public double DPI { get; private set; }

        public bool IsPrimary { get; private set; }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private string _deviceNameNoPath
        {
            get
            {
                return DeviceName.Replace("\\\\.\\", "");
            }
        }
    }
}
