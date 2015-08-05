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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSGenero.Analysis;

namespace VSGenero.EditorExtensions.Intellisense
{
    class SnapshotCookie : IAnalysisCookie
    {
        private readonly ITextSnapshot _snapshot;

        public SnapshotCookie(ITextSnapshot snapshot)
        {
            _snapshot = snapshot;
        }

        public ITextSnapshot Snapshot
        {
            get
            {
                return _snapshot;
            }
        }

        #region IAnalysisCookie Members

        public string GetLine(int lineNo)
        {
            if(lineNo > 0)
                return _snapshot.GetLineFromLineNumber(lineNo - 1).GetText();
            return null;
        }

        #endregion
    }
}
