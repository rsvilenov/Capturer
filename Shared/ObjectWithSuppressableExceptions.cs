using System;
using System.Diagnostics;

namespace Shared
{
    public class ObjectWithSuppressableExceptions
    {
        protected void HandleException(string message, Exception ex)
        {
            if (!SuppressExceptions)
            {
                throw new Exception(message, ex);
            }
            else
            {
                Debug.WriteLine(string.Format("{0}\n{1}", message, ex.ToString()));
            }
        }

        protected void HandleException(Exception ex)
        {
            if (!SuppressExceptions)
            {
                throw ex;
            }
            else
            {
                Debug.WriteLine( ex.ToString());
            }
        }

        public bool SuppressExceptions { get; set; }
    }
}
