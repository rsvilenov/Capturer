using System.Windows;
using System.Windows.Controls;
using System.Windows.Interactivity;

namespace Capturer.UI.Technical.Behaviors
{
    public class CanvasElementPositionToCenterBehavior : Behavior<FrameworkElement>
    {
        public Point DesiredCenterPoint
        {
            get { return (Point)GetValue(DesiredCenterPointProperty); }
            set { SetValue(DesiredCenterPointProperty, value); }
        }

        // Using a DependencyProperty as the backing store for DesiredCenterPoint.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DesiredCenterPointProperty =
            DependencyProperty.Register("DesiredCenterPoint", typeof(Point), typeof(CanvasElementPositionToCenterBehavior), new PropertyMetadata(new Point(), DesiredCenterPointChanged));

        private static void DesiredCenterPointChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var thisBehavior = d as CanvasElementPositionToCenterBehavior;
            thisBehavior.SetCenterPoint((Point)e.NewValue);
        }

        private void SetCenterPoint(Point p)
        {
            double newLeft = DesiredCenterPoint.X -  AssociatedObject.ActualWidth / 2;
            double newTop = DesiredCenterPoint.Y - AssociatedObject.ActualHeight / 2;
            Canvas.SetLeft(AssociatedObject, newLeft);
            Canvas.SetTop(AssociatedObject, newTop);
        }

        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.Loaded += AssociatedObject_Loaded;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            AssociatedObject.Loaded -= AssociatedObject_Loaded;
        }

        private void AssociatedObject_Loaded(object sender, RoutedEventArgs e)
        {
            SetCenterPoint(DesiredCenterPoint);
        }
    }
}
