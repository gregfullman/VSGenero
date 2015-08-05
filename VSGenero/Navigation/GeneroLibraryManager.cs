/* ****************************************************************************
 * Copyright (c) 2015 Greg Fullman
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the Apache License, Version 2.0, please send an email to 
 * vspython@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 * ***************************************************************************/

using System;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudioTools;
using Microsoft.VisualStudioTools.Navigation;
using Microsoft.VisualStudioTools.Project;

namespace VSGenero.Navigation
{
    /// <summary>
    /// This interface defines the service that finds Python files inside a hierarchy
    /// and builds the informations to expose to the class view or object browser.
    /// </summary>
    [Guid(VSGeneroConstants.LibraryManagerServiceGuid)]
    public interface IGeneroLibraryManager : ILibraryManager {
    }

    /// <summary>
    /// Implementation of the service that builds the information to expose to the symbols
    /// navigation tools (class view or object browser) from the Python files inside a
    /// hierarchy.
    /// </summary>
    [Guid(VSGeneroConstants.LibraryManagerGuid)]
    public class GeneroLibraryManager : LibraryManager, IGeneroLibraryManager
    {
        private readonly VSGeneroPackage/*!*/ _package;

        public GeneroLibraryManager(VSGeneroPackage/*!*/ package)
            : base(package)
        {
            _package = package;
        }

        protected override LibraryNode CreateLibraryNode(LibraryNode parent, IScopeNode subItem, string namePrefix, IVsHierarchy hierarchy, uint itemid)
        {
            throw new NotImplementedException();
        }
    }
}
