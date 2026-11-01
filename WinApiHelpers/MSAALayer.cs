using Accessibility;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms; // for IAccessible.accLocation()

namespace WinApiHelpers
{
    public static class MSAALayer
    {
        // https://www.codeproject.com/Articles/38906/UI-Automation-Using-Microsoft-Active-Accessibility

        [DllImport("oleacc.dll")]
        public static extern uint GetRoleText(uint dwRole, [Out] StringBuilder lpszRole, uint cchRoleMax);

        [DllImport("oleacc.dll")]
        public static extern uint GetStateText(uint dwStateBit, [Out] StringBuilder lpszStateBit, uint cchStateBitMax);

        [DllImport("oleacc.dll")]
        public static extern uint WindowFromAccessibleObject(IAccessible pacc, ref IntPtr phwnd);

        [DllImport("oleacc.dll", PreserveSig = false)]
        [return: MarshalAs(UnmanagedType.Interface)]
        public static extern object AccessibleObjectFromWindow(int hwnd, int dwId, ref Guid riid);

        [DllImport("oleacc.dll")]
        internal static extern int AccessibleObjectFromWindow(
             IntPtr hwnd,
             uint id,
             ref Guid iid,
             [In, Out, MarshalAs(UnmanagedType.IUnknown)] ref object ppvObject);

        [DllImport("oleacc.dll")]
        public static extern int AccessibleChildren(IAccessible paccContainer, int iChildStart, int cChildren,
                                                     [Out()] [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 4)] object[] rgvarChildren, ref int pcObtained);



        public static IAccessible[] GetAccessibleChildren(IAccessible objAccessible)
        {
            int childCount = 0;

            try
            {
                childCount = objAccessible.accChildCount;
            }
            catch (Exception ex)
            {
                childCount = 0;
            }

            var accObjects = new IAccessible[childCount];
            int count = 0;

            if (childCount != 0)
            {
                AccessibleChildren(objAccessible, 0, childCount, accObjects, ref count);
            }

            return accObjects;
        }

        public static IAccessible GetAccessibleObjectFromHandle(IntPtr hwnd)
        {
            object accObject = new object();
            IAccessible objAccessible = default(IAccessible);
            var guidAccessible = new Guid("{618736E0-3C3D-11CF-810C-00AA00389B71}");
            if (hwnd != (IntPtr)0)
            {
                AccessibleObjectFromWindow(hwnd, 0, ref guidAccessible, ref accObject);
                objAccessible = (IAccessible)accObject;
            }
            
            return objAccessible;
        }

        internal static IntPtr GetHandle(IAccessible accessible)
        {
            IntPtr hwnd = IntPtr.Zero;
            WindowFromAccessibleObject(accessible, ref hwnd);
            return hwnd;
        }

        

        ///<summary>
        ///Member not found.
        ///</summary>
        [Description("Member not found.")]
        public const int DISP_E_MEMBERNOTFOUND = unchecked((int)0x80020003);


        private const int CHILDID_SELF = 0;

        public static Rectangle GetBounds(IAccessible accessible)
        {
            // IAccessible.accLocation() is a hidden methid, "for internal use only"... however, we have no other choice but use it

            // this is copied from System.Windows.Forms source code
            if (accessible != null)
            {
                int left = 0;
                int top = 0;
                int width = 0;
                int height = 0;
                try
                {
                    accessible.accLocation(out left, out top, out width, out height, CHILDID_SELF);
                    return new Rectangle(left, top, width, height);
                }
                catch (COMException e)
                {
                    if (e.ErrorCode != DISP_E_MEMBERNOTFOUND)
                    {
                        throw e;
                    }
                }
            }
            return Rectangle.Empty;
        }
        

    }
}
