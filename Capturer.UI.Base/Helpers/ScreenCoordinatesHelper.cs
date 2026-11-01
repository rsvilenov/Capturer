using System.Windows;

namespace Capturer.UI.Base.Helpers
{
    public static class ScreenCoordinatesHelper
    {

        public static Rect GetRectOfFrameworkElement(FrameworkElement element)
        {
            
            var screenPosition = element.PointToScreen(new Point(0, 0));
            var size = new Size(element.ActualWidth, element.ActualHeight);
            size = (Size)PresentationSource.FromVisual(element).CompositionTarget.TransformFromDevice.Transform((Vector)size);


            Rect rectangleBounds = new Rect(screenPosition, size);
            //var screenCoords = element.PointToScreen(new Point(SystemParameters.VirtualScreenLeft, SystemParameters.VirtualScreenTop));
            //rectangleBounds = element.RenderTransform.TransformBounds(new Rect(screenCoords.X, screenCoords.Y, element.ActualWidth, element.ActualHeight));
            return rectangleBounds;
        }
    }
}
