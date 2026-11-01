using System.Windows;

namespace Capturer.UI.Input.EventArgs
{
    public class MouseRegularEventArgs
    {
        public MouseRegularEventArgs(Point mousePosition)
        {
            _mousePosition = mousePosition;
        }

        private Point _mousePosition;

        public Point MousePosition
        {
            get { return _mousePosition; }
            set { _mousePosition = value; }
        }
    }
}
