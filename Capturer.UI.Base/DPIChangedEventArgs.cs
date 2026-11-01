using System;

namespace Capturer.UI.Base
{
    public class DPIChangedEventArgs : EventArgs
    {
        public DPIChangedEventArgs(double newDPI)
        {
            NewDPI = newDPI;
            ScaleFactor = newDPI / 96.0;
        }

        public double NewDPI { get; private set; }

        public double ScaleFactor { get; private set; }
    }
}
