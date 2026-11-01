using System.Windows;
using Capturer.Engine.ScreenAnalyzer;

namespace TestApp
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        PassiveScreenAnalyzer _analyzer;
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            _analyzer = new PassiveScreenAnalyzer();
            _analyzer.Analyze();
            using (SelectorWrapper wrapper = new SelectorWrapper(_analyzer))
            {
                wrapper.ShowSelector();
            }
        }

        
    }
}
