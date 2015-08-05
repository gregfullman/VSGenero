/* ****************************************************************************
 * Copyright (c) 2015 Greg Fullman 
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution.
 * By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 * ***************************************************************************/

using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.TextManager.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSGenero.Refactoring
{
    /// <summary>
    /// Provides a common interface for our elements of the preview list.  Currently we break this into a top-level
    /// tree node for each file (FilePreviewItem) and a leaf node for each individual rename location (LocationPreviewItem).
    /// </summary>
    interface IPreviewItem
    {
        ushort Glyph
        {
            get;
        }

        IntPtr ImageList
        {
            get;
        }

        bool IsExpandable
        {
            get;
        }

        PreviewList Children
        {
            get;
        }

        string GetText(VSTREETEXTOPTIONS options);

        _VSTREESTATECHANGEREFRESH ToggleState();

        __PREVIEWCHANGESITEMCHECKSTATE CheckState
        {
            get;
        }

        void DisplayPreview(IVsTextView view);

        void Close(VSTREECLOSEACTIONS vSTREECLOSEACTIONS);

        Span? Selection
        {
            get;
        }
    }
}
