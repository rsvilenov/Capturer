using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Collections;
using WinApiHelpers;

namespace Capturer.UI.Technical.Extentions
{
    public static class FrameworkElementExtentions
    {
        public static void BringToFront(this FrameworkElement element)
        {
            if (element == null) return;

            var parent = element.Parent as Panel; // Panels and Canvas
            if (parent == null) return;

            var maxZ = parent.Children.OfType<UIElement>()
              .Where(x => x != element)
              .Select(x => Panel.GetZIndex(x))
              .Max();
            Panel.SetZIndex(element, maxZ + 1);
        }

        ///<summary>
        /// Returns the position of the point in screen coordinates of that control instead
        /// of the main-screen coordinates
        ///</summary>
        public static Point PointToCurrentScreen(this FrameworkElement self, Point location)
        {
            var window = System.Windows.Window.GetWindow(self);
            var screen = DisplayMonitors.GetByWindowHandle(
                new WindowInteropHelper(window).Handle);
            var screenBounds = screen.Bounds;
            var globalCoordinates = self.PointToScreen(location);
            return new Point(globalCoordinates.X - screenBounds.X, globalCoordinates.Y - screenBounds.Y);
        }

        public static void FreezeAllFreezableChildren(this DependencyObject parent)
        {
            var list = parent.GetLogicalChildCollection<Freezable>();
            foreach (var freezable in list)
            {
                if (freezable.CanFreeze)
                {
                    freezable.Freeze();
                }
            }
        }

        public static IEnumerable<T> GetLogicalChildCollection<T>(this DependencyObject parent) where T : DependencyObject
        {
            List<T> logicalCollection = new List<T>();
            GetLogicalChildCollection(parent as DependencyObject, logicalCollection);
            return logicalCollection;
        }

        private static void GetLogicalChildCollection<T>(DependencyObject parent, List<T> logicalCollection) where T : DependencyObject
        {
            IEnumerable children = LogicalTreeHelper.GetChildren(parent);
            foreach (object child in children)
            {
                if (child is DependencyObject)
                {
                    DependencyObject depChild = child as DependencyObject;
                    if (child is T)
                    {
                        logicalCollection.Add(child as T);
                    }
                    GetLogicalChildCollection(depChild, logicalCollection);
                }
            }
        }
    }
}
