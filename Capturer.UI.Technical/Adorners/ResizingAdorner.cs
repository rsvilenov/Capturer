using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using Capturer.UI.Base.Extentions;
using Capturer.UI.Base.Helpers;

namespace Capturer.UI.Technical.Adorners
{

    public class ResizingAdorner : Adorner
    {
        public class ResizingMousePosition : EventArgs
        {
            public Point MousePosition { get; set; }
            public ResizingMousePosition(Point mousePos)
            {
                MousePosition = mousePos;
            }
        }

        public event EventHandler ResizingChanged;

        public event EventHandler<ResizingMousePosition> ResizingMousePositionChanged;
        
        public bool Resizing { get; private set; }

        private string _thumbStyleKey;

        private MovementDirectionHelper _movementDirectionHelper;
        
        // Resizing adorner uses Thumbs for visual elements.  
        // The Thumbs have built-in mouse input handling.
        Thumb _topLeftThumb, 
            _topRightThumb, 
            _bottomLeftThumb, 
            _bottomRightThumb, 
            _leftThumb, 
            _rightThumb, 
            _topThumb, 
            _bottomThumb;

        // To store and manage the adorner's visual children.
        VisualCollection _visualChildren;

        Canvas _parentCanvas;

        FrameworkElement _adornedFrameworkElement;

        bool _showMiddleHandles;
        
        // Initialize the ResizingAdorner.
        public ResizingAdorner(UIElement adornedElement, bool showMiddleHandles, string thumbStyleKey)
            : base(adornedElement)
        {
            _thumbStyleKey = thumbStyleKey;

            _showMiddleHandles = showMiddleHandles;
            
            _visualChildren = new VisualCollection(this);

            _movementDirectionHelper = new MovementDirectionHelper();

            
            _parentCanvas = LogicalTreeHelper.GetParent(adornedElement) as Canvas;
            if (_parentCanvas == null)
            {
                throw new NotSupportedException("The adorned element must be an imediate child of a Canvas control.");
            }

            _adornedFrameworkElement = AdornedElement as FrameworkElement;
            if (_adornedFrameworkElement == null)
            {
                throw new NotSupportedException("The adorned element should at least be of type 'FrameworkElement'");
            }

            // Call a helper method to initialize the Thumbs
            // with a customized cursors.
            BuildAdornerCorner(ref _topLeftThumb, Cursors.SizeNWSE);
            BuildAdornerCorner(ref _topRightThumb, Cursors.SizeNESW);
            BuildAdornerCorner(ref _bottomLeftThumb, Cursors.SizeNESW);
            BuildAdornerCorner(ref _bottomRightThumb, Cursors.SizeNWSE);

            // we want to make sure that the value of MousePosition is correct before we start dragging
            // because the consumer may count on the correctness of this value (as I do)
            _topLeftThumb.PreviewMouseLeftButtonDown += (s, e) =>
            { SetResizingPropDp(true); ReportMousePosition(); };
            _topLeftThumb.PreviewMouseLeftButtonUp += (s, e) => SetResizingPropDp(false);
            _topRightThumb.PreviewMouseLeftButtonDown += (s, e) =>
            { SetResizingPropDp(true); ReportMousePosition(); };
            _topRightThumb.PreviewMouseLeftButtonUp += (s, e) => SetResizingPropDp(false);
            _bottomLeftThumb.PreviewMouseLeftButtonDown += (s, e) =>
            { SetResizingPropDp(true); ReportMousePosition(); };
            _bottomLeftThumb.PreviewMouseLeftButtonUp += (s, e) => SetResizingPropDp(false);
            _bottomRightThumb.PreviewMouseLeftButtonDown += (s, e) =>
            { SetResizingPropDp(true); ReportMousePosition(); };
            _bottomRightThumb.PreviewMouseLeftButtonUp += (s, e) => SetResizingPropDp(false);


            // Add handlers for resizing.
            _bottomLeftThumb.DragDelta += new DragDeltaEventHandler(HandleBottomLeft);
            _bottomRightThumb.DragDelta += new DragDeltaEventHandler(HandleBottomRight);
            _topLeftThumb.DragDelta += new DragDeltaEventHandler(HandleTopLeft);
            _topRightThumb.DragDelta += new DragDeltaEventHandler(HandleTopRight);

            if (_showMiddleHandles)
            {
                BuildAdornerCorner(ref _bottomThumb, Cursors.SizeNS);
                BuildAdornerCorner(ref _topThumb, Cursors.SizeNS);
                BuildAdornerCorner(ref _leftThumb, Cursors.SizeWE);
                BuildAdornerCorner(ref _rightThumb, Cursors.SizeWE);
                
                _leftThumb.PreviewMouseLeftButtonDown += (s, e) =>
                { SetResizingPropDp(true); ReportMousePosition(); };
                _leftThumb.PreviewMouseLeftButtonUp += (s, e) => SetResizingPropDp(false);
                _rightThumb.PreviewMouseLeftButtonDown += (s, e) =>
                { SetResizingPropDp(true); ReportMousePosition(); };
                _rightThumb.PreviewMouseLeftButtonUp += (s, e) => SetResizingPropDp(false);
                _topThumb.PreviewMouseLeftButtonDown += (s, e) =>
                { SetResizingPropDp(true); ReportMousePosition(); };
                _topThumb.PreviewMouseLeftButtonUp += (s, e) => SetResizingPropDp(false);
                _bottomThumb.PreviewMouseLeftButtonDown += (s, e) =>
                { SetResizingPropDp(true); ReportMousePosition(); };
                _bottomThumb.PreviewMouseLeftButtonUp += (s, e) => SetResizingPropDp(false);
                
                _leftThumb.DragDelta += new DragDeltaEventHandler(HandleLeft);
                _rightThumb.DragDelta += new DragDeltaEventHandler(HandleRight);
                _topThumb.DragDelta += new DragDeltaEventHandler(HandleTop);
                _bottomThumb.DragDelta += new DragDeltaEventHandler(HandleBottom);

                ManageMiddleHandlesVisibility(adornedElement.RenderSize);
                (adornedElement as FrameworkElement).SizeChanged += (s, args)
                 =>
                {
                    ManageMiddleHandlesVisibility(args.NewSize);
                };
            }
            
        }

        private void ManageMiddleHandlesVisibility(Size currentSize)
        {
            if (currentSize.Height < 50)
            {
                _leftThumb.Visibility = Visibility.Collapsed;
                _rightThumb.Visibility = Visibility.Collapsed;
            }
            else
            {
                _leftThumb.Visibility = Visibility.Visible;
                _rightThumb.Visibility = Visibility.Visible;
            }

            if (currentSize.Width < 50)
            {
                _topThumb.Visibility = Visibility.Collapsed;
                _bottomThumb.Visibility = Visibility.Collapsed;
            }
            else
            {
                _topThumb.Visibility = Visibility.Visible;
                _bottomThumb.Visibility = Visibility.Visible;
            }
        }

        private void ReportMousePosition(Point p)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Background,
                new Action(() =>
                {
                    ResizingMousePositionChanged?.Invoke(this, new ResizingMousePosition(p));
                }));
        }

        private void ReportMousePosition()
        {
            Point p = Mouse.GetPosition(_parentCanvas);
            
            ReportMousePosition(new Point
            {
                X = p.X,
                Y = p.Y
            });
        }

        private void SetResizingPropDp(bool newValue)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Background,
                new Action(() =>
                {
                    Resizing = newValue;
                    ResizingChanged?.Invoke(this, new EventArgs());
                }));
        }

        private void _slidingTimer_TimeoutElapsed(object sender, EventArgs e)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Background,
                new Action(() =>
                {
                    
                    Resizing = false;
                    ResizingChanged?.Invoke(this, new EventArgs());

                }));
        }

        // Handler for resizing from the bottom-right.
        void HandleBottom(object sender, DragDeltaEventArgs args)
        {
            Thumb hitThumb = sender as Thumb;
            
            Point mousePos = Mouse.GetPosition(_parentCanvas);
            ReportMousePosition(new Point
            {
                X = mousePos.X, // + args.HorizontalChange,
                Y = mousePos.Y + args.VerticalChange
            });
            // Ensure that the Width and Height are properly initialized after the resize.
            EnforceSize(_adornedFrameworkElement);

            // Change the size by the amount the user drags the mouse, as long as it's larger 
            // than the width or height of an adorner, respectively.

            double newHeight = Math.Max(args.VerticalChange + _adornedFrameworkElement.Height, hitThumb.DesiredSize.Height);
            if (Canvas.GetTop(_adornedFrameworkElement).GetZeroIfNaN() + newHeight <= _parentCanvas.ActualHeight)
            {
                _adornedFrameworkElement.Height = newHeight;
            }

            if (newHeight <= _topLeftThumb.ActualHeight)
            {
                MoveMouseDownTo(_topThumb);
            }
        }

        // Handler for resizing from the bottom-right.
        void HandleBottomRight(object sender, DragDeltaEventArgs args)
        {
            Thumb hitThumb = sender as Thumb;
            
            Point mousePos = Mouse.GetPosition(_parentCanvas);
            _movementDirectionHelper.Feed(mousePos);

            ReportMousePosition(new Point
            {
                X = mousePos.X + args.HorizontalChange,
                Y = mousePos.Y + args.VerticalChange
            });
            // Ensure that the Width and Height are properly initialized after the resize.
            EnforceSize(_adornedFrameworkElement);

            // Change the size by the amount the user drags the mouse, as long as it's larger 
            // than the width or height of an adorner, respectively.
            double newWidth = Math.Max(_adornedFrameworkElement.Width + args.HorizontalChange, hitThumb.DesiredSize.Width);
            if (Canvas.GetLeft(_adornedFrameworkElement).GetZeroIfNaN() + newWidth <= _parentCanvas.ActualWidth)
            {
                _adornedFrameworkElement.Width = newWidth;
            }

            double newHeight = Math.Max(args.VerticalChange + _adornedFrameworkElement.Height, hitThumb.DesiredSize.Height);
            if (Canvas.GetTop(_adornedFrameworkElement).GetZeroIfNaN() + newHeight <= _parentCanvas.ActualHeight)
            {
                _adornedFrameworkElement.Height = newHeight;
            }

            if (_adornedFrameworkElement.Width <= _topLeftThumb.ActualWidth || _adornedFrameworkElement.Height <= _topLeftThumb.ActualHeight)
            {
                switch (_movementDirectionHelper.Direction)
                {
                    case MovementDirectionHelper.MovementDirection.TopRight:
                    case MovementDirectionHelper.MovementDirection.Top:
                        MoveMouseDownTo(_topRightThumb);
                        break;
                    case MovementDirectionHelper.MovementDirection.TopLeft:
                        MoveMouseDownTo(_topLeftThumb);
                        break;
                    case MovementDirectionHelper.MovementDirection.BottomLeft:
                    case MovementDirectionHelper.MovementDirection.Left:
                        MoveMouseDownTo(_bottomLeftThumb);
                        break;
                }
            }
        }


        // Handler for resizing from the top-right.
        void HandleRight(object sender, DragDeltaEventArgs args)
        {
            Thumb hitThumb = sender as Thumb;
            
            Point mousePos = Mouse.GetPosition(_parentCanvas);
            ReportMousePosition(new Point
            {
                X = mousePos.X + args.HorizontalChange,
                Y = mousePos.Y // + args.VerticalChange
            });
            // Ensure that the Width and Height are properly initialized after the resize.
            EnforceSize(_adornedFrameworkElement);

            // Change the size by the amount the user drags the mouse, as long as it's larger 
            // than the width or height of an adorner, respectively.
            double newWidth = Math.Max(_adornedFrameworkElement.Width + args.HorizontalChange, hitThumb.DesiredSize.Width);
            if (Canvas.GetLeft(_adornedFrameworkElement).GetZeroIfNaN() + newWidth <= _parentCanvas.ActualWidth)
            {
                _adornedFrameworkElement.Width = newWidth;
            }

            if (_adornedFrameworkElement.Width <= _rightThumb.ActualWidth)
            {
                MoveMouseDownTo(_leftThumb);
            }
        }

        void MoveMouseDownTo(FrameworkElement el)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.ContextIdle, new Action(() =>
            {
                WinApiHelpers.Mouse.MouseEvent(WinApiHelpers.Mouse.MouseEventFlags.LEFTUP);
                // TODO: multiple monitors?
                var location = el.PointToScreen(new Point(0, 0));
                location.X += el.ActualWidth / 2;
                location.Y += el.ActualHeight / 2;
                WinApiHelpers.Mouse.MouseEvent(WinApiHelpers.Mouse.MouseEventFlags.LEFTDOWN, (int)location.X, (int)location.Y);
            }));
        }

        void Swap<T>(ref T obj1, ref T obj2)
        {
            var tmp = obj1;
            obj2 = obj1;
            obj2 = tmp;
        }

        // Handler for resizing from the top-right.
        void HandleTop(object sender, DragDeltaEventArgs args)
        {
            Thumb hitThumb = sender as Thumb;
            
            Point mousePos = Mouse.GetPosition(_parentCanvas);
            ReportMousePosition(new Point
            {
                X = mousePos.X, // + args.HorizontalChange,
                Y = mousePos.Y + args.VerticalChange
            });
            // Ensure that the Width and Height are properly initialized after the resize.
            EnforceSize(_adornedFrameworkElement);
            
            double oldHeight = _adornedFrameworkElement.Height;
            double newHeight = Math.Max(_adornedFrameworkElement.Height - args.VerticalChange, hitThumb.DesiredSize.Height);
            double oldTop = Canvas.GetTop(_adornedFrameworkElement).GetZeroIfNaN();
            double newTop = oldTop - (newHeight - oldHeight);
            
            if (newTop >= 0)
            {
                _adornedFrameworkElement.Height = newHeight;
                Canvas.SetTop(_adornedFrameworkElement, newTop);
            }

            if (newHeight <= _topLeftThumb.ActualHeight)
            {
                MoveMouseDownTo(_bottomThumb);
            }
        }

        // Handler for resizing from the top-right.
        void HandleTopRight(object sender, DragDeltaEventArgs args)
        {
            Thumb hitThumb = sender as Thumb;
            
            Point mousePos = Mouse.GetPosition(_parentCanvas);
            _movementDirectionHelper.Feed(mousePos);
            ReportMousePosition(new Point
            {
                X = mousePos.X + args.HorizontalChange,
                Y = mousePos.Y + args.VerticalChange
            });
            // Ensure that the Width and Height are properly initialized after the resize.
            EnforceSize(_adornedFrameworkElement);

            // Change the size by the amount the user drags the mouse, as long as it's larger 
            // than the width or height of an adorner, respectively.
            double newWidth = Math.Max(_adornedFrameworkElement.Width + args.HorizontalChange, hitThumb.DesiredSize.Width);
            if (Canvas.GetLeft(_adornedFrameworkElement).GetZeroIfNaN() + newWidth <= _parentCanvas.ActualWidth)
            {
                _adornedFrameworkElement.Width = newWidth;
            }

            double oldHeight = _adornedFrameworkElement.Height;
            double newHeight = Math.Max(_adornedFrameworkElement.Height - args.VerticalChange, hitThumb.DesiredSize.Height);
            double oldTop = Canvas.GetTop(_adornedFrameworkElement).GetZeroIfNaN();
            double newTop = oldTop - (newHeight - oldHeight);

            if (newTop >= 0)
            {
                _adornedFrameworkElement.Height = newHeight;
                Canvas.SetTop(_adornedFrameworkElement, newTop);
            }

            if (_adornedFrameworkElement.Width <= _topLeftThumb.ActualWidth || _adornedFrameworkElement.Height <= _topLeftThumb.ActualHeight)
            {
                switch (_movementDirectionHelper.Direction)
                {
                    case MovementDirectionHelper.MovementDirection.BottomRight:
                    case MovementDirectionHelper.MovementDirection.Bottom:
                        MoveMouseDownTo(_bottomRightThumb);
                        break;
                    case MovementDirectionHelper.MovementDirection.BottomLeft:
                        MoveMouseDownTo(_bottomLeftThumb);
                        break;
                    case MovementDirectionHelper.MovementDirection.TopLeft:
                    case MovementDirectionHelper.MovementDirection.Left:
                        MoveMouseDownTo(_topLeftThumb);
                        break;
                }
            }
        }

        // Handler for resizing from the top-left.
        void HandleLeft(object sender, DragDeltaEventArgs args)
        {
            Thumb hitThumb = sender as Thumb;
            
            Point mousePos = Mouse.GetPosition(_parentCanvas);
            ReportMousePosition(new Point
            {
                X = mousePos.X + args.HorizontalChange,
                Y = mousePos.Y // + args.VerticalChange
            });
            // Ensure that the Width and Height are properly initialized after the resize.
            EnforceSize(_adornedFrameworkElement);
            
            double oldWidth = _adornedFrameworkElement.Width;
            double newWidth = Math.Max(_adornedFrameworkElement.Width - args.HorizontalChange, hitThumb.DesiredSize.Width);
            double oldLeft = Canvas.GetLeft(_adornedFrameworkElement).GetZeroIfNaN();
            double newLeft = oldLeft - (newWidth - oldWidth);

            if (newLeft >= 0)
            {
                _adornedFrameworkElement.Width = newWidth;
                Canvas.SetLeft(_adornedFrameworkElement, newLeft);
            }
            if (_adornedFrameworkElement.Width <= _leftThumb.ActualWidth)
            {
                MoveMouseDownTo(_rightThumb);
            }
        }

        // Handler for resizing from the top-left.
        void HandleTopLeft(object sender, DragDeltaEventArgs args)
        {
            Thumb hitThumb = sender as Thumb;
            
            Point mousePos = Mouse.GetPosition(_parentCanvas);
            _movementDirectionHelper.Feed(mousePos);

            ReportMousePosition(new Point
            {
                X = mousePos.X + args.HorizontalChange,
                Y = mousePos.Y + args.VerticalChange
            });
            // Ensure that the Width and Height are properly initialized after the resize.
            EnforceSize(_adornedFrameworkElement);
            
            double oldWidth = _adornedFrameworkElement.Width;
            double newWidth = Math.Max(_adornedFrameworkElement.Width - args.HorizontalChange, hitThumb.DesiredSize.Width);
            double oldLeft = Canvas.GetLeft(_adornedFrameworkElement).GetZeroIfNaN();
            double newLeft = oldLeft - (newWidth - oldWidth);

            if (newLeft >= 0)
            {
                _adornedFrameworkElement.Width = newWidth;
                Canvas.SetLeft(_adornedFrameworkElement, newLeft);
            }

            double oldHeight = _adornedFrameworkElement.Height;
            double newHeight = Math.Max(_adornedFrameworkElement.Height - args.VerticalChange, hitThumb.DesiredSize.Height);
            double oldTop = Canvas.GetTop(_adornedFrameworkElement).GetZeroIfNaN();
            double newTop = oldTop - (newHeight - oldHeight);

            if (newTop >= 0)
            {
                _adornedFrameworkElement.Height = newHeight;
                Canvas.SetTop(_adornedFrameworkElement, newTop);
            }

            if (_adornedFrameworkElement.Width <= _topLeftThumb.ActualWidth || _adornedFrameworkElement.Height <= _topLeftThumb.ActualHeight)
            {
                switch (_movementDirectionHelper.Direction)
                {
                    case MovementDirectionHelper.MovementDirection.BottomRight:
                        MoveMouseDownTo(_bottomRightThumb);
                        break;
                    case MovementDirectionHelper.MovementDirection.BottomLeft:
                        MoveMouseDownTo(_bottomLeftThumb);
                        break;
                    case MovementDirectionHelper.MovementDirection.TopRight:
                    case MovementDirectionHelper.MovementDirection.Right:
                        MoveMouseDownTo(_topRightThumb);
                        break;
                }
            }
        }

        // Handler for resizing from the bottom-left.
        void HandleBottomLeft(object sender, DragDeltaEventArgs args)
        {
            Thumb hitThumb = sender as Thumb;

            Point mousePos = Mouse.GetPosition(_parentCanvas);
            _movementDirectionHelper.Feed(mousePos);

            ReportMousePosition(new Point
            {
                X = mousePos.X + args.HorizontalChange,
                Y = mousePos.Y + args.VerticalChange
            });

            // Ensure that the Width and Height are properly initialized after the resize.
            EnforceSize(_adornedFrameworkElement);
            
            double newHeight = Math.Max(args.VerticalChange + _adornedFrameworkElement.Height, hitThumb.DesiredSize.Height);
            if (Canvas.GetTop(_adornedFrameworkElement).GetZeroIfNaN() + newHeight <= _parentCanvas.ActualHeight)
            {
                _adornedFrameworkElement.Height = newHeight;
            }

            double oldWidth = _adornedFrameworkElement.Width;
            double newWidth = Math.Max(_adornedFrameworkElement.Width - args.HorizontalChange, hitThumb.DesiredSize.Width);
            double oldLeft = Canvas.GetLeft(_adornedFrameworkElement).GetZeroIfNaN();
            double newLeft = oldLeft - (newWidth - oldWidth);

            if (newLeft >= 0)
            {
                _adornedFrameworkElement.Width = newWidth;
                Canvas.SetLeft(_adornedFrameworkElement, newLeft);
            }

            if (_adornedFrameworkElement.Width <= _topLeftThumb.ActualWidth || _adornedFrameworkElement.Height <= _topLeftThumb.ActualHeight)
            {
                switch (_movementDirectionHelper.Direction)
                {
                    case MovementDirectionHelper.MovementDirection.BottomRight:
                    case MovementDirectionHelper.MovementDirection.Right:
                        MoveMouseDownTo(_bottomRightThumb);
                        break;
                    case MovementDirectionHelper.MovementDirection.TopLeft:
                    case MovementDirectionHelper.MovementDirection.Left:
                        MoveMouseDownTo(_topLeftThumb);
                        break;
                    case MovementDirectionHelper.MovementDirection.TopRight:
                        MoveMouseDownTo(_topRightThumb);
                        break;
                }
            }
        }
        // Arrange the Adorners.
        protected override Size ArrangeOverride(Size finalSize)
        {
            // desiredWidth and desiredHeight are the width and height of the element that's being adorned.  
            // These will be used to place the ResizingAdorner at the corners of the adorned element.  
            double desiredWidth = AdornedElement.DesiredSize.Width;
            double desiredHeight = AdornedElement.DesiredSize.Height;
            // adornerWidth & adornerHeight are used for placement as well.
            double adornerWidth = this.DesiredSize.Width;
            double adornerHeight = this.DesiredSize.Height;

            _topLeftThumb.Arrange(new Rect(-adornerWidth / 2, -adornerHeight / 2, adornerWidth, adornerHeight));
            _topRightThumb.Arrange(new Rect(desiredWidth - adornerWidth / 2, -adornerHeight / 2, adornerWidth, adornerHeight));
            _bottomLeftThumb.Arrange(new Rect(-adornerWidth / 2, desiredHeight - adornerHeight / 2, adornerWidth, adornerHeight));
            _bottomRightThumb.Arrange(new Rect(desiredWidth - adornerWidth / 2, desiredHeight - adornerHeight / 2, adornerWidth, adornerHeight));

            if (_showMiddleHandles)
            {
                _leftThumb.Arrange(new Rect(-adornerWidth / 2, desiredHeight / 2 - adornerHeight / 2, adornerWidth, adornerHeight));
                _rightThumb.Arrange(new Rect(desiredWidth - adornerWidth / 2, desiredHeight / 2 - adornerHeight / 2, adornerWidth, adornerHeight));
                _topThumb.Arrange(new Rect(desiredWidth / 2 - adornerWidth / 2, -adornerHeight / 2, adornerWidth, adornerHeight));
                _bottomThumb.Arrange(new Rect(desiredWidth / 2 - adornerWidth / 2, desiredHeight - adornerHeight / 2, adornerWidth, adornerHeight));
            }
            // Return the final size.
            return finalSize;
        }

        // Helper method to instantiate the corner Thumbs, set the Cursor property, 
        // set some appearance properties, and add the elements to the visual tree.
        void BuildAdornerCorner(ref Thumb cornerThumb, Cursor customizedCursor)
        {
            if (cornerThumb != null) return;

            cornerThumb = new Thumb();
            cornerThumb.Cursor = customizedCursor;

            if (string.IsNullOrEmpty(_thumbStyleKey))
            {
                // Set some arbitrary visual characteristics.
                cornerThumb.Height = cornerThumb.Width = 10;
                cornerThumb.Opacity = 0.40;
                cornerThumb.Background = new SolidColorBrush(Colors.MediumBlue);
            }
            else
            {
                cornerThumb.SetResourceReference(Thumb.StyleProperty, _thumbStyleKey);
            }
            
            _visualChildren.Add(cornerThumb);
        }

        // This method ensures that the Widths and Heights are initialized.  Sizing to content produces
        // Width and Height values of Double.NaN.  Because this Adorner explicitly resizes, the Width and Height
        // need to be set first.  It also sets the maximum size of the adorned element.
        void EnforceSize(FrameworkElement adornedElement)
        {
            if (adornedElement.Width.Equals(Double.NaN))
                adornedElement.Width = adornedElement.DesiredSize.Width;
            if (adornedElement.Height.Equals(Double.NaN))
                adornedElement.Height = adornedElement.DesiredSize.Height;

            FrameworkElement parent = adornedElement.Parent as FrameworkElement;
            if (parent != null)
            {
                adornedElement.MaxHeight = parent.ActualHeight;
                adornedElement.MaxWidth = parent.ActualWidth;
            }
        }
        // Override the VisualChildrenCount and GetVisualChild properties to interface with 
        // the adorner's visual collection.
        protected override int VisualChildrenCount { get { return _visualChildren.Count; } }
        protected override Visual GetVisualChild(int index) { return _visualChildren[index]; }
        
    }
}
