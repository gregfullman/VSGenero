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
using Microsoft.VisualStudio.Text.BraceCompletion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSGenero.EditorExtensions.BraceCompletion
{
    internal class NormalCompletionContext : IBraceCompletionContext
    {
        // Fields
        protected readonly Genero4glLanguageInfo LanguageInfo;

        // Methods
        public NormalCompletionContext(Genero4glLanguageInfo languageInfo)
        {
            this.LanguageInfo = languageInfo;
        }

        public virtual bool AllowOverType(IBraceCompletionSession session)
        {
            SnapshotPoint? caretPosition = session.GetCaretPosition();
            if (!caretPosition.HasValue || !this.LanguageInfo.IsValidContext(caretPosition.Value))
            {
                return false;
            }
            this.LanguageInfo.DismissAllAndCommitActiveOne(session.TextView, session.ClosingBrace);
            return true;
        }

        public virtual void Finish(IBraceCompletionSession session)
        {
        }

        public virtual void OnReturn(IBraceCompletionSession session)
        {
        }

        public virtual void Start(IBraceCompletionSession session)
        {
        }
    }


}
