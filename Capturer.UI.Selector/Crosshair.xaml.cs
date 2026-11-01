using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace Capturer.UI.Selector
{
    /// <summary>
    /// Interaction logic for Crosshair.xaml
    /// </summary>
    public partial class Crosshair : UserControl
    {
        public Crosshair()
        {
            InitializeComponent();
            
            LayoutUpdated += Crosshair_LayoutUpdated;
        }

        private void Crosshair_LayoutUpdated(object sender, EventArgs e)
        {
            if (CCH_CenterRectangle == null || DesignerProperties.GetIsInDesignMode(this))
                return;
            var location = CCH_CenterRectangle.PointToScreen(new Point(0, 0));
            if (location != _prevLocation)
            {
                ActualScreenLocation = location;
            }
            _prevLocation = location;
        }

        private Point _prevLocation;


        public Point ActualScreenLocation
        {
            get { return (Point)GetValue(ScreenLocationProperty); }
            set { SetValue(ScreenLocationProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ScreenLocation.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ScreenLocationProperty =
            DependencyProperty.Register("ActualScreenLocation", typeof(Point), typeof(Crosshair), new PropertyMetadata(new Point()));

        
    }
}
