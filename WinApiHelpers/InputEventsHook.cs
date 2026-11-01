using System;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace WinApiHelpers
{
    public class InputEventsHook
    {

        public class Win32
        {
            [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
            public static extern IntPtr SetWindowsHookEx(int idHook, LowLevelHookProc lpfn, IntPtr hMod, uint dwThreadId);

            [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool UnhookWindowsHookEx(IntPtr hhk);

            [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
            public static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

            [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
            public static extern IntPtr GetModuleHandle(string lpModuleName);
            

            public delegate IntPtr LowLevelHookProc(int nCode, IntPtr wParam, IntPtr lParam);
        }
        
        private static IntPtr _keyboardHookID = IntPtr.Zero;
        private static IntPtr _mouseHookID = IntPtr.Zero;

        private static bool _isHookRunning = false;

        private static Win32.LowLevelHookProc _keyboardProc = KeyboardHookCallback;
        private static Win32.LowLevelHookProc _mouseProc = MouseHookCallback;


        public class MouseEventArgs : EventArgs
        {
            public Point Point { get; set; }
            public Win32Enums.MouseMessages MouseAction { get; set; }
            public bool CancelEvent { get; set; }

            public MouseEventArgs(Point p, Win32Enums.MouseMessages m)
            {
                Point = p;
                MouseAction = m;
            }
        }

        public class KeyDownEventArgs
        {
            public Win32Enums.KeyboardMessages KeyboardAction { get; set; }
            public Keys Key { get; set; }

            public KeyDownEventArgs(Keys key, Win32Enums.KeyboardMessages action)
            {
                Key = key;
                KeyboardAction = action;
            }
        }

        public static event Action<KeyDownEventArgs> KeyboardAction;
        public static event Action<MouseEventArgs> MouseAction;

        public static void Start()
        {
            if (_isHookRunning)
                return;

            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
            {
                // GetModuleHandle("user32")
                // Win32.GetModuleHandle(curModule.ModuleName)
                _keyboardHookID = Win32.SetWindowsHookEx((int)Win32Enums.HookType.WH_KEYBOARD_LL, _keyboardProc,
                    Win32.GetModuleHandle(curModule.ModuleName), 0);
                if (_keyboardHookID == IntPtr.Zero)
                    throw new WinApiException("Setting the kb hook failed");
                _mouseHookID = Win32.SetWindowsHookEx((int)Win32Enums.HookType.WH_MOUSE_LL, _mouseProc, 
                    Win32.GetModuleHandle(curModule.ModuleName), 0);
                if (_mouseHookID == IntPtr.Zero)
                    throw new WinApiException("Setting the mouse hook failed");
            }
            

        }

        public static void Stop()
        {
            if (KeyboardAction?.GetInvocationList().Length > 0 || MouseAction?.GetInvocationList().Length > 0)
                throw new InvalidOperationException("Cannot stop the hook while there are listeners on its events. Please unsubscribe the listeners first");
            if (!Win32.UnhookWindowsHookEx(_keyboardHookID))
                throw new WinApiException("UnhookWindowsHookEx for the keyboard hook failed");
            if (!Win32.UnhookWindowsHookEx(_mouseHookID))
                throw new WinApiException("UnhookWindowsHookEx for the mouse hook failed");
            _isHookRunning = false;
        }

        public static void DetatchAllListeners()
        {
            foreach (var deleg in KeyboardAction.GetInvocationList())
                KeyboardAction -= (Action<KeyDownEventArgs>)deleg;
        }


        private static IntPtr KeyboardHookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && (wParam == (IntPtr)Win32Enums.KeyboardMessages.WM_KEYDOWN || wParam == (IntPtr)Win32Enums.KeyboardMessages.WM_KEYUP))
            {
                int vkCode = Marshal.ReadInt32(lParam);
                KeyboardAction?.Invoke(new KeyDownEventArgs((Keys)vkCode, (Win32Enums.KeyboardMessages)wParam));
            }

            return Win32.CallNextHookEx(_keyboardHookID, nCode, wParam, lParam);
        }

        private static IntPtr MouseHookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {

            Win32Structures.MSLLHOOKSTRUCT hookStruct = (Win32Structures.MSLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(Win32Structures.MSLLHOOKSTRUCT));
            MouseEventArgs args = new MouseEventArgs(new Point(hookStruct.pt.X, hookStruct.pt.Y), (Win32Enums.MouseMessages)wParam);
            if (nCode >= 0)
            {
                MouseAction?.Invoke(args);
            }
            if (args.CancelEvent)
                return new IntPtr(1);
            return Win32.CallNextHookEx(_mouseHookID, nCode, wParam, lParam);
        }

    }
}
