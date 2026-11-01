using System;
using System.Windows;
using Capturer.UI.Base.Helpers;
using Capturer.UI.Input.EventArgs;
using Capturer.UI.Input.Interfaces;

namespace Capturer.UI.Input.MouseInputs
{
    /// <summary>
    /// Use WPF's tunnelled events for mouse handling
    /// </summary>
    internal class WPFBaseMouseInput : IMouseInput
    {

        private Window _window;
        private Point _lastMouseDownPoint;
        private bool _dragging;
        private bool _areMouseEventWired;
        
        public event EventHandler<MouseCancelEventArgs> MouseLeftButtonUp;
        public event EventHandler<MouseCancelEventArgs> MouseLeftButtonDown;
        public event EventHandler<MouseCancelEventArgs> MouseRightButtonUp;
        public event EventHandler<MouseCancelEventArgs> MouseRightButtonDown;
        public event EventHandler<MouseCancelEventArgs> MouseDragBegin;
        public event EventHandler<MouseDragEventArgs> MouseDragEnd;
        public event EventHandler<MouseDragEventArgs> MouseDragging;
        public event EventHandler<MouseCancelEventArgs> MouseMove;

        public bool Dragging
        {
            get
            {
                return _dragging;
            }
        }

        public virtual void SetWindow(Window window)
        {
            if (_window != null)
            {
                throw new InvalidOperationException("SetWindow should be called only once");
            }
            if (_disposedValue)
            {
                throw new ObjectDisposedException(nameof(WPFBaseMouseInput));
            }
            _window = window;
            _window.Loaded += _window_Loaded;
            _window.Closing += _window_Closing;
            if (_window.IsVisible)
            {
                WireUpEvents();
            }
        }

        private void _window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Dispose(false);
        }

        private void _window_Loaded(object sender, RoutedEventArgs e)
        {
            if (_window.IsVisible)
            {
                WireUpEvents();
            }
            else
            {
                _window.IsVisibleChanged += _window_IsVisibleChanged;
            }
        }

        private void _window_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue)
            {
                WireUpEvents();
                _window.IsVisibleChanged -= _window_IsVisibleChanged;
            }
        }

        private void WireUpEvents()
        {
            _window.PreviewMouseLeftButtonDown += _window_MouseLeftButtonDown;
            _window.PreviewMouseLeftButtonUp += _window_MouseLeftButtonUp;
            _window.PreviewMouseRightButtonDown += _window_MouseRightButtonDown;
            _window.PreviewMouseRightButtonUp += _window_MouseRightButtonUp;
            _window.PreviewMouseMove += _window_PreviewMouseMove;
            _areMouseEventWired = true;
        }

        private void _window_PreviewMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            var currentPosition = e.GetPosition(_window);
            var args = new MouseCancelEventArgs(currentPosition);
            MouseMove?.Invoke(_window, args);
            e.Handled = args.Cancel;

            if (!e.Handled)
            {
                if (!_dragging)
                {
                    if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed &&
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

        private void _window_MouseRightButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var args = new MouseCancelEventArgs(e.GetPosition(_window));
            MouseRightButtonUp?.Invoke(_window, args);
            e.Handled = args.Cancel;
        }

        private void _window_MouseRightButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var args = new MouseCancelEventArgs(e.GetPosition(_window));
            MouseRightButtonDown?.Invoke(_window, args);
            e.Handled = args.Cancel;
        }

        private void _window_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var currentPosition = e.GetPosition(_window);
            var args = new MouseCancelEventArgs(currentPosition);
            MouseLeftButtonUp?.Invoke(_window, args);
            e.Handled = args.Cancel;
            if (!e.Handled && _dragging)
            {
                MouseDragEnd?.Invoke(_window, new MouseDragEventArgs(_lastMouseDownPoint, currentPosition));
                _dragging = false;
            }
        }

        private void _window_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var currentPosition = e.GetPosition(_window);
            var args = new MouseCancelEventArgs(currentPosition);
            MouseLeftButtonDown?.Invoke(_window, args);
            e.Handled = args.Cancel;
            if (!e.Handled)
            {
                _lastMouseDownPoint = currentPosition;
            }
        }

        #region IDisposable Support
        private bool _disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                _window.Closing -= _window_Closing;
                _window.Loaded -= _window_Loaded;
                if (_areMouseEventWired)
                {
                    _window.PreviewMouseLeftButtonDown -= _window_MouseLeftButtonDown;
                    _window.PreviewMouseLeftButtonUp -= _window_MouseLeftButtonUp;
                    _window.PreviewMouseRightButtonDown -= _window_MouseRightButtonDown;
                    _window.PreviewMouseRightButtonUp -= _window_MouseRightButtonUp;
                    _window.PreviewMouseMove -= _window_PreviewMouseMove;
                    _areMouseEventWired = false;
                }
                _window = null;
                _disposedValue = true;
            }
        }
        
        public void Dispose()
        {
            Dispose(true);
        }
        #endregion
    }
}
