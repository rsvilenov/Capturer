using System;
using System.Windows.Interactivity;
using System.Windows;
using System.Windows.Media;
using System.Windows.Controls;

namespace Capturer.UI.Technical.Behaviors
{
    /// <summary>
    /// If we want exact rendering of the coordinates that we have passed, we cannot afford letting WPF using the calculated VisualOffset for our controls...
    /// So we are compensating for the calculated offset.
    /// Otherwise the controls inside the canvas will not be exactly where we want them to be.
    /// </summary>
    public class CanvasElementRemoveOffsetBehavior : Behavior<FrameworkElement>
    {
        bool _offsetCorrected;

        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.Loaded += AssociatedObject_Loaded;
        }

        private void AssociatedObject_Loaded(object sender, RoutedEventArgs e)
        {
            AssociatedObject.LayoutUpdated += AssociatedObject_LayoutUpdated;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            AssociatedObject.Loaded -= AssociatedObject_Loaded;
            AssociatedObject.LayoutUpdated -= AssociatedObject_LayoutUpdated;
        }

        private void AssociatedObject_LayoutUpdated(object sender, EventArgs e)
        {
            if (!_offsetCorrected)
            {
                var offset = VisualTreeHelper.GetOffset(AssociatedObject);

                Canvas.SetTop(AssociatedObject, - 1 * offset.Y);
                Canvas.SetLeft(AssociatedObject, -1 * offset.X);

                _offsetCorrected = true;
            }
        }
    }
}
