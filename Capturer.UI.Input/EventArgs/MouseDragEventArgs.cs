using System.Windows;

namespace Capturer.UI.Input.EventArgs
{
    public class MouseDragEventArgs
    {
        public MouseDragEventArgs(Point initialMousePosition, Point currentMousePosition)
        {
            _initialMousePosition = initialMousePosition;
            _currentMousePosition = currentMousePosition;
        }

        private Point _initialMousePosition;

        public Point InitialMousePosition
        {
            get { return _initialMousePosition; }
            set { _initialMousePosition = value; }
        }


        private Point _currentMousePosition;

        public Point CurrentMousePosition
        {
            get { return _currentMousePosition; }
            set { _currentMousePosition = value; }
        }
    }
}
