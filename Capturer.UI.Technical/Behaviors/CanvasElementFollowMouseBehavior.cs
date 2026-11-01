using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interactivity;
using System.Windows.Media;
using Capturer.UI.Base.Extentions;
using Capturer.UI.Base.Helpers;
using WinApiHelpers;

namespace Capturer.UI.Technical.Behaviors
{
    public class CanvasElementFollowMouseBehavior : Behavior<FrameworkElement>
    {
        private const double SWITCH_TOLLERANCE_IN_PX = 15;

        public enum HorizontalElementPlacement
        {
            Left,
            Right
        }


        public enum VerticalElementPlacement
        {
            Top,
            Bottom
        }
        


        public VerticalElementPlacement ActualVerticalPlacementRelativeToMouse
        {
            get { return (VerticalElementPlacement)GetValue(ActualVerticalPlacementRelativeToMouseProperty); }
            set { SetValue(ActualVerticalPlacementRelativeToMouseProperty, value); }
        }


        // Using a DependencyProperty as the backing store for ActualVerticalPlacementRelativeToMouse.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ActualVerticalPlacementRelativeToMouseProperty =
            DependencyProperty.Register("ActualVerticalPlacementRelativeToMouse", typeof(VerticalElementPlacement), typeof(CanvasElementFollowMouseBehavior), new PropertyMetadata(VerticalElementPlacement.Bottom));


        // Using a DependencyProperty as the backing store for ActualVerticalPlacementRelativeToMouse.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ActualHorizontalPlacementRelativeToMouseProperty =
            DependencyProperty.Register("ActualHorizontalPlacementRelativeToMouse", typeof(HorizontalElementPlacement), typeof(CanvasElementFollowMouseBehavior), new PropertyMetadata(HorizontalElementPlacement.Left));

        public HorizontalElementPlacement ActualHorizontalPlacementRelativeToMouse
        {
            get { return (HorizontalElementPlacement)GetValue(ActualHorizontalPlacementRelativeToMouseProperty); }
            set { SetValue(ActualHorizontalPlacementRelativeToMouseProperty, value); }
        }
        
        public HorizontalElementPlacement InitialHorizontalPlacementRelativeToMouse
        {
            get { return (HorizontalElementPlacement)GetValue(InitialHorizontalPlacementRelativeToMouseProperty); }
            set { SetValue(InitialHorizontalPlacementRelativeToMouseProperty, value); }
        }

        // Using a DependencyProperty as the backing store for PositionRelativeToMouse.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty InitialHorizontalPlacementRelativeToMouseProperty =
            DependencyProperty.Register("InitialHorizontalPlacementRelativeToMouse", typeof(HorizontalElementPlacement), typeof(CanvasElementFollowMouseBehavior), new PropertyMetadata(HorizontalElementPlacement.Right, InitialHorizontalPlacementRelativeToMouseChanged));

        private static void InitialHorizontalPlacementRelativeToMouseChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var thisBehavior = d as CanvasElementFollowMouseBehavior;
            thisBehavior.ActualHorizontalPlacementRelativeToMouse = (HorizontalElementPlacement)e.NewValue;
        }

        public double DistanceFromMouse
        {
            get { return (double)GetValue(DistanceFromMouseProperty); }
            set { SetValue(DistanceFromMouseProperty, value); }
        }

        // Using a DependencyProperty as the backing store for DistanceFromMouse.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DistanceFromMouseProperty =
            DependencyProperty.Register("DistanceFromMouse", typeof(double), typeof(CanvasElementFollowMouseBehavior), new PropertyMetadata(10d));


        


        private Canvas _parentCanvas;
        private Rect _parentCanvasRect;
        private PresentationSource _parentCanvasPresentationSource;

        private double _verticalSwitchTreshold;
        private double _horizontalSwitchTreshold;

        private List<Rect> _allScreenRects;

        Rect _enclosingRectangle;


        private List<Tuple<Rect, Rect>> _screenRectangles = new List<Tuple<Rect, Rect>>();

        public Point MousePosition
        {
            get { return (Point)GetValue(MousePositionProperty); }
            set { SetValue(MousePositionProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MousePosition.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MousePositionProperty =
            DependencyProperty.Register("MousePosition", typeof(Point), typeof(CanvasElementFollowMouseBehavior), new PropertyMetadata(new Point(), MousePositionChanged));

        private static void MousePositionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var thisBehavior = (d as CanvasElementFollowMouseBehavior);
            thisBehavior.SetAssociatedObjectLocation();
        }
        

        private Rect ScaleRect(Rect rect, double scaleFactor)
        {
            rect.Scale(0.95, 0.95);
            return rect;
        }

        private void SetCurrentScreenCoordinates()
        {
            var associatedObjectRect = ScreenCoordinatesHelper.GetRectOfFrameworkElement(AssociatedObject);
            var intersectingScreens = _allScreenRects.Where(mon => mon.IntersectsWith(associatedObjectRect)).
                Select(mon => new Tuple<Rect, Rect>(mon, Rect.Intersect(mon, associatedObjectRect)));

            if (!intersectingScreens.Any())
                return;

            if (intersectingScreens.Count() > 1)
            {
                var newScreenRect = intersectingScreens.First(scr => scr.Item1 != _enclosingRectangle).Item1;
                if (_enclosingRectangle != newScreenRect)
                {
                    _enclosingRectangle = newScreenRect;
                    _horizontalSwitchTreshold = _enclosingRectangle.Width;
                    _verticalSwitchTreshold = _enclosingRectangle.Height;
                }
            }
            else
            {
                var newScreenRect = intersectingScreens.Single().Item1;

                if (newScreenRect.Contains(_parentCanvasRect) == false && _enclosingRectangle != newScreenRect)
                {
                    _enclosingRectangle = newScreenRect;
                    _horizontalSwitchTreshold = _enclosingRectangle.Width;
                    _verticalSwitchTreshold = _enclosingRectangle.Height;
                }
            }
        }

        void SetAssociatedObjectLocation()
        {
            // if the associated object is already disconnected from its presentation source, return
            if (!AssociatedObject.IsVisible)
            {
                return;
            }

            SetCurrentScreenCoordinates();

            // make sure that the mouse coordinates are reported 
            // only as internal to the canvas (when the mouse is inside,
            // reort it as positioned at the border of the canvas)

            var mouseXpos = MousePosition.X;
            
            if (mouseXpos > _parentCanvas.ActualWidth)
            {
                mouseXpos = _parentCanvas.ActualWidth;
            }

            var mouseYpos = MousePosition.Y;
            if (mouseYpos > _parentCanvas.ActualHeight)
            {
                mouseYpos = _parentCanvas.ActualHeight;
            }

            // these are coordinates inner to the canvas, so no negative values are allowed here
            if (mouseXpos < 0)
            {
                mouseXpos = 0;
            }

            if (mouseYpos < 0)
            {
                mouseYpos = 0;
            }
            

            double newTop = mouseYpos + DistanceFromMouse;

            if (newTop + AssociatedObject.ActualHeight >= _verticalSwitchTreshold)
            {
                newTop = mouseYpos - (AssociatedObject.ActualHeight + DistanceFromMouse);
                if (ActualVerticalPlacementRelativeToMouse == VerticalElementPlacement.Bottom)
                {
                    _verticalSwitchTreshold -= SWITCH_TOLLERANCE_IN_PX;
                    ActualVerticalPlacementRelativeToMouse = VerticalElementPlacement.Top;
                }
            }
            else
            { 
                if (ActualVerticalPlacementRelativeToMouse == VerticalElementPlacement.Top)
                {
                    _verticalSwitchTreshold += SWITCH_TOLLERANCE_IN_PX;
                    ActualVerticalPlacementRelativeToMouse = VerticalElementPlacement.Bottom;
                }
            }


            double newLeft = 0;
            if (InitialHorizontalPlacementRelativeToMouse == HorizontalElementPlacement.Right)
            {
                newLeft = mouseXpos + DistanceFromMouse;
                if (newLeft + AssociatedObject.ActualWidth >=
                    _horizontalSwitchTreshold)
                {
                    newLeft = mouseXpos -  AssociatedObject.ActualWidth - DistanceFromMouse;
                    if (ActualHorizontalPlacementRelativeToMouse == HorizontalElementPlacement.Right)
                    {
                        _horizontalSwitchTreshold -= SWITCH_TOLLERANCE_IN_PX;
                        ActualHorizontalPlacementRelativeToMouse = HorizontalElementPlacement.Left;
                    }
                }
                else
                {
                    if (ActualHorizontalPlacementRelativeToMouse == HorizontalElementPlacement.Left)
                    {
                        _horizontalSwitchTreshold += SWITCH_TOLLERANCE_IN_PX;
                        ActualHorizontalPlacementRelativeToMouse = HorizontalElementPlacement.Right;
                    }
                }
            }
            else
            {
                newLeft = mouseXpos - AssociatedObject.ActualWidth - DistanceFromMouse;
                if (newLeft <= _parentCanvas.PointFromScreen(_enclosingRectangle.Location).X)
                {
                    ActualHorizontalPlacementRelativeToMouse = HorizontalElementPlacement.Right;
                    newLeft = mouseXpos + DistanceFromMouse;
                }
                else if (newLeft <= SWITCH_TOLLERANCE_IN_PX && ActualHorizontalPlacementRelativeToMouse == HorizontalElementPlacement.Right)
                {
                    newLeft = mouseXpos + DistanceFromMouse;
                }
                else
                {
                    ActualHorizontalPlacementRelativeToMouse = HorizontalElementPlacement.Left;
                }
            }

            //rctMovingObject.BeginAnimation(Rectangle.WidthProperty, animation);
            Canvas.SetTop(AssociatedObject, newTop);
            Canvas.SetLeft(AssociatedObject, newLeft);

        }
        
        protected override void OnAttached()
        {
            base.OnAttached();
            
            _parentCanvas = AssociatedObject.Parent as Canvas;
            AssociatedObject.IsVisibleChanged += AssociatedObject_IsVisibleChanged;
            AssociatedObject.Loaded += AssociatedObject_Loaded;

            _allScreenRects = DisplayMonitors.AllMonitors.Select(mon => mon.DPICalibratedBounds.ToSystemWindowsRect()).ToList();
        }

        private void AssociatedObject_Loaded(object sender, RoutedEventArgs e)
        {
            _parentCanvasPresentationSource = PresentationSource.FromVisual((Visual)sender);
            _parentCanvasPresentationSource.ContentRendered += PresentationSource_ContentRendered;

            ActualHorizontalPlacementRelativeToMouse = InitialHorizontalPlacementRelativeToMouse;
        }

        private void PresentationSource_ContentRendered(object sender, EventArgs e)
        {
            _parentCanvasRect = ScreenCoordinatesHelper.GetRectOfFrameworkElement(_parentCanvas);
            _enclosingRectangle = _parentCanvasRect;
            _horizontalSwitchTreshold = _enclosingRectangle.Width;
            _verticalSwitchTreshold = _enclosingRectangle.Height;

            SetCurrentScreenCoordinates();

            SetAssociatedObjectLocation();
        }
        

        private void AssociatedObject_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            SetAssociatedObjectLocation();
        }
        

        protected override void OnDetaching()
        {
            base.OnDetaching();
            AssociatedObject.IsVisibleChanged -= AssociatedObject_IsVisibleChanged;
            AssociatedObject.Loaded -= AssociatedObject_Loaded;
            _parentCanvasPresentationSource.ContentRendered -= PresentationSource_ContentRendered;
        }
        
    }
}
