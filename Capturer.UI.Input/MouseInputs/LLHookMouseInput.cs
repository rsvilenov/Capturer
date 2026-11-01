using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Capturer.UI.Base.Extentions;
using Capturer.UI.Base.Helpers;
using Capturer.UI.Input.EventArgs;
using Capturer.UI.Input.Interfaces;
using Capturer.UI.Technical.Extentions;

namespace Capturer.UI.Input.MouseInputs
{
    internal class LLHookMouseInput : IMouseInput
    {
        private Window _window;
        private Point _lastMouseDownPoint;
        private bool _isLeftButtonDown;
        private bool _dragging;
        
        public event EventHandler<MouseCancelEventArgs> MouseDragBegin;
        public event EventHandler<MouseDragEventArgs> MouseDragEnd;
        public event EventHandler<MouseDragEventArgs> MouseDragging;
        public event EventHandler<MouseCancelEventArgs> MouseLeftButtonDown;
        public event EventHandler<MouseCancelEventArgs> MouseLeftButtonUp;
        public event EventHandler<MouseCancelEventArgs> MouseMove;
        public event EventHandler<MouseCancelEventArgs> MouseRightButtonDown;
        public event EventHandler<MouseCancelEventArgs> MouseRightButtonUp;

        public LLHookMouseInput()
        {
            WinApiHelpers.InputEventsHook.Start();
            WinApiHelpers.InputEventsHook.MouseAction += InputEventsHook_MouseAction;
        }

        public bool Dragging
        {
            get
            {
                return _dragging;
            }
        }

        private void InputEventsHook_MouseAction(WinApiHelpers.InputEventsHook.MouseEventArgs obj)
        {
            var currentPosition = obj.Point.ToSystemWindowsPoint();

            switch (obj.MouseAction)
            {
                case WinApiHelpers.Win32Enums.MouseMessages.WM_LBUTTONDOWN:
                    {
                        var args = new MouseCancelEventArgs(currentPosition);
                        MouseLeftButtonDown?.Invoke(_window, args);
                        obj.CancelEvent = args.Cancel;
                        if (!obj.CancelEvent)
                        {
                            _lastMouseDownPoint = currentPosition;
                            _isLeftButtonDown = true;
                        }
                    }
                    break;
                case WinApiHelpers.Win32Enums.MouseMessages.WM_LBUTTONUP:
                    {
                        var args = new MouseCancelEventArgs(currentPosition);
                        MouseLeftButtonUp?.Invoke(_window, args);
                        obj.CancelEvent = args.Cancel;
                        if (!obj.CancelEvent)
                        {
                            _isLeftButtonDown = false;
                            if (_dragging)
                            {
                                MouseDragEnd?.Invoke(_window, new MouseDragEventArgs(_lastMouseDownPoint, currentPosition));
                                _dragging = false;
                            }
                        }
                    }
                    break;
                case WinApiHelpers.Win32Enums.MouseMessages.WM_RBUTTONDOWN:
                    {
                        var args = new MouseCancelEventArgs(currentPosition);
                        MouseRightButtonDown?.Invoke(_window, args);
                        obj.CancelEvent = args.Cancel;
                    }
                    break;
                case WinApiHelpers.Win32Enums.MouseMessages.WM_RBUTTONUP:
                    {
                        var args = new MouseCancelEventArgs(currentPosition);
                        MouseRightButtonUp?.Invoke(_window, args);
                        obj.CancelEvent = args.Cancel;
                    }
                    break;
                case WinApiHelpers.Win32Enums.MouseMessages.WM_MOUSEMOVE:
                    {
                        var args = new MouseCancelEventArgs(currentPosition);
                        MouseMove?.Invoke(_window, args);
                        obj.CancelEvent = args.Cancel;
                        if (!obj.CancelEvent)
                        {
                            if (!_dragging)
                            {
                                if (_isLeftButtonDown &&
                                    MouseGesturesHelper.CheckMinDragDistanceReached(_lastMouseDownPoint, currentPosition))
                                {
                                    var dragBeginCancelArgs = new MouseCancelEventArgs(currentPosition);
                                    MouseDragBegin?.Invoke(_window, new MouseCancelEventArgs(currentPosition));
                                    _dragging = !dragBeginCancelArgs.Cancel;
                                }
                            }
                            else
                            {
                                MouseDragging?.Invoke(_window, new MouseDragEventArgs(_lastMouseDownPoint, currentPosition));
                            }
                        }
                    }
                    
                    break;

            }
        }

        public void Dispose()
        {
            WinApiHelpers.InputEventsHook.Stop();
            _window = null;
        }

        public void SetWindow(Window window)
        {
            _window = window;
        }
    }
}
