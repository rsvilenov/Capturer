using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interactivity;
using System.Windows.Media.Animation;

namespace Capturer.UI.Technical.Behaviors
{
    public class CanvasElementAnimateMoveResizeBehavior : Behavior<FrameworkElement>
    {
        private Dictionary<string, Storyboard> _stories = new Dictionary<string, Storyboard>
        {
            {Canvas.TopProperty.Name, new Storyboard() },
            {Canvas.LeftProperty.Name, new Storyboard() },
            {FrameworkElement.WidthProperty.Name, new Storyboard() },
            {FrameworkElement.HeightProperty.Name, new Storyboard() }
        };



        public bool Enabled
        {
            get { return (bool)GetValue(EnabledProperty); }
            set { SetValue(EnabledProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Enabled.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty EnabledProperty =
            DependencyProperty.Register("Enabled", typeof(bool), typeof(CanvasElementAnimateMoveResizeBehavior), new PropertyMetadata(true, EnabledChanged));

        private static void EnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!(bool)e.NewValue)
            {
                var thisBehavior = d as CanvasElementAnimateMoveResizeBehavior;
                foreach (var story in thisBehavior._stories)
                {
                    story.Value.Stop();
                    story.Value.Remove(thisBehavior.AssociatedObject);
                }
            }
        }

        public Duration AnimationDuration
        {
            get { return (Duration)GetValue(AnimationDurationProperty); }
            set { SetValue(AnimationDurationProperty, value); }
        }

        // Using a DependencyProperty as the backing store for AnimationDuration.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty AnimationDurationProperty =
            DependencyProperty.Register("AnimationDuration", typeof(Duration), typeof(CanvasElementAnimateMoveResizeBehavior), new PropertyMetadata(new Duration(new TimeSpan(0, 0, 0, 0, 200))));




        public double NewHight
        {
            get { return (double)GetValue(NewHightProperty); }
            set { SetValue(NewHightProperty, value); }
        }

        // Using a DependencyProperty as the backing store for NewHight.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty NewHightProperty =
            DependencyProperty.Register("NewHight", typeof(double), typeof(CanvasElementAnimateMoveResizeBehavior), new PropertyMetadata(0d, NewHeightChanged));

        private static void NewHeightChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var thisBehavior = d as CanvasElementAnimateMoveResizeBehavior;
            thisBehavior.AnimateProperty(new PropertyPath(FrameworkElement.HeightProperty), thisBehavior.AssociatedObject.Height, (double)e.NewValue);
            //thisBehavior.AssociatedObject.Height = (double)e.NewValue;
        }

        public double NewWidth
        {
            get { return (double)GetValue(NewWidthProperty); }
            set { SetValue(NewWidthProperty, value); }
        }

        // Using a DependencyProperty as the backing store for NewHight.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty NewWidthProperty =
            DependencyProperty.Register("NewWidth", typeof(double), typeof(CanvasElementAnimateMoveResizeBehavior), new PropertyMetadata(0d, NewWidthChanged));

        private static void NewWidthChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var thisBehavior = d as CanvasElementAnimateMoveResizeBehavior;

            thisBehavior.AnimateProperty(new PropertyPath(FrameworkElement.WidthProperty), thisBehavior.AssociatedObject.Width, (double)e.NewValue);

            //thisBehavior.AssociatedObject.Width = (double)e.NewValue;
        }

        public double NewCanvasTop
        {
            get { return (double)GetValue(NewCanvasTopProperty); }
            set { SetValue(NewCanvasTopProperty, value); }
        }

        // Using a DependencyProperty as the backing store for NewHight.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty NewCanvasTopProperty =
            DependencyProperty.Register("NewCanvasTop", typeof(double), typeof(CanvasElementAnimateMoveResizeBehavior), new PropertyMetadata(0d, NewCanvasTopChanged));

        private static void NewCanvasTopChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var thisBehavior = d as CanvasElementAnimateMoveResizeBehavior;
            thisBehavior.AnimateProperty(new PropertyPath(Canvas.TopProperty), Canvas.GetTop(thisBehavior.AssociatedObject), (double)e.NewValue);
            //Canvas.SetTop(thisBehavior.AssociatedObject, (double)e.NewValue);
        }

        public double NewCanvasLeft
        {
            get { return (double)GetValue(NewCanvasLeftProperty); }
            set { SetValue(NewCanvasLeftProperty, value); }
        }

        // Using a DependencyProperty as the backing store for NewHight.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty NewCanvasLeftProperty =
            DependencyProperty.Register("NewCanvasLeft", typeof(double), typeof(CanvasElementAnimateMoveResizeBehavior), new PropertyMetadata(0d, NewCanvasLeftChanged));

        private static void NewCanvasLeftChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var thisBehavior = d as CanvasElementAnimateMoveResizeBehavior;
            thisBehavior.AnimateProperty(new PropertyPath(Canvas.LeftProperty), Canvas.GetLeft(thisBehavior.AssociatedObject), (double)e.NewValue);
            //Canvas.SetLeft(thisBehavior.AssociatedObject, (double)e.NewValue);
        }
        
        private void AnimateProperty(PropertyPath propPath, double oldValue, double newValue)
        {
            if (!Enabled)
            {
                return;
            }

            if (double.IsNaN(oldValue))
                oldValue = 0d;

            DoubleAnimation animation = new DoubleAnimation(oldValue, newValue, AnimationDuration);
            animation.Completed += Animation_Completed;
            Storyboard.SetTarget(animation, AssociatedObject);
            Storyboard.SetTargetProperty(animation, propPath);

            var dp = propPath.PathParameters[0] as DependencyProperty;
            Storyboard story = _stories[dp.Name];
            story.Stop();
            story.Children.Clear();
            story.Children.Add(animation);
            story.Begin();
        }

        private void Animation_Completed(object sender, EventArgs e)
        {
            var property = Storyboard.GetTargetProperty(((sender as AnimationClock).Timeline as AnimationTimeline));
            var dp = property.PathParameters[0] as DependencyProperty;
            AssociatedObject.SetValue(dp, AssociatedObject.GetValue(dp));
            
        }
        
    }
}
