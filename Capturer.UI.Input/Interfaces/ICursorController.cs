using System.Windows.Input;

namespace Capturer.UI.Input.Interfaces
{
    public interface ICursorController : IUIInput
    {
        void HideCursor();
        void RestoreDefaultCursor();
        void SetCursor(string cursorFile);
        void SetCursor(Cursor cursor);
    }
}
