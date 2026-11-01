using System;
using Capturer.UI.Input.EventArgs;

namespace Capturer.UI.Input.Interfaces
{
    public interface IMouseInput : IUIInput
    {
        event EventHandler<MouseCancelEventArgs> MouseLeftButtonUp;
        event EventHandler<MouseCancelEventArgs> MouseLeftButtonDown;
        event EventHandler<MouseCancelEventArgs> MouseRightButtonUp;
        event EventHandler<MouseCancelEventArgs> MouseRightButtonDown;
        event EventHandler<MouseCancelEventArgs> MouseDragBegin;
        event EventHandler<MouseDragEventArgs> MouseDragEnd;
        event EventHandler<MouseDragEventArgs> MouseDragging;
        event EventHandler<MouseCancelEventArgs> MouseMove;

        bool Dragging { get; }
    }
}
