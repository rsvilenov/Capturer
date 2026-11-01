namespace Capturer.UI.Base.Extentions
{
    public static class DoubleExtentionMethods
    {
        public static double GetZeroIfNaN(this double originalDouble)
        {
            if (double.IsNaN(originalDouble))
                return 0;
            return originalDouble;
        }
    }
}
