using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace VSGenero.Peek
{
    internal static class NativeMethods
    {
        [DllImport("GDI32.DLL", EntryPoint = "CreateRectRgn")]
        internal static extern IntPtr CreateRectRgn(Int32 x1, Int32 y1, Int32 x2, Int32 y2);

        [DllImport("User32.dll", SetLastError = true)]
        internal static extern Int32 SetWindowRgn(IntPtr hWnd, IntPtr hRgn, Boolean bRedraw);
    }
}
