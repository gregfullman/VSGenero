using Microsoft.VisualStudio.Shell.Interop;
using PostSharp.Aspects;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
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
        private static StackTrace _lastDisplayedException_StackTrace;

        public override void OnException(MethodExecutionArgs args)
        {
            if (VSCommonPackage.ActivityLog != null && args != null)
            {
                if (args.Method != null && args.Method.DeclaringType != null)
                {
                    // Write the exception to the activity log
                    VSCommonPackage.ActivityLog.LogEntry((uint) __ACTIVITYLOG_ENTRYTYPE.ALE_ERROR,
                        args.Method.DeclaringType.FullName, args.Exception.ToString());
                }

                if(_lastDisplayedException_StackTrace == null)
                {
                    _lastDisplayedException_StackTrace = new StackTrace(args.Exception);
                }
                else
                {
                    // determine if the currently thrown exception's target site is within the stored stack trace
                    var currentMethodBase = args.Exception.TargetSite;
                    for(int i = 0; i < _lastDisplayedException_StackTrace.FrameCount; i++)
                    {
                        if(currentMethodBase == _lastDisplayedException_StackTrace.GetFrame(i).GetMethod())
                        {
                            return;
                        }
                    }
                    // at this point, we know we're in a new unique stack trace, so store it
                    _lastDisplayedException_StackTrace = new StackTrace(args.Exception);
                }

                // Display the exception
                ExceptionForm form = new ExceptionForm(args.Exception);
                form.ShowDialog(new DevEnvWindowWrapper());
                
            }
            base.OnException(args);
        }
    }
}
