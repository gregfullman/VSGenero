using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Microsoft.VisualStudio.VSCommon.Utilities
{
    public class DevEnvWindowWrapper : IWin32Window
    {
        public IntPtr Handle
        {
            get
            {
                var shell = (IVsUIShell)VSCommonPackage.GetGlobalService(typeof(SVsUIShell));
                IntPtr hwnd;
                shell.GetDialogOwnerHwnd(out hwnd);
                return hwnd;
            }
        }
    }
}
