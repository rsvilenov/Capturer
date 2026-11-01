using System;
using System.Windows;

namespace Capturer.UI.Input.Interfaces
{
    public interface IUIInput : IDisposable
    {
        void SetWindow(Window window);
    }
}
