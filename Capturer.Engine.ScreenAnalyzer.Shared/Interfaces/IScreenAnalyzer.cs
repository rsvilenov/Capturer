using System;
using System.Collections.Generic;
using System.Drawing;
using Capturer.Engine.ScreenAnalyzer.Shared.DTO;

namespace Capturer.Engine.ScreenAnalyzer.Shared.Interfaces
{
    public interface IScreenAnalyzer
    {
        WindowInfo GetWindowInfoFromPoint(Point p, Func<IEnumerable<WindowInfo>, IEnumerable<WindowInfo>> filterFunction = null);

        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        double AnalysisTime { get; set; }
    }
}
