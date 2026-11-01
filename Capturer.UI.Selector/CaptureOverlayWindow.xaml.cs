using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Diagnostics;
using static Capturer.UI.Technical.Behaviors.CanvasElementFollowMouseBehavior;
using Capturer.UI.Input.Interfaces;
using Capturer.UI.Base.Extentions;
using Capturer.UI.Base.Helpers;

namespace Capturer.UI.Selector
{

    /// <summary>
    /// Interaction logic for CaptureOverlayWindow.xaml
    /// </summary>
    public partial class CaptureOverlayWindow : Window, IDisposable
    {
        public readonly System.Drawing.Size MagnifierCropSliceSize = new System.Drawing.Size(16, 16);

        private WinApiHelpers.Mouse.Cursor _systemCursorObject;
        private bool _isSystemCursorVisible;
        private bool _isVertScrollBtnVisible;
        private bool _isHorizScrollBtnVisible;

        private bool _isDragSelectionOngoing;

        public event EventHandler VertScrollBtnClicked;
        public event EventHandler HorizScrollBtnClicked;
        public event EventHandler SelectionClicked;
        public event EventHandler DragSelectionStarted;
        public event EventHandler DragSelectionStopped;
        
        public enum HideCursorMode
        {
            WPFEmptyCursor,
            SystemEmptyCursor
        }
        
        public enum SelectorTypes
        {
            ManualSelector,
            AutoSelector
        }
        
        public bool IsEventTransparent { get; private set; }

        public System.Drawing.Bitmap BackgroundImage { get; private set; }

        private IMouseInput _mouseInput;

        private ICursorController _cursorController;

        public CaptureOverlayWindow(IMouseInput mouseInput,
            ICursorController cursorController,
            System.Drawing.Bitmap backgroundImage = null,
            bool isEventTransparent = false)
        {
            _mouseInput = mouseInput;
            _mouseInput.SetWindow(this);

            _cursorController = cursorController;
            _cursorController.SetWindow(this);

            IsEventTransparent = isEventTransparent;
            BackgroundImage = backgroundImage;

            InitializeComponent();
            mainCanvas.DataContext = this;

            this.Loaded += CaptureOverlayWindow_Loaded;
        }

        private void CaptureOverlayWindow_Loaded(object sender, RoutedEventArgs e)
        {

            // this.WindowStyle = WindowStyle.None;
            // ensure full screen on multiple monitor systems

            this.WindowState = WindowState.Normal;
            this.Top = SystemParameters.VirtualScreenTop;
            this.Width = SystemParameters.VirtualScreenWidth;
            this.Height = SystemParameters.VirtualScreenHeight;
            this.Left = SystemParameters.VirtualScreenLeft;
            
            backgroundImg.Source = ImageSourceHelper.BitmapToImageSource(BackgroundImage);

            var mousePosPoint = WinApiHelpers.Mouse.GetMousePosition();
            MousePosition = this.PointFromScreen(new Point(mousePosPoint.X, mousePosPoint.Y));
            //HwndSource hwndSource = PresentationSource.FromVisual(this) as HwndSource;

            //HwndTarget hwndTarget = hwndSource.CompositionTarget;

            //hwndTarget.RenderMode = RenderMode.SoftwareOnly;
        }
        
        private void CaptureOverlayWindow_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            var newPos = e.GetPosition(mainCanvas);
            UpdateMousePositionRelatedStuff(newPos);
        }
        
        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            if (IsEventTransparent)
            {
                var hwnd = new WindowInteropHelper(this).Handle;
                WinApiHelpers.WindowApis.SetWindowEventPassThrough(hwnd);
            }

            _mouseInput.MouseLeftButtonUp += _mouseInput_MouseLeftButtonUp;
            _mouseInput.MouseLeftButtonDown += _mouseInput_MouseLeftButtonDown;
            _mouseInput.MouseMove += _mouseInput_MouseMove;
            
            HideCursor();

            PreviewKeyDown += CaptureOverlayWindow_PreviewKeyDown;
            
        }

        private void _mouseInput_MouseMove(object sender, Input.EventArgs.MouseCancelEventArgs e)
        {
            var newPos = e.MousePosition;
            UpdateMousePositionRelatedStuff(newPos);
        }

        private void _mouseInput_MouseLeftButtonDown(object sender, Input.EventArgs.MouseCancelEventArgs e)
        {
            UpdateLeftMouseBtnState(true);
        }

        private void _mouseInput_MouseLeftButtonUp(object sender, Input.EventArgs.MouseCancelEventArgs e)
        {
            UpdateLeftMouseBtnState(false);
        }

        private void CaptureOverlayWindow_MouseUp(object sender, MouseButtonEventArgs e)
        {
            UpdateLeftMouseBtnState(false);
        }

        private void CaptureOverlayWindow_MouseDown(object sender, MouseButtonEventArgs e)
        {
            UpdateLeftMouseBtnState(true);
        }

        private void InputEventsHook_MouseAction(WinApiHelpers.InputEventsHook.MouseEventArgs obj)
        {
            // do nothing if the window has just been closed
            if (!this.IsVisible)
                return;

            var newPos = new System.Windows.Point(obj.Point.X, obj.Point.Y);
            if (obj.MouseAction == WinApiHelpers.Win32Enums.MouseMessages.WM_MOUSEMOVE)
            {
                var positionInsideWindow = this.PointFromScreen(newPos);
                UpdateMousePositionRelatedStuff(positionInsideWindow);
            }
            else if (obj.MouseAction == WinApiHelpers.Win32Enums.MouseMessages.WM_LBUTTONDOWN)
            {
                UpdateLeftMouseBtnState(true);
            }
            else if (obj.MouseAction == WinApiHelpers.Win32Enums.MouseMessages.WM_LBUTTONUP)
            {
                UpdateLeftMouseBtnState(false);
            }
        }
        
        private void CaptureOverlayWindow_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                (sender as Window).Close();
            }
        }


        private void UpdateLeftMouseBtnState(bool isDown)
        {
            var vertScrollEntranceRect = new Rect
                    (Canvas.GetLeft(vertScroll), Canvas.GetTop(vertScroll), vertScroll.ActualWidth, vertScroll.ActualHeight);

            //ScreenCoordinatesHelper.GetRectOfFrameworkElement(vertScroll);
            var horizScrollEntranceRect = new Rect
                    (Canvas.GetLeft(horizScroll), Canvas.GetTop(horizScroll), horizScroll.ActualWidth, horizScroll.ActualHeight);

            //ScreenCoordinatesHelper.GetRectOfFrameworkElement(horizScroll);
            // we want mouse up here
            if (!isDown)
            {
                if (_isDragSelectionOngoing)
                {
                    DragSelectionStopped?.Invoke(this, new EventArgs());
                    _isDragSelectionOngoing = false;
                }

                if (vertScrollEntranceRect.Contains(MousePosition))
                {
                    VertScrollBtnClicked?.Invoke(this, new EventArgs());
                }
                else if (horizScrollEntranceRect.Contains(MousePosition))
                {
                    HorizScrollBtnClicked?.Invoke(this, new EventArgs());
                }
                else
                {
                    SelectionClicked?.Invoke(this, new EventArgs());
                }
            }
            else
            {
                // mouse left button is down
                MouseSelectionBoxStartPosition = MousePosition;
            }
        }

        public void SwitchToDragSelectionMode()
        {
            selectorInnerControl.Width = 0;
            selectorInnerControl.Height = 0;
            indicator.Visibility = Visibility.Visible;
            HideScrollbarControlsIfVisible();
            MouseDragSelectionMode = true;
        }

        public void SwitchToManualSelectionMode()
        {
            if (!ManualSelectionMode)
            {
                MouseDragSelectionMode = false;
                SelectorBorderStyleIndex = (int)SelectorBorderStyleIndices.Solid;
                ScreenwideCrossVisibility = Visibility.Collapsed;
                indicator.Visibility = Visibility.Visible;
                ManualSelectionMode = true;
                magnifier.Visibility = Visibility.Collapsed;
                RestoreCursor();
                HideScrollbarControlsIfVisible();
            }
        }

        private void HideCursor()
        {
            _cursorController.HideCursor();
            
            _isSystemCursorVisible = false;
        }

        private void RestoreCursor()
        {
            _cursorController.RestoreDefaultCursor();

            _isSystemCursorVisible = true;
        }

        public double SelectBoxNewHeight
        {
            get { return (double)GetValue(SelectBoxNewHeightProperty); }
            set { SetValue(SelectBoxNewHeightProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SelectBoxNewHeight.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectBoxNewHeightProperty =
            DependencyProperty.Register("SelectBoxNewHeight", typeof(double), typeof(CaptureOverlayWindow), new PropertyMetadata(0d));


        public double SelectBoxNewWidth
        {
            get { return (double)GetValue(SelectBoxNewWidthProperty); }
            set { SetValue(SelectBoxNewWidthProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SelectBoxNewHeight.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectBoxNewWidthProperty =
            DependencyProperty.Register("SelectBoxNewWidth", typeof(double), typeof(CaptureOverlayWindow), new PropertyMetadata(0d));

        public double SelectBoxNewCanvasTop
        {
            get { return (double)GetValue(SelectBoxNewCanvasTopProperty); }
            set { SetValue(SelectBoxNewCanvasTopProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SelectBoxNewHeight.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectBoxNewCanvasTopProperty =
            DependencyProperty.Register("SelectBoxNewCanvasTop", typeof(double), typeof(CaptureOverlayWindow), new PropertyMetadata(0d));

        public double SelectBoxNewCanvasLeft
        {
            get { return (double)GetValue(SelectBoxNewCanvasLeftProperty); }
            set { SetValue(SelectBoxNewCanvasLeftProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SelectBoxNewHeight.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectBoxNewCanvasLeftProperty =
            DependencyProperty.Register("SelectBoxNewCanvasLeft", typeof(double), typeof(CaptureOverlayWindow), new PropertyMetadata(0d));


        public enum SelectorBorderStyleIndices
        {
            MarchingAnts,
            Solid
        }

        public int SelectorBorderStyleIndex
        {
            get { return (int)GetValue(SelectorBorderStyleIndexProperty); }
            set { SetValue(SelectorBorderStyleIndexProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SelectorBorderType.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectorBorderStyleIndexProperty =
            DependencyProperty.Register("SelectorBorderStyleIndex", typeof(int), typeof(CaptureOverlayWindow), new PropertyMetadata((int)SelectorBorderStyleIndices.MarchingAnts));




        public bool MouseDragSelectionMode
        {
            get { return (bool)GetValue(MouseDragSelectionModeProperty); }
            set { SetValue(MouseDragSelectionModeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MouseSelectionMode.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MouseDragSelectionModeProperty =
            DependencyProperty.Register("MouseDragSelectionMode", typeof(bool), typeof(CaptureOverlayWindow), new PropertyMetadata(false));



        public Point MouseSelectionBoxStartPosition
        {
            get { return (Point)GetValue(MouseSelectionBoxStartPositionProperty); }
            set { SetValue(MouseSelectionBoxStartPositionProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MouseSelectionBoxStartPosition.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MouseSelectionBoxStartPositionProperty =
            DependencyProperty.Register("MouseSelectionBoxStartPosition", typeof(Point), typeof(CaptureOverlayWindow), new PropertyMetadata(new Point(300, 200)));



        public Point MousePosition
        {
            get { return (Point)GetValue(MousePositionProperty); }
            set { SetValue(MousePositionProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MousePosition.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MousePositionProperty =
            DependencyProperty.Register("MousePosition", typeof(System.Windows.Point), typeof(CaptureOverlayWindow), new PropertyMetadata(new Point(), MousePositionChanged));


        private void UpdateMagnifierImage(Point corrdinates)
        {
            // this has a lower priority and should not block the mouse hook
            Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Input, new Action(
                () =>
                {
                    var drawingPoint = new System.Drawing.Point((int)corrdinates.X, (int)corrdinates.Y);
                    if (magnifier.MagnifiedImage != null)
                    {
                        magnifier.MagnifiedImage.Dispose();
                    }
                    try
                    {
                        //magnifier.MagnifiedImage = BackgroundImage.Clone(new System.Drawing.Rectangle(drawingPoint, new System.Drawing.Size(16, 16)), BackgroundImage.PixelFormat);


                        var cropRect = new System.Drawing.Rectangle(drawingPoint, MagnifierCropSliceSize);

                        System.Drawing.Bitmap target = new System.Drawing.Bitmap(MagnifierCropSliceSize.Width, MagnifierCropSliceSize.Height);

                        using (System.Drawing.Graphics graphics = System.Drawing.Graphics.FromImage(target))
                        {
                            graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                            graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
                            graphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighSpeed;
                            graphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
                            graphics.DrawImage(BackgroundImage, new System.Drawing.Rectangle(0, 0, target.Width, target.Height),
                                             cropRect,
                                             System.Drawing.GraphicsUnit.Pixel);
                        }

                        magnifier.MagnifiedImage = target;

                    }
                    catch (OutOfMemoryException)
                    {
                        // TODO: figure this out (may have already been solved by using Graphics instead of Bitmap.Clone())
                    }
                }));
        }

        private void UpdateMousePositionRelatedStuff(Point newPos)
        {

            MousePosition = newPos;

            if (_mouseInput.Dragging && !_isDragSelectionOngoing && !ManualSelectionMode)
            {
                DragSelectionStarted?.Invoke(this, new EventArgs());
                _isDragSelectionOngoing = true;
            }

            if (_isDragSelectionOngoing)
            {
                return;
            }

            // this has a lower priority and should not block the mouse hook
            Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Background, new Action(
                () =>
                {
                    
                    if (vertScroll.Visibility == Visibility.Visible || horizScroll.Visibility == Visibility.Visible)
                    {
                        var vertScrollEntranceRect = new Rect
                            (Canvas.GetLeft(vertScroll), Canvas.GetTop(vertScroll), vertScroll.ActualWidth, vertScroll.ActualHeight);
                        //ScreenCoordinatesHelper.GetRectOfFrameworkElement(vertScroll);
                        var horizScrollEntranceRect = new Rect
                            (Canvas.GetLeft(horizScroll), Canvas.GetTop(horizScroll), horizScroll.ActualWidth, horizScroll.ActualHeight);

                        //ScreenCoordinatesHelper.GetRectOfFrameworkElement(horizScroll);
                        var vertScrollExitRect = new Rect(vertScrollEntranceRect.Left - 5,
                            vertScrollEntranceRect.Top - 15,
                            vertScrollEntranceRect.Width + 5,
                            vertScrollEntranceRect.Height + 15);

                        var horizScrollExitRect = new Rect(horizScrollEntranceRect.Left - 15,
                           horizScrollEntranceRect.Top - 5,
                           horizScrollEntranceRect.Width + 15,
                           horizScrollEntranceRect.Height + 5);

                        if (vertScroll.Visibility == Visibility.Visible && vertScrollEntranceRect.Contains(newPos))
                        {
                            RestoreCursor();
                            ScreenwideCrossVisibility = Visibility.Hidden;
                            magnifier.Visibility = Visibility.Hidden;
                            // stop the button animation (it is active only when the button is not enabled)
                            vertScroll.IsEnabled = true;
                            vScrollLabel.Visibility = Visibility.Visible;
                            Canvas.SetLeft(vScrollLabel, newPos.X - vScrollLabel.ActualWidth);
                            Canvas.SetTop(vScrollLabel, newPos.Y - vScrollLabel.ActualHeight - vertScroll.ActualHeight);

                            if (_isHorizScrollBtnVisible)
                            {
                                horizScroll.Visibility = Visibility.Hidden;
                            }
                        }
                        else if (horizScroll.Visibility == Visibility.Visible && horizScrollEntranceRect.Contains(newPos))
                        {
                            RestoreCursor();
                            ScreenwideCrossVisibility = Visibility.Hidden;
                            magnifier.Visibility = Visibility.Hidden;
                            // stop the button animation (it is active only when the button is not enabled)
                            horizScroll.IsEnabled = true;
                            hScrollLabel.Visibility = Visibility.Visible;
                            Canvas.SetLeft(hScrollLabel, newPos.X);
                            Canvas.SetTop(hScrollLabel, newPos.Y - hScrollLabel.ActualHeight - horizScroll.ActualHeight);
                            if (_isVertScrollBtnVisible)
                            {
                                vertScroll.Visibility = Visibility.Hidden;
                            }
                        }
                        else if (_isSystemCursorVisible && !vertScrollExitRect.Contains(newPos) && !horizScrollExitRect.Contains(newPos))
                        {
                            HideCursor();
                            ScreenwideCrossVisibility = Visibility.Visible;
                            magnifier.Visibility = Visibility.Visible;
                            horizScroll.IsEnabled = false;
                            if (_isHorizScrollBtnVisible)
                            {
                                horizScroll.Visibility = Visibility.Visible;
                            }
                            vertScroll.IsEnabled = false;
                            if (_isVertScrollBtnVisible)
                            {
                                vertScroll.Visibility = Visibility.Visible;
                            }
                            hScrollLabel.Visibility = Visibility.Collapsed;
                            vScrollLabel.Visibility = Visibility.Collapsed;
                        }
                    }
                    else
                    {
                        vScrollLabel.Visibility = Visibility.Collapsed;
                        hScrollLabel.Visibility = Visibility.Collapsed;

                        if (ManualSelectionMode)
                        {
                            var canvasLeft = Canvas.GetLeft(selectorInnerControl).GetZeroIfNaN();
                            var canvasTop = Canvas.GetTop(selectorInnerControl).GetZeroIfNaN();
                            
                            var selectorInnerControlRect = new Rect
                            (canvasLeft, canvasTop, selectorInnerControl.ActualWidth, selectorInnerControl.ActualHeight);
                            //if (ScreenCoordinatesHelper.GetRectOfFrameworkElement(selectorInnerControl).Contains(newPos))
                            if (selectorInnerControlRect.Contains(newPos))
                                Cursor = Cursors.SizeAll;
                            else
                                RestoreCursor();
                        }

                    }

                }));
        }

        private static void MousePositionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
        }


        public Point CrosshairActualPosition
        {
            get { return (Point)GetValue(CrosshairActualPositionProperty); }
            set { SetValue(CrosshairActualPositionProperty, value); }
        }

        // Using a DependencyProperty as the backing store for CrosshairActualPosition.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CrosshairActualPositionProperty =
            DependencyProperty.Register("CrosshairActualPosition", typeof(System.Windows.Point), typeof(CaptureOverlayWindow), new PropertyMetadata(new Point(), CrosshairActualPositionChanged));

        public event EventHandler<Point> OnCrosshairActualPositionChanged;

        private static void CrosshairActualPositionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            
            var thisControl = d as CaptureOverlayWindow;
            System.Windows.Point newPosition = (Point)e.NewValue;

            thisControl.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.ContextIdle, new Action(() =>
            {
                var pointToImageControl = thisControl.backgroundImg.PointFromScreen(new Point(newPosition.X - thisControl.MagnifierCropSliceSize.Width / 2, newPosition.Y - thisControl.MagnifierCropSliceSize.Height / 2));
                thisControl.UpdateMagnifierImage(pointToImageControl);

                thisControl.OnCrosshairActualPositionChanged?.Invoke(thisControl, newPosition);
            }));
        }

        public void ShowVerticalScrollbarControl(Rect rect)
        {
            vertScroll.Visibility = Visibility.Visible;
            // disabled by default (the animation is active)
            vertScroll.IsEnabled = false;
            int systemScrollbarWidth = System.Windows.Forms.SystemInformation.VerticalScrollBarWidth;
            Canvas.SetTop(vertScroll, rect.Top + rect.Height / 2 - vertScroll.Height / 2);
            Canvas.SetLeft(vertScroll, rect.Left + rect.Width - vertScroll.Width - systemScrollbarWidth);
            _isVertScrollBtnVisible = true;
        }

        public void ShowHorizontalScrollbarControl(Rect rect)
        {
            horizScroll.Visibility = Visibility.Visible;
            // disabled by default (the animation is active)
            horizScroll.IsEnabled = false;
            int systemScrollbarHeight = System.Windows.Forms.SystemInformation.HorizontalScrollBarHeight;
            Canvas.SetTop(horizScroll, rect.Top + rect.Height - horizScroll.Height - systemScrollbarHeight);
            Canvas.SetLeft(horizScroll, rect.Left + rect.Width / 2 - horizScroll.Width / 2);
            _isHorizScrollBtnVisible = true;
        }

        public void HideScrollbarControlsIfVisible()
        {
            vertScroll.Visibility = Visibility.Collapsed;
            horizScroll.Visibility = Visibility.Collapsed;
            _isHorizScrollBtnVisible = false;
            _isVertScrollBtnVisible = false;
        }

        public void SetSelectorRectangle(Rect rect, bool includeSelectorBorderSize)
        {
            Debug.WriteLine("Y: " + this.Top);
            SelectBoxNewCanvasLeft = rect.Left;
            SelectBoxNewCanvasTop = rect.Top;
            SelectBoxNewHeight = rect.Height;
            SelectBoxNewWidth = rect.Width;
        }

        public void SetMagnifierText(string text)
        {
            magnifier.Text = text;
        }

        public SelectorTypes SelectorType
        {
            get { return (SelectorTypes)GetValue(SelectorTypeProperty); }
            set { SetValue(SelectorTypeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SelectorType.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectorTypeProperty =
            DependencyProperty.Register("SelectorType", typeof(SelectorTypes), typeof(CaptureOverlayWindow), new PropertyMetadata(SelectorTypes.AutoSelector, OnSelectorTypeChanged));

        private static void OnSelectorTypeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var window = d as CaptureOverlayWindow;
            
        }

        public bool Resizing
        {
            get { return (bool)GetValue(ResizingProperty); }
            set { SetValue(ResizingProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Resizing.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ResizingProperty =
            DependencyProperty.Register("Resizing", typeof(bool), typeof(CaptureOverlayWindow), new PropertyMetadata(false, ResizingChanged));

        private static void ResizingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            
        }



        public Visibility ScreenwideCrossVisibility
        {
            get { return (Visibility)GetValue(ScreenwideCrossVisibilityProperty); }
            set { SetValue(ScreenwideCrossVisibilityProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ScreenwideCrossVisibility.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ScreenwideCrossVisibilityProperty =
            DependencyProperty.Register("ScreenwideCrossVisibility", typeof(Visibility), typeof(CaptureOverlayWindow), new PropertyMetadata(Visibility.Visible));




        public bool ManualSelectionMode
        {
            get { return (bool )GetValue(ManualSelectionModeProperty); }
            set { SetValue(ManualSelectionModeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ManualSelectionMode.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ManualSelectionModeProperty =
            DependencyProperty.Register("ManualSelectionMode", typeof(bool ), typeof(CaptureOverlayWindow), new PropertyMetadata(false));



        public VerticalElementPlacement MagnifierVerticalPlacement
        {
            set
            {
                if (value == VerticalElementPlacement.Top)
                {
                    magnifier.PlaceTextOnTop = true;
                }
                else
                {
                    magnifier.PlaceTextOnTop = false;
                }
            }
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                RestoreCursor();
                
                if (BackgroundImage != null)
                {
                    BackgroundImage.Dispose();
                }

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        ~CaptureOverlayWindow()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(false);
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
