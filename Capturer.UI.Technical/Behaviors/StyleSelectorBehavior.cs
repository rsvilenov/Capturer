using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interactivity;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace Capturer.UI.Technical.Behaviors
{
    public class StyleSelectorBehavior : Behavior<FrameworkElement>
    {

        public StyleSelectorBehavior()
        {
            StylesResourceKeys = new ObservableCollection<string>();
        }

        public ObservableCollection<string> StylesResourceKeys
        {
            get { return (ObservableCollection<string>)GetValue(StylesResourceKeysProperty); }
            set { SetValue(StylesResourceKeysProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Styles.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty StylesResourceKeysProperty =
            DependencyProperty.Register("StylesResourceKeys", typeof(ObservableCollection<string>), typeof(StyleSelectorBehavior), new PropertyMetadata(null));



        public int SelectedStyleIndex
        {
            get { return (int)GetValue(SelectedStyleIndexProperty); }
            set { SetValue(SelectedStyleIndexProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SelectedStyleIndex.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectedStyleIndexProperty =
            DependencyProperty.Register("SelectedStyleIndex", typeof(int), typeof(StyleSelectorBehavior), new PropertyMetadata(0, SelectedStyleIndexChanged));

        private static void SelectedStyleIndexChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var thisBehavior = d as StyleSelectorBehavior;
            int newIndex = (int)e.NewValue;
            if (thisBehavior.AssociatedObject == null || thisBehavior.StylesResourceKeys == null || !thisBehavior.AssociatedObject.IsLoaded)
            {
                return;
            }
            thisBehavior.SetStyleByIndex(newIndex);
        }

        private void SetStyleByIndex(int index)
        {
            if (StylesResourceKeys.Count <= index)
            {
                throw new ArgumentOutOfRangeException(string.Format("SelectedStyleIndex should be between 0 and {0}", StylesResourceKeys.Count - 1));
            }
            StopAllAnimations(AssociatedObject);
            AssociatedObject.SetResourceReference(FrameworkElement.StyleProperty, string.Empty);

            AssociatedObject.Dispatcher.BeginInvoke(DispatcherPriority.ContextIdle,
                new Action(() => AssociatedObject.SetResourceReference(FrameworkElement.StyleProperty, StylesResourceKeys[SelectedStyleIndex])));
            
        }

        /// <summary>
        /// https://social.msdn.microsoft.com/Forums/vstudio/en-US/97c8e06c-42d1-49b1-89aa-89f2839129ef/how-to-stop-all-animations-in-a-window?forum=wpf
        /// </summary>
        /// <param name="fe"></param>
        private void StopAllAnimations(FrameworkElement fe)
        {
            foreach (DictionaryEntry i in fe.Resources)
            {
                ((Storyboard)fe.Resources[i.Key]).Stop(fe);
            }

            if (fe.Parent is Panel)
            {
                var p = fe.Parent as Panel;
                StopAllAnimations(p);
            }
        }

        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.Loaded += AssociatedObject_Loaded;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            AssociatedObject.Loaded -= AssociatedObject_Loaded;
        }

        private void AssociatedObject_Loaded(object sender, RoutedEventArgs e)
        {
            foreach (string key in StylesResourceKeys)
            {
                var resource = AssociatedObject.TryFindResource(key);
                if (resource == null)
                {
                    throw new ResourceReferenceKeyNotFoundException("Style key could not be found.", key);
                }
                
                if (!(resource is Style))
                {
                    throw new ArgumentException(string.Format("{0} is not of type Style", key));
                }
            }

           SetStyleByIndex(SelectedStyleIndex);
        }

    }
}
