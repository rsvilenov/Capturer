using System;
using System.Linq;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Interactivity;
using System.Windows.Threading;
using Capturer.UI.Technical.Adorners;

namespace Capturer.UI.Technical.Behaviors
{
    public class CanvasElementResizeBehavior : Behavior<FrameworkElement>
    {
        const string DEFAULT_THUMB_STYLE = "ThumbStyle_ResizeHandle";

        AdornerLayer _adornerLayer;
        
        public string ThumbStyleKey
        {
            get { return (string)GetValue(ThumbStyleKeyProperty); }
            set { SetValue(ThumbStyleKeyProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ThumbStyleKey.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ThumbStyleKeyProperty =
            DependencyProperty.Register("ThumbStyleKey", typeof(string), typeof(CanvasElementResizeBehavior), new PropertyMetadata(DEFAULT_THUMB_STYLE, ThumbStyleChanged));

        private static void ThumbStyleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {

            var thisBehavior = d as CanvasElementResizeBehavior;

            // redraw the adorner; the new adorner will use the new thumb style
            thisBehavior.RemoveAdorner();
            thisBehavior.AddAdorner(thisBehavior.ShowMiddleHandles);
        }

        public bool ShowMiddleHandles
        {
            get { return (bool)GetValue(ShowMiddleHandlesProperty); }
            set { SetValue(ShowMiddleHandlesProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ShowMiddleHandles.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ShowMiddleHandlesProperty =
            DependencyProperty.Register("ShowMiddleHandles", typeof(bool), typeof(CanvasElementResizeBehavior), new PropertyMetadata(false, ShowMiddleHandlesChanged));

        private static void ShowMiddleHandlesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var thisBehavior = d as CanvasElementResizeBehavior;

            thisBehavior.RemoveAdorner();
            thisBehavior.AddAdorner((bool)e.NewValue);
        }




        public Point ResizingMousePosition
        {
            get { return (Point)GetValue(ResizingMousePositionProperty); }
            set { SetValue(ResizingMousePositionProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ResizingMousePosition.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ResizingMousePositionProperty =
            DependencyProperty.Register("ResizingMousePosition", typeof(Point), typeof(CanvasElementResizeBehavior), new PropertyMetadata(new Point()));
        
        public bool Resizing
        {
            get { return (bool)GetValue(ResizingProperty); }
            set { SetValue(ResizingProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Resizing.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ResizingProperty =
            DependencyProperty.Register("Resizing", typeof(bool), typeof(CanvasElementResizeBehavior), new PropertyMetadata(false));

        public bool ResizeEnabled
        {
            get { return (bool)GetValue(ResizeEnabledProperty); }
            set { SetValue(ResizeEnabledProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ResizeEnabled.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ResizeEnabledProperty =
            DependencyProperty.Register("ResizeEnabled", typeof(bool), typeof(CanvasElementResizeBehavior), new PropertyMetadata(true, ResizeEnabledChanged));

        private static void ResizeEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var thisBehavior = d as CanvasElementResizeBehavior;
            if ((bool)e.NewValue)
            {
                thisBehavior.AddAdorner(thisBehavior.ShowMiddleHandles);
            }
            else
            {
                thisBehavior.RemoveAdorner();
            }
        }

        protected override void OnAttached()
        {
            base.OnAttached();

            
            // We can get the adorner layer only after the window of the current element has been shown.
            // When the window gets shown, the event "LayoutUpdated" starts to fire.
            AssociatedObject.LayoutUpdated += AssociatedObject_LayoutUpdated;
        }

        private void AssociatedObject_LayoutUpdated(object sender, EventArgs e)
        {
            _adornerLayer = AdornerLayer.GetAdornerLayer(AssociatedObject);

            if (ResizeEnabled)
            {
                AddAdorner(ShowMiddleHandles);
            }
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            AssociatedObject.LayoutUpdated -= AssociatedObject_LayoutUpdated;
            // don't leave the adorner referenced in the adorner collection
            RemoveAdorner();
        }
        

        Adorner ExistingAdorner
        {
            get
            {
                var adorner = _adornerLayer?.GetAdorners(AssociatedObject)?. // when no adorners, GetAdorners returns null
                    Where(a => a is ResizingAdorner).
                    FirstOrDefault();
                return adorner;
            }
        }

        void AddAdorner(bool showMiddleHandles)
        {
            // the designer isn't happy with our messing with the adorner layer
            //if (DesignerProperties.GetIsInDesignMode(AssociatedObject))
            //    return;
            Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() =>
            {
                if (_adornerLayer != null && ExistingAdorner == null)
                {
                    var newAdorner = new ResizingAdorner(AssociatedObject, showMiddleHandles, ThumbStyleKey);
                    
                    newAdorner.ResizingChanged += NewAdorner_ResizingChanged;
                    newAdorner.ResizingMousePositionChanged += NewAdorner_ResizingMousePositionChanged;
                    _adornerLayer.Add(newAdorner);
                }
            }));
        }

        private void NewAdorner_ResizingMousePositionChanged(object sender, ResizingAdorner.ResizingMousePosition e)
        {
            ResizingMousePosition = e.MousePosition;
        }

        private void NewAdorner_ResizingChanged(object sender, EventArgs e)
        {
            Resizing = (sender as ResizingAdorner).Resizing;
            //Debug.WriteLine("Resizing: " + Resizing);
        }

        void RemoveAdorner()
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() =>
            {
                // get a reference to the adorner, as ExistingAdorner may reevaluate to a different value
                var existingAdorner = ExistingAdorner as ResizingAdorner;
                if (_adornerLayer != null && existingAdorner != null)
                {
                    existingAdorner.ResizingChanged -= NewAdorner_ResizingChanged;
                    existingAdorner.ResizingMousePositionChanged -= NewAdorner_ResizingMousePositionChanged;
                    _adornerLayer.Remove(existingAdorner);
                }
            }));
        }

    }
}
