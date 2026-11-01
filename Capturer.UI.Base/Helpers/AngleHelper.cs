using System;
using System.Windows;

namespace Capturer.UI.Base.Helpers
{
    public static class AngleHelper
    {
        public enum AngleMetrics
        {
            Degrees,
            Radians
        }

        /// <summary>
        /// Get the clock arrow angle between the clock center and the point of the arrow
        /// </summary>
        /// <param name="target"></param>
        /// <param name="origin"></param>
        /// <param name="metrics"></param>
        /// <returns></returns>
        public static double GetClockwiseAngleBetween(Point target, Point origin, AngleMetrics metrics = AngleMetrics.Degrees)
        {
            // http://stackoverflow.com/questions/17530169/get-angle-between-point-and-origin
            var vector2 = target - origin;
            var vector1 = new Point(0, 1); // 12 o'clock == 0°, assuming that y goes from bottom to top

            double angleInRadians = Math.Atan2(vector2.Y, vector2.X) - Math.Atan2(vector1.Y, vector1.X);
            angleInRadians = Math.Abs(angleInRadians);
            if (metrics == AngleMetrics.Radians)
            {
                return angleInRadians;
            }
            else
            {
                return angleInRadians * (180.0 / Math.PI);
            }
        }
    }
}
