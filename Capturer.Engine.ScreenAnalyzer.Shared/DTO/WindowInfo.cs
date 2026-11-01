using System;
using System.Collections.ObjectModel;
using System.Drawing;
using Capturer.Engine.ScreenAnalyzer.Shared.Enums;
using Shared;

namespace Capturer.Engine.ScreenAnalyzer.Shared.DTO
{
    public class WindowInfo : ObservableObject
    {
        private IntPtr _HWND;
        /// <summary>
        /// The handle of the window
        /// </summary>
        public IntPtr HWND
        {
            get
            {
                return _HWND;
            }
            set
            {
                SetField(ref _HWND, value);
            }
        }

        private Rectangle _rectangle;
        /// <summary>
        /// The rectangle of the window
        /// </summary>
        public Rectangle Rectangle
        {
            get
            {
                return _rectangle;
            }
            set
            {
                SetField(ref _rectangle, value);
            }
        }

        private Rectangle _clientArea;
        /// <summary>
        /// The client area rectangle of the window
        /// </summary>
        public Rectangle ClientArea
        {
            get
            {
                return _clientArea;
            }
            set
            {
                SetField(ref _clientArea, value);
            }
        }

        private Rectangle _titleBarArea;
        /// <summary>
        /// The rectangle, representing the title bar area
        /// </summary>
        public Rectangle TitleBarArea
        {
            get
            {
                return _titleBarArea;
            }
            set
            {
                SetField(ref _titleBarArea, value);
            }
        }

        private string _capionText;
        /// <summary>
        /// Window's caption text
        /// </summary>
        public string CaptionText
        {
            get
            {
                return _capionText;
            }
            set
            {
                SetField(ref _capionText, value);
            }
        }

        private string _applicationName;
        public string ApplicationName
        {
            get
            {
                return _applicationName;
            }
            set
            {
                SetField(ref _applicationName, value);
            }
        }

        private string _applicationVersion;
        public string ApplicationVersion
        {
            get
            {
                return _applicationName;
            }
            set
            {
                SetField(ref _applicationName, value);
            }
        }

        private string _processPath;
        public string ProcessPath
        {
            get
            {
                return _processPath;
            }
            set
            {
                SetField(ref _processPath, value);
            }
        }

        private int _pid;
        public int Pid
        {
            get
            {
                return _pid;
            }
            set
            {
                SetField(ref _pid, value);
            }
        }

        private string _windowClassName;
        public string WindowClassName
        {
            get
            {
                return _windowClassName;
            }
            set
            {
                SetField(ref _windowClassName, value);
            }
        }

        private RectangleOverlapState _overlapState;
        public RectangleOverlapState OverlapState
        {
            get
            {
                return _overlapState;
            }
            set
            {
                SetField(ref _overlapState, value);
            }
        }

        private WindowTypes _windowType;
        public WindowTypes WindowType
        {
            get
            {
                return _windowType;
            }
            set
            {
                SetField(ref _windowType, value);
            }
        }

        private ObservableCollection<ScrollbarInfo> _scrollbarInfos;
        public ObservableCollection<ScrollbarInfo> ScrollbarInfos
        {
            get
            {
                return _scrollbarInfos;
            }
            set
            {
                if (_scrollbarInfos != null)
                {
                    _scrollbarInfos.CollectionChanged -= _scrollbarInfos_CollectionChanged;
                }

                SetField(ref _scrollbarInfos, value);

                if (_scrollbarInfos != null)
                {
                    _scrollbarInfos.CollectionChanged += _scrollbarInfos_CollectionChanged;
                }
            }
        }

        private void _scrollbarInfos_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            RaisePropertyChanged(nameof(ScrollbarInfos));
        }
    }
}
