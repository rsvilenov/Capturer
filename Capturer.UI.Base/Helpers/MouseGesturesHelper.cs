using System;
using System.Windows;

namespace Capturer.UI.Base.Helpers
{
    public static class MouseGesturesHelper
    {
        public static bool CheckMinDragDistanceReached(Point initialPosition, Point currentPosition)
        {
            if (((Math.Abs(currentPosition.X - initialPosition.X) > SystemParameters.MinimumHorizontalDragDistance) ||
                    (Math.Abs(currentPosition.Y - initialPosition.Y) > SystemParameters.MinimumVerticalDragDistance)))
                return true;

            return false;
        }
    }
}
