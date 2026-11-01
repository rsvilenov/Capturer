using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace WinApiHelpers
{
    // I think that win32exception will invoke formatmessage, but in any case, I will enclose the hresult in the message text
    public class WinApiException : Win32Exception
    {
        public WinApiException(string message) : base(String.Format("{0}. ErrorCode: {1}, HRESULT: {2}", message, Marshal.GetLastWin32Error(), Marshal.GetHRForLastWin32Error()))
        { }
    }
}
