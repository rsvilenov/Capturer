using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Threading;

namespace Capturer.UI.Base
{
    /// <summary>
    /// Translated from https://code.msdn.microsoft.com/windowsdesktop/Per-Monitor-Aware-WPF-e43cde33
    /// Partially based on http://www.telerik.com/blogs/winforms-scaling-at-large-dpi-settings-is-it-even-possible-
    /// </summary>
    public class PerMonitorDPIWindow : Window
    {
        private double _currentDPI;

        public double CurrentDPI
        {
            get
            {
                return _currentDPI;
            }
            set
            {
                _currentDPI = value;
            }
        }

        private double _wpfDPI;

        public double WpfDPI
        {
            get
            {
                return _wpfDPI;
            }
            set
            {
                _wpfDPI = value;
            }
        }

        private double _scaleFactor;

        public double ScaleFactor
        {
            get
            {
                return _scaleFactor;
            }
            set
            {
                _scaleFactor = value;
            }
        }

        private IntPtr _handle;

        public IntPtr Handle
        {
            get
            {
                return _handle;
            }
        }

        private System.Drawing.Rectangle _clientRect;

        public System.Drawing.Rectangle ClientRect
        {
            get
            {
                return _clientRect;
            }
        }

        public bool AutoScaleAccordingToDPI { get; set; } = true;

        public event EventHandler<DPIChangedEventArgs> DPIChanged;
        
        private WinApiHelpers.Win32Structures.RECT _recommendedRectByWmDpichanged;


        protected PerMonitorDPIWindow()
        {
            Loaded += PerMonitorDPIWindow_Loaded;
            ContentRendered += PerMonitorDPIWindow_ContentRendered;
        }

        private void PerMonitorDPIWindow_ContentRendered(object sender, EventArgs e)
        {
            _currentDPI = WinApiHelpers.DPIHelper.GetDPIForWindow(_handle);
            _scaleFactor = _currentDPI / _wpfDPI;

            if (AutoScaleAccordingToDPI)
            {
                Width = Width * _scaleFactor;
                Height = Height * _scaleFactor;

                //Update graphics and text based on the current DPI of the monitor
                UpdateLaoyoutTransformWithScale(_scaleFactor);
            }
        }

        private void PerMonitorDPIWindow_Loaded(object sender, RoutedEventArgs e)
        {
            WinApiHelpers.DPIHelper.EnsurePerMonitorDPIAwareness();

            _handle = new WindowInteropHelper(this).Handle;
            PresentationSource MainWindowPresentationSource = PresentationSource.FromVisual(this);
            Matrix m = MainWindowPresentationSource.CompositionTarget.TransformToDevice;

            _wpfDPI = 96.0 * m.M11;

            _clientRect = WinApiHelpers.Window.GetClientArea(Handle);

            var hWndSource = HwndSource.FromHwnd(_handle);
            var hWndSourceHook = new HwndSourceHook(WndProc);
            hWndSource.AddHook(hWndSourceHook);
        }


        protected void UpdateLaoyoutTransformWithScale(double scaleFactor)
        {
            var child = GetVisualChild(0);
            if (scaleFactor != 1.0)
            {
                var dpiScale = new ScaleTransform(scaleFactor, scaleFactor);
                child.SetValue(Window.LayoutTransformProperty, dpiScale);
            }
            else
            {
                child.SetValue(Window.LayoutTransformProperty, null);
            }
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            double oldDpi;
            switch ((uint)msg)
            {
                case (int)WinApiHelpers.Win32Enums.WindowMessages.WM_DPICHANGED:
                    _recommendedRectByWmDpichanged = Marshal.PtrToStructure<WinApiHelpers.Win32Structures.RECT>(lParam);
                    oldDpi = _currentDPI;
                    var currentMonitor = WinApiHelpers.DisplayMonitors.GetByWindowHandle(hwnd);
                    _currentDPI = WinApiHelpers.DPIHelper.GetDPIForScreen(currentMonitor.Handle);//
                                                                                                 //_currentDPI = (double)wParam;
                    _scaleFactor = _currentDPI / _wpfDPI;
                    if (oldDpi != _currentDPI)
                    {
                        // do not execute this immediately as it may send WM_DPICHANGED again (see the telerik's article for per-monitor dpi)
                        Dispatcher.BeginInvoke((Action)(() => HandleDpiChange()));

                        DPIChanged?.Invoke(this, new DPIChangedEventArgs(_currentDPI));
                    }
                    break;
            }

            return IntPtr.Zero;
        }

        private void HandleDpiChange()
        {
            if (AutoScaleAccordingToDPI)
            {
                WinApiHelpers.WindowApis.SetWindowsPosition(Handle, _recommendedRectByWmDpichanged.Left, _recommendedRectByWmDpichanged.Top,
                    _recommendedRectByWmDpichanged.Width, 
                    _recommendedRectByWmDpichanged.Height, true);

                UpdateLaoyoutTransformWithScale(_scaleFactor);
            }
            _clientRect = WinApiHelpers.Window.GetClientArea(Handle);
        }
    }
}

