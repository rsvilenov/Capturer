using System.Windows;
using System.Windows.Controls;
using System.Windows.Interactivity;

namespace Capturer.UI.Technical.Behaviors
{
    public class CanvasElementMouseSelectionBox : Behavior<FrameworkElement>
    {


        public Point StartPosition
        {
            get { return (Point)GetValue(StartPositionProperty); }
            set { SetValue(StartPositionProperty, value); }
        }

        // Using a DependencyProperty as the backing store for StartPosition.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty StartPositionProperty =
            DependencyProperty.Register("StartPosition", typeof(Point), typeof(CanvasElementMouseSelectionBox), new PropertyMetadata(new Point(), StartPositionChanged));

        private static void StartPositionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var thisBehavior = d as CanvasElementMouseSelectionBox;
            Point newPos = (Point)e.NewValue;
            thisBehavior.SetAssociatedElementRectangle(newPos.X, newPos.Y, 0, 0);
        }

        public Point MousePosition
        {
            get { return (Point)GetValue(MousePositionProperty); }
            set { SetValue(MousePositionProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MousePosition.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MousePositionProperty =
            DependencyProperty.Register("MousePosition", typeof(Point), typeof(CanvasElementMouseSelectionBox), new PropertyMetadata(new Point(), MousePositionChanged));


        private static void MousePositionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var thisBehavior = d as CanvasElementMouseSelectionBox;
            Point newPos = (Point)e.NewValue;
            
            thisBehavior.SetAssociatedElementRectangleFromMousePos(newPos);
        }


        public bool Enabled
        {
            get { return (bool)GetValue(EnabledProperty); }
            set { SetValue(EnabledProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Enabled.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty EnabledProperty =
            DependencyProperty.Register("Enabled", typeof(bool), typeof(CanvasElementMouseSelectionBox), new PropertyMetadata(false));


        private void SetAssociatedElementRectangle(double x, double y, double width, double height)
        {
            if (AssociatedObject != null && AssociatedObject.IsLoaded && Enabled)
            {
                Canvas.SetLeft(AssociatedObject, x);
                Canvas.SetTop(AssociatedObject, y);
                if (width >= 0)
                {
                    AssociatedObject.Width = width;
                }
                if (height >= 0)
                {
                    AssociatedObject.Height = height;
                }
            }
        }
        
        private void SetAssociatedElementRectangleFromMousePos(Point mousePos)
        {
            if (!Enabled)
                return;
            
            double newX = Canvas.GetLeft(AssociatedObject);
            double newY = Canvas.GetTop(AssociatedObject);
            double newWidth = AssociatedObject.Width;
            double newHeight = AssociatedObject.Height;
            
            if (mousePos.X > StartPosition.X)
            {
                newWidth = mousePos.X - StartPosition.X;
                newX = StartPosition.X;
            }
            else if (mousePos.X < StartPosition.X)
            {
                newWidth = StartPosition.X - mousePos.X;
                newX = mousePos.X;
            }


            if (mousePos.Y > StartPosition.Y)
            {
                newHeight = mousePos.Y - StartPosition.Y;
                newY = StartPosition.Y;
            }
            else if (mousePos.Y < StartPosition.Y)
            {
                newHeight = StartPosition.Y - mousePos.Y;
                newY = mousePos.Y;
            }

            SetAssociatedElementRectangle(newX, newY, newWidth, newHeight);
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
            SetAssociatedElementRectangle(StartPosition.X, StartPosition.Y, 0, 0);
        }
    }
}
