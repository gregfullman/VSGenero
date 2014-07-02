using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio;
using Microsoft.VisualStudioTools;
using Microsoft.VisualStudioTools.Project;

namespace VSGenero.Project
{
    internal class Genero4GLFileNode : CommonFileNode
    {
        internal Genero4GLFileNode(CommonProjectNode root, ProjectElement e)
            : base(root, e)
        {
        }
    }
}
