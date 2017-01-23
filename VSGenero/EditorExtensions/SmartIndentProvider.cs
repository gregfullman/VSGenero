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

using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSGenero.External;

namespace VSGenero.EditorExtensions
{
    [Export(typeof(ISmartIndentProvider))]
    [ContentType(External.GeneroConstants.ContentType4GL)]
    [ContentType(External.GeneroConstants.ContentTypeINC)]
    public sealed class SmartIndentProvider : ISmartIndentProvider
    {
        private sealed class Indent : ISmartIndent
        {
            private readonly ITextView _textView;

            public Indent(ITextView view)
            {
                _textView = view;
                //AutoIndent.Initialize();
            }

            /// <summary>
            /// This is called when the enter key is pressed or when navigating to an empty line.
            /// </summary>
            /// <param name="line"></param>
            /// <returns></returns>
            public int? GetDesiredIndentation(ITextSnapshotLine line)
            {
                if (VSGeneroPackage.Instance.LangPrefs.IndentMode == vsIndentStyle.vsIndentStyleSmart)
                {
                    return AutoIndent.GetLineIndentation(line, _textView);
                }
                else
                {
                    return null;
                }
            }

            public void Dispose()
            {
            }
        }

        public ISmartIndent CreateSmartIndent(ITextView textView)
        {
            return new Indent(textView);
        }
    }
}
