using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Shell;

namespace VSGenero.Peek
{
    public class SmoothProgressBarWorkaround : SmoothProgressBar
    {
        public static object ProgressBarStyleKey
        {
            get
            {
                return VsResourceKeys.ProgressBarStyleKey;
            }
        }
    }
}
