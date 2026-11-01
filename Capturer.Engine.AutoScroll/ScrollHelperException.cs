using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Capturer.Engine.AutoScroll
{
    public class ScrollHelperException : Exception
    {
        public string CallerFilePathName { get; private set; }

        public ScrollHelperException(string message, Exception ex, [CallerFilePath]string classFilePath = null) : 
            base(message, ex)
        {
            CallerFilePathName = classFilePath;
        }

        public ScrollHelperException(Exception ex) :
            this(string.Empty, ex)
        { }
    }
}
