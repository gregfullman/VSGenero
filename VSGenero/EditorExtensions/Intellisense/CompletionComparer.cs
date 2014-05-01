﻿/* ****************************************************************************
 *
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
using System.Collections.Generic;
using Microsoft.VisualStudio.Language.Intellisense;

namespace VSGenero.EditorExtensions.Intellisense
{
    /// <summary>
    /// Compares various types of completions.
    /// </summary>
    public class CompletionComparer : IEqualityComparer<Completion>, IComparer<Completion>, IComparer<string>
    {
        /// <summary>
        /// A CompletionComparer that sorts names beginning with underscores to
        /// the end of the list.
        /// </summary>
        public static readonly CompletionComparer UnderscoresLast = new CompletionComparer(true);
        /// <summary>
        /// A CompletionComparer that determines whether
        /// <see cref="MemberResult" /> structures are equal.
        /// </summary>
        public static readonly IEqualityComparer<Completion> MemberEquality = UnderscoresLast;
        /// <summary>
        /// A CompletionComparer that sorts names beginning with underscores to
        /// the start of the list.
        /// </summary>
        public static readonly CompletionComparer UnderscoresFirst = new CompletionComparer(false);

        bool _sortUnderscoresLast;

        /// <summary>
        /// Compares two strings.
        /// </summary>
        public int Compare(string xName, string yName)
        {
            if (yName == null)
            {
                return xName == null ? 0 : -1;
            }
            else if (xName == null)
            {
                return yName == null ? 0 : 1;
            }

            if (_sortUnderscoresLast)
            {
                bool xUnder = xName.StartsWith("__") && xName.EndsWith("__");
                bool yUnder = yName.StartsWith("__") && yName.EndsWith("__");

                if (xUnder != yUnder)
                {
                    // The one that starts with an underscore comes later
                    return xUnder ? 1 : -1;
                }

                bool xSingleUnder = xName.StartsWith("_");
                bool ySingleUnder = yName.StartsWith("_");
                if (xSingleUnder != ySingleUnder)
                {
                    // The one that starts with an underscore comes later
                    return xSingleUnder ? 1 : -1;
                }
            }
            return String.Compare(xName, yName, StringComparison.CurrentCultureIgnoreCase);
        }

        private CompletionComparer(bool sortUnderscoresLast)
        {
            _sortUnderscoresLast = sortUnderscoresLast;
        }

        /// <summary>
        /// Compares two instances of <see cref="Completion"/> using their
        /// displayed text.
        /// </summary>
        public int Compare(Completion x, Completion y)
        {
            return Compare(x.DisplayText, y.DisplayText);
        }

        public bool Equals(Completion x, Completion y)
        {
            return Compare(x.DisplayText, y.DisplayText) == 0;
        }

        public int GetHashCode(Completion obj)
        {
            return obj.DisplayText.GetHashCode();
        }
    }
}
