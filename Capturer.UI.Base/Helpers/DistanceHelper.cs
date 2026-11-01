using System;
using System.Windows;
using System.Windows.Media;

namespace Capturer.UI.Base.Helpers
{
    public static class DistanceHelper
    {
        public static double GetDistanceBetween(Point p1, Point p2)
        {
            double distance = Math.Sqrt((Math.Pow(p1.X - p2.X, 2) +
                Math.Pow(p1.Y - p2.Y, 2)));
            return distance;
        }

        public static double GetDistanceTo(this Visual self, Visual v)
        {
            Point p1 = self.PointToScreen(new Point(0, 0));
            Point p2 = v.PointToScreen(new Point(0, 0));
            return GetDistanceBetween(p1, p2);
        }
    }
}
