using Microsoft.VisualStudio.VSCommon.Utilities;

[assembly: MethodTracing(AttributeTargetTypes = "Microsoft.VisualStudio.VSCommon.*")]
[assembly: MethodTracing(AttributeTargetTypes = "Microsoft.VisualStudio.VSCommon.Utilities.*", AttributeExclude = true, AttributePriority = 1)]