using NLog;
using NLog.Layouts;
using PostSharp.Aspects;
using PostSharp.Serialization;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace Microsoft.VisualStudio.VSCommon.Utilities
{
    [Serializable]
    public class MethodTracingAttribute : OnMethodBoundaryAspect
    {
        private string _methodSig;
        private string _fullName;

        [NonSerialized]
        private Logger _logger;
        private static StackTrace _lastDisplayedException_StackTrace;

        public override void CompileTimeInitialize(MethodBase method, AspectInfo aspectInfo)
        {
            _fullName = string.Format("{0}.{1}", method.ReflectedType.FullName,
                                       method.Name);
            _methodSig = string.Format("{0}({1})", 
                                       _fullName, 
                                       string.Join(",", method.GetParameters().Select(o => string.Format("{0} {1}", o.ParameterType, o.Name)).ToArray()));
            base.CompileTimeInitialize(method, aspectInfo);
        }

        private Logger CurrentLogger
        {
            get
            {
                if (_logger == null)
                    _logger = LogManager.GetLogger(_fullName);
                return _logger;
            }
        }

        public override void OnEntry(MethodExecutionArgs args)
        {
            CurrentLogger.Trace("Entering: {0}", _methodSig);
            //var traceEvent = new LogEventInfo(LogLevel.Trace, string.Empty, CultureInfo.CurrentCulture, "Entering: {0}", new object[] { _methodSig });
            //CurrentLogger.Log(traceEvent);
            base.OnEntry(args);
        }

        public override void OnSuccess(MethodExecutionArgs args)
        {
            CurrentLogger.Trace("Leaving: {0}", _methodSig);
            //var traceEvent = new LogEventInfo(LogLevel.Trace, string.Empty, CultureInfo.CurrentCulture, "Leaving: {0}", new object[] { _methodSig });
            ////traceEvent.Properties["MethodArguments"] = string.Format("{0}", args.ReturnValue);
            //CurrentLogger.Log(traceEvent);
            base.OnSuccess(args);
        }

        public override void OnException(MethodExecutionArgs args)
        {
            if (VSCommonPackage.ActivityLog != null && args != null)
            { 
                if (_lastDisplayedException_StackTrace == null)
                {
                    _lastDisplayedException_StackTrace = new StackTrace(args.Exception);
                }
                else
                {
                    // determine if the currently thrown exception's target site is within the stored stack trace
                    var currentMethodBase = args.Exception.TargetSite;
                    for (int i = 0; i < _lastDisplayedException_StackTrace.FrameCount; i++)
                    {
                        if (currentMethodBase == _lastDisplayedException_StackTrace.GetFrame(i).GetMethod())
                        {
                            return;
                        }
                    }
                    // at this point, we know we're in a new unique stack trace, so store it
                    _lastDisplayedException_StackTrace = new StackTrace(args.Exception);
                }

                var traceEvent = new LogEventInfo(LogLevel.Fatal, _fullName, CultureInfo.CurrentCulture, "Exception occurred.", new object[] { _methodSig }, args.Exception);
                traceEvent.Properties["MethodArguments"] = string.Format("Args: ({0})", string.Join(",", args.Arguments.Select(x => x == null ? "(null)" : x.ToString())));
                CurrentLogger.Log(traceEvent);

                // Display the exception
                ExceptionForm form = new ExceptionForm(args.Exception);
                form.ShowDialog(new DevEnvWindowWrapper());

            }
            base.OnException(args);
        }
    }
}
