namespace Capturer.UI.Base.Extentions
{
    public static class SystemDrawingExtentions
    {
        public static System.Windows.Rect ToSystemWindowsRect(this System.Drawing.Rectangle rectangle)
        {
            return new System.Windows.Rect(rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height);
        }

        public static System.Windows.Rect ToSystemWindowsRect(this System.Drawing.RectangleF rectangle)
        {
            return new System.Windows.Rect(rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height);
        }

        public static System.Drawing.Rectangle ToSystemDrawingRectangle(this System.Windows.Rect rect)
        {
            return new System.Drawing.Rectangle((int)rect.X, (int)rect.Y, (int)rect.Width, (int)rect.Height);
        }

        public static System.Drawing.RectangleF ToSystemDrawingRectangleF(this System.Windows.Rect rect)
        {
            return new System.Drawing.RectangleF((float)rect.X, (float)rect.Y, (float)rect.Width, (float)rect.Height);
        }

        public static System.Windows.Point ToSystemWindowsPoint(this System.Drawing.Point point)
        {
            return new System.Windows.Point(point.X, point.Y);
        }

        public static System.Drawing.Point ToSystemDrawingPoint(this System.Windows.Point point)
        {
            return new System.Drawing.Point((int)point.X, (int)point.Y);
        }

        public static System.Drawing.PointF ToSystemDrawingPointF(this System.Windows.Point point)
        {
            return new System.Drawing.PointF((float)point.X, (float)point.Y);
        }
    }
}
