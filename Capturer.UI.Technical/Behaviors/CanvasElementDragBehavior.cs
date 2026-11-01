using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interactivity;
using Capturer.UI.Base.Extentions;
using Capturer.UI.Base.Helpers;

namespace Capturer.UI.Technical.Behaviors
{
    /// <summary>
    /// Behavior that provides drag behavior of UIElements that are children of a Canvas
    /// </summary>
    public class CanvasElementDragBehavior : Behavior<FrameworkElement>
    {
        private Canvas _parentCanvas;
        private Window _parentWindow;

        private bool _isMouseDown;
        private bool _isDragging;
        private Point _startPoint;
        private double _originalLeft;
        private double _originalTop;



        public bool Enabled
        {
            get { return (bool)GetValue(EnabledProperty); }
            set { SetValue(EnabledProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Enabled.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty EnabledProperty =
            DependencyProperty.Register("Enabled", typeof(bool), typeof(CanvasElementDragBehavior), new PropertyMetadata(true));



        protected override void OnAttached()
        {
            base.OnAttached();

            _parentCanvas = LogicalTreeHelper.GetParent(AssociatedObject) as Canvas;
            if (_parentCanvas != null)
            {
                _parentWindow = Window.GetWindow(_parentCanvas);

                AssociatedObject.MouseDown += AssociatedObject_MouseDown;
                _parentWindow.MouseUp += ParentWindow_MouseUp;
                _parentWindow.MouseMove += ParentWindow_MouseMove;
                _parentWindow.MouseLeave += ParentWindow_MouseLeave;
            }
            else
            {
                throw new InvalidOperationException("This behavior cannot be attached to elements which are not immediate children of a Canvas control.");
            }
        }

        /// <summary>
        /// If the mouse leaves the window, reset the dragging flags
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ParentWindow_MouseLeave(object sender, MouseEventArgs e)
        {
            if (!Enabled)
            {
                return;
            }
            _isMouseDown = false;
            _isDragging = false;
        }

        private void ParentWindow_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (!Enabled)
            {
                return;
            }
            _isMouseDown = false;
            _isDragging = false;
        }

        private void ParentWindow_MouseMove(object sender, MouseEventArgs e)
        {
            if (!Enabled)
            {
                return;
            }

            if (_isMouseDown)
            {

                Point currentMousePosition = e.GetPosition(_parentCanvas);
                if (!_isDragging)
                {
                    if (MouseGesturesHelper.CheckMinDragDistanceReached(_startPoint, currentMousePosition))
                    {
                        _isDragging = true;
                    }
                }

                if (_isDragging)
                {
                    // get the new positions
                    double newTop = currentMousePosition.Y - (_startPoint.Y - _originalTop);
                    double newLeft = currentMousePosition.X - (_startPoint.X - _originalLeft);

                    // get the parent and the dragging child's rectangle
                    var parentRect = GetRectOfObject(_parentCanvas);
                    var childRect = new Rect
                    {
                        X = newLeft,
                        Y = newTop,
                        Width = AssociatedObject.ActualWidth,
                        Height = AssociatedObject.ActualHeight
                    };

                    if (newLeft > 0 && 
                        newLeft + AssociatedObject.ActualWidth < parentRect.Width)
                    {
                        Canvas.SetLeft(AssociatedObject, newLeft);
                    }
                    // if the new top position is outside, snap to the parent's border
                    else if (newLeft < 0)
                    {
                        Canvas.SetLeft(AssociatedObject, 0);
                    }
                    else if (newLeft + AssociatedObject.ActualWidth > parentRect.Width)
                    {
                        Canvas.SetLeft(AssociatedObject, parentRect.Width - AssociatedObject.ActualWidth);
                    }

                    if (newTop > 0 && newTop + AssociatedObject.ActualHeight < parentRect.Height)
                    {
                        Canvas.SetTop(AssociatedObject, newTop);
                    }
                    // if the new top position is outside, snap to the parent's border
                    else if (newTop < 0)
                    {
                        Canvas.SetTop(AssociatedObject, 0);
                    }
                    else if (newTop + AssociatedObject.ActualHeight > parentRect.Height)
                    {
                        Canvas.SetTop(AssociatedObject, parentRect.Height - AssociatedObject.ActualHeight);
                    }
                
                    // if the child rect is still within the bounds of the parent, move it
                    //if (parentRect.Contains(childRect))
                    //{
                    //    Canvas.SetTop(AssociatedObject, newTop);
                    //    Canvas.SetLeft(AssociatedObject, newLeft);
                    //}
                }
            }
        }

        private Rect GetRectOfObject(FrameworkElement _element)
        {
            Rect rectangleBounds = new Rect();
            rectangleBounds = _element.RenderTransform.TransformBounds(new Rect(0, 0, _element.ActualWidth, _element.ActualHeight));
            return rectangleBounds;
        }

        private void AssociatedObject_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (!Enabled)
            {
                return;
            }
            _isMouseDown = true;
            _startPoint = e.GetPosition(_parentCanvas);

            _originalLeft = Canvas.GetLeft(AssociatedObject).GetZeroIfNaN();
            _originalTop = Canvas.GetTop(AssociatedObject).GetZeroIfNaN();
            
            e.Handled = true;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();

            AssociatedObject.MouseDown -= AssociatedObject_MouseDown;
            _parentWindow.MouseUp -= ParentWindow_MouseUp;
            _parentWindow.MouseMove -= ParentWindow_MouseMove;
            _parentWindow.MouseLeave -= ParentWindow_MouseLeave;
        }
    }
}
