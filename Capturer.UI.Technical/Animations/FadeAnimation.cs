using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Animation;

namespace Capturer.UI.Technical.Animations
{
    public class FadeAnimation
    {
        FrameworkElement _element;
        Storyboard _storyboard;
        TimeSpan _duration;
        ManualResetEvent _mrEvent;

        public FadeAnimation(FrameworkElement element, int durationMs)
        {
            _element = element;
            _storyboard = new Storyboard();
            _duration = TimeSpan.FromMilliseconds(durationMs); //
            _mrEvent = new ManualResetEvent(true);
        }

        public async Task Animate(bool fadeOut)
        {
            await Task.Run(() => 
            {
                _mrEvent.Reset();
                _element.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.ContextIdle, new Action(() =>

                {
                    _storyboard.Stop();
                    if (fadeOut)
                    {
                        DoubleAnimation fadeOutAnimation = new DoubleAnimation()
                        { From = 1.0, To = 0.0, Duration = new Duration(_duration) };
                        
                        fadeOutAnimation.Completed += (sender, args) => _mrEvent.Set();

                        Storyboard.SetTarget(fadeOutAnimation, _element);
                        Storyboard.SetTargetProperty(fadeOutAnimation, new PropertyPath("Opacity", 0));
                        _storyboard.Children.Add(fadeOutAnimation);
                        _storyboard.Begin(_element);
                        
                    }
                    else
                    {
                        DoubleAnimation fadeInAnimation = new DoubleAnimation()
                        { From = 0.0, To = 1.0, Duration = new Duration(_duration) };
                        fadeInAnimation.Completed += (sender, args) => _mrEvent.Set();

                        Storyboard.SetTarget(fadeInAnimation, _element);
                        Storyboard.SetTargetProperty(fadeInAnimation, new PropertyPath("Opacity", 1));
                        _storyboard.Children.Add(fadeInAnimation);
                        _storyboard.Begin(_element);
                    }
                }
                ));
                _mrEvent.WaitOne();
            });
        }
    }
}
