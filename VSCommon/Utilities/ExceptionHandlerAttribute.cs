using Microsoft.VisualStudio.Shell.Interop;
using PostSharp.Aspects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Interop;

namespace Microsoft.VisualStudio.VSCommon.Utilities
{
    [Serializable]
    public class ExceptionHandlerAttribute : OnExceptionAspect
    {
        public override void OnException(MethodExecutionArgs args)
        {
            if (VSCommonPackage.ActivityLog != null)
            {
                // Write the exception to the activity log
                VSCommonPackage.ActivityLog.LogEntry((uint)__ACTIVITYLOG_ENTRYTYPE.ALE_ERROR, args.Method.DeclaringType.FullName, args.Exception.ToString());
                
                // Display the exception
                ExceptionForm form = new ExceptionForm(args.Exception);
                form.ShowDialog(new DevEnvWindowWrapper());
                
            }
            base.OnException(args);
        }
    }
}
