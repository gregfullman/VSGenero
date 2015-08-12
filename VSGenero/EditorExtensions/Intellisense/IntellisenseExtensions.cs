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


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Language.StandardClassification;
using Microsoft.VisualStudio.Text.Classification;
using System.Data.SqlClient;
using System.Data;
using VSGenero.Analysis;
using VSGenero.Analysis.Parsing.AST;

namespace VSGenero.EditorExtensions.Intellisense
{
    public static class IntellisenseExtensions
    {
        // This list holds MRU completions for pre-selection of auto-complete list.
        private static Dictionary<string, int> _lastCommittedCompletions;
        public static Dictionary<string, int> LastCommittedCompletions
        {
            get
            {
                if (_lastCommittedCompletions == null)
                    _lastCommittedCompletions = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
                return _lastCommittedCompletions;
            }
        }
    }
}
