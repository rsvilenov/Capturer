using Capturer.Engine.AutoScroll;
using Capturer.UI.Selector;
using System;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Threading;
using Capturer.Engine.ScreenCapture;
using WinApiHelpers;
using Capturer.Engine.ScreenAnalyzer.Shared.Interfaces;
using Capturer.Engine.ScreenAnalyzer.Shared.DTO;
using Capturer.Engine.ScreenAnalyzer.Shared.Enums;
using Capturer.Engine.ScreenCapture.Shared.Enums;
using Capturer.UI.Input.Interfaces;
using Capturer.UI.Input;
using Capturer.UI.Input.Enums;
using Capturer.Imaging;

namespace TestApp
{
    public class SelectorWrapper : IDisposable
    {
        private IScreenAnalyzer _screenAnalyzer;
        CaptureOverlayWindow _captureOverlay;
        IMouseInput _mouseInput;
        ICursorController _cursorController;

        public SelectorWrapper(IScreenAnalyzer analyzer)
        {
            DPIHelper.EnsurePerMonitorDPIAwareness();
            var allMonitors = DisplayMonitors.AllMonitors;

            _screenAnalyzer = analyzer;
        }

        public void ShowSelector()
        {
            var captureHelper = ScreenCaptureHelperFactory.GetScreenCaptureHelper(ScreenCaptureMethods.GDI);
            System.Drawing.Bitmap image = null;
            //for (int i = 0; i < 24* 10; i++)
            //{
                image = captureHelper.GetScreenshot();
            //}
            //image.Save("test.bmp") ;
            //Process.Start("test.bmp");

            _mouseInput = MouseInputFactory.GetMouseInput(MouseInputType.WPFCaptureMouseInput);
            _cursorController = CursorControllerFactory.GetCursorController(CursorControlMethods.System);
            //return;
            // wpf mouse move has problems
            _captureOverlay = 
                new CaptureOverlayWindow(_mouseInput,
                _cursorController,
                image, false);

               
            
            //_captureOverlay.SelectorPosition = new Point(60, 60);
            //_captureOverlay.SelectorSize = new Size(100, 50);
            try
            {
                _captureOverlay.OnCrosshairActualPositionChanged += Wnd_OnCrosshairActualPositionChanged;
                _captureOverlay.VertScrollBtnClicked += _captureOverlay_VertScrollBtnClicked;
                _captureOverlay.HorizScrollBtnClicked += _captureOverlay_HorizScrollBtnClicked;
                _captureOverlay.SelectionClicked += _captureOverlay_SelectionClicked;
                _captureOverlay.DragSelectionStarted += _captureOverlay_DragSelectionStarted;
                _captureOverlay.DragSelectionStopped += _captureOverlay_DragSelectionStopped;
                _captureOverlay.ShowDialog();
            }
            finally
            {
                _captureOverlay.OnCrosshairActualPositionChanged -= Wnd_OnCrosshairActualPositionChanged;
                _captureOverlay.VertScrollBtnClicked -= _captureOverlay_VertScrollBtnClicked;
                _captureOverlay.HorizScrollBtnClicked -= _captureOverlay_HorizScrollBtnClicked;
                _captureOverlay.SelectionClicked -= _captureOverlay_SelectionClicked;
                _captureOverlay.DragSelectionStarted -= _captureOverlay_DragSelectionStarted;
                _captureOverlay.DragSelectionStopped -= _captureOverlay_DragSelectionStopped;

            }
        }

        private void _captureOverlay_DragSelectionStopped(object sender, EventArgs e)
        {
            _captureOverlay.SwitchToManualSelectionMode();
        }

        private void _captureOverlay_DragSelectionStarted(object sender, EventArgs e)
        {
            _captureOverlay.SwitchToDragSelectionMode();
        }

        private void _captureOverlay_SelectionClicked(object sender, EventArgs e)
        {
            _captureOverlay.SwitchToManualSelectionMode();
        }

        private void _captureOverlay_HorizScrollBtnClicked(object sender, EventArgs e)
        {
            var vertScrollbarWithPattern = _currentWindowInfo.ScrollbarInfos.FirstOrDefault(sp => sp.Orientation == ScrollbarOrientation.Horizontal && sp.AccessType == ScrollbarAccessType.ScrollPattern);
            if (vertScrollbarWithPattern != null)
            {
                _captureOverlay.SwitchToManualSelectionMode(); // this should be a separate mode
                for (int i = 0; i <= 100; i += 10)
                {
                    bool success = UIAutomationHelpers.Objects.TryScrollToPercent(vertScrollbarWithPattern.AccessHandleObject as UIAutomationHelpers.Objects.UIAutomationScrollInfo, i, false);
                    System.Threading.Thread.Sleep(500);
                    if (!success)
                    {
                        if (i > 10)
                            i -= 10;
                    }
                }
            }
        }

        private void _captureOverlay_VertScrollBtnClicked(object sender, EventArgs e)
        {
            var vertScrollbar = _currentWindowInfo.ScrollbarInfos.FirstOrDefault(sp => sp.Orientation == ScrollbarOrientation.Vertical);
            if (vertScrollbar != null)
            {
                
                var scrollHelper = ScrollHelperFactory.GetScrollHelper(vertScrollbar);
                scrollHelper.ScrollToBeginning();
                var scrollAreaInfo = scrollHelper.GetScrollAreaInfo();
                PanoramicBlender blender = new PanoramicBlender();
                var captureHelper = Capturer.Engine.ScreenCapture.ScreenCaptureHelperFactory.GetScreenCaptureHelper(ScreenCaptureMethods.GDI);
                if (scrollAreaInfo != null)
                {
                  //var capture = captureHelper.GetScreenshot(_currentWindowInfo.Rectangle);
                  //capture.Save("cap.png");
                  //System.Diagnostics.Process.Start("cap.png");
                  //  blender.AddImage(capture);
                    for (double ci = 0; ci < scrollAreaInfo.MaxScrollValue; ci += scrollAreaInfo.ScrollStepSize)
                    {
                        scrollHelper.ScrollByStep(scrollAreaInfo.ScrollStepSize);
                        //capture = captureHelper.GetScreenshot(_currentWindowInfo.Rectangle);
                        //blender.AddImage(capture);
                        System.Threading.Thread.Sleep(500);
                    }
                    //var blendedImage = blender.GetBlendedImage();
                    //blendedImage.Save("blended.png");
                    //System.Diagnostics.Process.Start("blended.png");
                }
                else
                {
                    scrollHelper.ScrollByStep(10);
                }
            }
        }

        private WindowInfo _currentWindowInfo;
        private void Wnd_OnCrosshairActualPositionChanged(object sender, Point p)
        {
            var wnd = sender as CaptureOverlayWindow;
            if (wnd.ManualSelectionMode || wnd.MouseDragSelectionMode)
            {
                return;
            }
            // convert the window coordinates to screen coordinates (window coordinates are never negative, while screen coordinates can be)
            //p = wnd.PointToScreen(p);
            System.Drawing.Point dp = new System.Drawing.Point((int)p.X, (int)p.Y);
            var wInfo = _screenAnalyzer.GetWindowInfoFromPoint(dp);
            //, 
                                                             //windowCollection => windowCollection.Where(wi => wi.OverlapState != RectangleOverlapState.Full &&
                                                             //wi.WindowType != WindowTypes.TitleBar));
            if (wInfo != null)
            {
                if (wInfo != _currentWindowInfo)
                {
                    if (_currentWindowInfo != null)
                    {
                        _currentWindowInfo.PropertyChanged -= _currentWindowInfo_PropertyChanged;
                    }

                    _currentWindowInfo = wInfo;
                    _currentWindowInfo.PropertyChanged += _currentWindowInfo_PropertyChanged;
                    SetInfoToCaptureOverlay(wInfo);

                } 
            }
            else
            {
                // maybe this should be part of the screen analyzer
                var screen = Screen.FromPoint(dp);
                wInfo = new WindowInfo
                {
                    Rectangle = screen.Bounds
                };
                _currentWindowInfo = wInfo;
                SetInfoToCaptureOverlay(wInfo);
            }
        }

        private void _currentWindowInfo_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(_currentWindowInfo.ScrollbarInfos))
            {
                // we are passing (sender as WindowInfo) instead of (_currentWindowInfo), because
                // by the time the method is invoked, _currentWindowInfo may be changed
                if (_captureOverlay != null)
                {
                    _captureOverlay.Dispatcher.BeginInvoke(DispatcherPriority.ContextIdle, new Action(() =>
                        SetInfoToCaptureOverlay(sender as WindowInfo)));
                }
            }
        }

        private void SetInfoToCaptureOverlay(WindowInfo wInfo)
        {
            if (_captureOverlay.ManualSelectionMode)
            {
                return;
            }
            var dRect = wInfo.Rectangle;
            // transform the screen points to wpf points relative to _captureOverlay
            // PointFromScreen takes into account the DPI settings
            var screenPoint = _captureOverlay.PointFromScreen(new Point(dRect.X, dRect.Y));
            // transform the size as well
            var oppositeDiagonal = _captureOverlay.PointFromScreen(new Point(dRect.X + dRect.Width, dRect.Y + dRect.Height));

            Debug.WriteLine("--> "+ dRect.X);
            Debug.WriteLine("====> " + screenPoint.X);
            // construct the rectangle from transformed points
            var rect = new Rect(screenPoint.X, screenPoint.Y, oppositeDiagonal.X - screenPoint.X, oppositeDiagonal.Y - screenPoint.Y);
            //_captureOverlay.SetMagnifierText(wInfo.WindowClassName);
            
            // report the points as screen coordinates, not as transformed wpf points
            string magnifierText = string.Format("{0} x {1}", dRect.Width, dRect.Height);
            _captureOverlay.SetMagnifierText(magnifierText);
            
            _captureOverlay.HideScrollbarControlsIfVisible();

            _captureOverlay.SetSelectorRectangle(rect, true);
            if (wInfo.ScrollbarInfos != null && wInfo.ScrollbarInfos.Any())
            {
                if (wInfo.ScrollbarInfos.Any(info => info.Orientation == ScrollbarOrientation.Horizontal) &&
                    wInfo.ScrollbarInfos.Any(info => info.Orientation == ScrollbarOrientation.Vertical))
                {
                    _captureOverlay.SetMagnifierText("bs_");// + wInfo.OverlapState.ToString(); ;
                    _captureOverlay.ShowHorizontalScrollbarControl(rect);
                    _captureOverlay.ShowVerticalScrollbarControl(rect);
                }
                else if (wInfo.ScrollbarInfos.Any(info => info.Orientation == ScrollbarOrientation.Vertical))
                {
                    _captureOverlay.SetMagnifierText("vs_");// + wInfo.WindowClassName;
                    _captureOverlay.ShowVerticalScrollbarControl(rect);
                }
                else if (wInfo.ScrollbarInfos.Any(info => info.Orientation == ScrollbarOrientation.Horizontal))
                {
                    _captureOverlay.ShowHorizontalScrollbarControl(rect);
                    _captureOverlay.SetMagnifierText("hs_");// + wInfo.WindowClassName;
                }
            }
        }

        public void Dispose()
        {
            if (_captureOverlay != null)
            {
                _captureOverlay.Dispose();
                _captureOverlay = null;
            }
            if (_mouseInput != null)
            {
                _mouseInput.Dispose();
                _mouseInput = null;
            }
        }
    }
}
