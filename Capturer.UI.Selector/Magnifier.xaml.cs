using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Capturer.UI.Base.Helpers;

namespace Capturer.UI.Selector
{
    /// <summary>
    /// Interaction logic for Magnifier.xaml
    /// </summary>
    public partial class Magnifier : UserControl
    {
        public Magnifier()
        {
            InitializeComponent();
            mainGrid.DataContext = this;
        }



        public double LabelBackgroundOpacity
        {
            get { return (double)GetValue(LabelBackgroundOpacityProperty); }
            set { SetValue(LabelBackgroundOpacityProperty, value); }
        }

        // Using a DependencyProperty as the backing store for LabelBackgroundOpacity.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty LabelBackgroundOpacityProperty =
            DependencyProperty.Register("LabelBackgroundOpacity", typeof(double), typeof(Magnifier), new PropertyMetadata(0.4));



        public System.Windows.Media.Color LabelBackgroundColor
        {
            get { return (System.Windows.Media.Color)GetValue(LabelBackgroundColorProperty); }
            set { SetValue(LabelBackgroundColorProperty, value); }
        }

        // Using a DependencyProperty as the backing store for LabelBackgroundColor.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty LabelBackgroundColorProperty =
            DependencyProperty.Register("LabelBackgroundColor", typeof(System.Windows.Media.Color), typeof(Magnifier), new PropertyMetadata(Colors.Gray));



        public System.Windows.Media.Color LabelForegroundColor
        {
            get { return (System.Windows.Media.Color)GetValue(LabelForegroundColorProperty); }
            set { SetValue(LabelForegroundColorProperty, value); }
        }

        // Using a DependencyProperty as the backing store for LabelForegroundColor.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty LabelForegroundColorProperty =
            DependencyProperty.Register("LabelForegroundColor", typeof(System.Windows.Media.Color), typeof(Magnifier), new PropertyMetadata(Colors.White));





        public Bitmap MagnifiedImage
        {
            get { return (Bitmap)GetValue(MagnifiedImageProperty); }
            set { SetValue(MagnifiedImageProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MagnifiedImage.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MagnifiedImageProperty =
            DependencyProperty.Register("MagnifiedImage", typeof(Bitmap), typeof(Magnifier), new PropertyMetadata(null, MagnifiedImageChanged));

        private static void MagnifiedImageChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var thisControl = d as Magnifier;
            Bitmap newBmp = e.NewValue as Bitmap;
            if (newBmp == null)
            {
                return;
            }

            var imgSource = ImageSourceHelper.BitmapToImageSource(newBmp);
            var brush = new ImageBrush(imgSource);
            thisControl.ellipse.Fill = brush;
        }

        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Text.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(Magnifier), new PropertyMetadata(string.Empty));



        public bool PlaceTextOnTop
        {
            get { return (bool)GetValue(PlaceTextOnTopProperty); }
            set { SetValue(PlaceTextOnTopProperty, value); }
        }

        // Using a DependencyProperty as the backing store for PlaceTextOnTop.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PlaceTextOnTopProperty =
            DependencyProperty.Register("PlaceTextOnTop", typeof(bool), typeof(Magnifier), new PropertyMetadata(false, PlaceTextOnTopChanged));

        private static void PlaceTextOnTopChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var thisControl = d as Magnifier;
            if ((bool)e.NewValue)
            {
                thisControl.topLabel.Visibility = Visibility.Visible;
                thisControl.bottomLabel.Visibility = Visibility.Collapsed;
            }
            else
            {
                thisControl.topLabel.Visibility = Visibility.Collapsed;
                thisControl.bottomLabel.Visibility = Visibility.Visible;
            }
        }
    }
}
