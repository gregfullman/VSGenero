/* ****************************************************************************
 * Copyright (c) 2014 Greg Fullman 
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
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using System.Windows.Media;

namespace VSGenero.EditorExtensions.Intellisense
{
    public class MemberCompletion : Completion
    {
        public MemberCompletion(string displayText, string insertionText, string description, ImageSource iconSource, string iconAutomationText)
            : base(displayText, insertionText, description, iconSource, iconAutomationText)
        {
        }

        private bool _visible, _previouslyVisible;
        internal bool Visible
        {
            get
            {
                return _visible;
            }
            set
            {
                _previouslyVisible = _visible;
                _visible = value;
            }
        }

        /// <summary>
        /// Resets <see cref="Visible"/> to its value before it was last set.
        /// </summary>
        internal void UndoVisible()
        {
            _visible = _previouslyVisible;
        }
    }

    public class MemberCompletionSet : CompletionSet
    {
        BulkObservableCollection<Completion> _completions;
        FilteredObservableCollection<Completion> _filteredCompletions;
        readonly FuzzyStringMatcher _comparer;
        Completion _previousSelection;

        public MemberCompletionSet(string moniker, string displayName, ITrackingSpan applicableTo, IEnumerable<MemberCompletion> completions, IComparer<Completion> comparer)
            : base(moniker, displayName, applicableTo, null, null)
        {
            _completions = new BulkObservableCollection<Completion>();
            _completions.AddRange(completions
                .Where(c => c != null && !string.IsNullOrWhiteSpace(c.DisplayText))
                .OrderBy(c => c, comparer)
            );
            _comparer = new FuzzyStringMatcher(FuzzyMatchMode.FuzzyIgnoreLowerCase); // TODO: make this configurable

            _filteredCompletions = new FilteredObservableCollection<Completion>(_completions);

            var text = ApplicableTo.GetText(ApplicableTo.TextBuffer.CurrentSnapshot);
            foreach (var c in _completions.Cast<MemberCompletion>())
            {
                c.Visible = true;
            }
            _filteredCompletions.Filter(IsVisible);
        }

        private static bool IsVisible(Completion completion)
        {
            return ((MemberCompletion)completion).Visible;
        }

        /// <summary>
        /// Gets or sets the list of completions that are part of this completion set.
        /// </summary>
        /// <value>
        /// A list of <see cref="Completion"/> objects.
        /// </value>
        public override IList<Completion> Completions
        {
            get
            {
                return (IList<Completion>)_filteredCompletions ?? _completions;
            }
        }

        public override void Filter()
        {
            if (_filteredCompletions == null)
            {
                foreach (var c in _completions.Cast<MemberCompletion>())
                {
                    c.Visible = true;
                }
                return;
            }

            var text = ApplicableTo.GetText(ApplicableTo.TextBuffer.CurrentSnapshot);
            if (text.Length > 0)
            {
                bool anyVisible = false;
                foreach (var c in _completions.Cast<MemberCompletion>())
                {
                    //if (_shouldHideAdvanced && IsAdvanced(c) && !text.StartsWith("__"))
                    //{
                    //    c.Visible = false;
                    //}
                    //else if (_shouldFilter)
                    //{
                    c.Visible = _comparer.IsCandidateMatch(c.DisplayText, text);
                    //}
                    //else
                    //{
                    //    c.Visible = true;
                    //}
                    anyVisible |= c.Visible;
                }
                if (!anyVisible)
                {
                    foreach (var c in _completions.Cast<MemberCompletion>())
                    {
                        // UndoVisible only works reliably because we always
                        // set Visible in the previous loop.
                        c.UndoVisible();
                    }
                }
                _filteredCompletions.Filter(IsVisible);
            }
            //else if (_shouldHideAdvanced)
            //{
            //    foreach (var c in _completions.Cast<MemberCompletion>())
            //    {
            //        c.Visible = !IsAdvanced(c);
            //    }
            //    _filteredCompletions.Filter(IsVisible);
            //}
            else
            {
                foreach (var c in _completions.Cast<MemberCompletion>())
                {
                    c.Visible = true;
                }
                _filteredCompletions.StopFiltering();
            }
        }

        /// <summary>
        /// Determines the best match in the completion set.
        /// </summary>
        public override void SelectBestMatch()
        {
            var text = ApplicableTo.GetText(ApplicableTo.TextBuffer.CurrentSnapshot);

            Completion bestMatch = _previousSelection;
            int bestValue = 0;
            bool isUnique = true;
            bool allowSelect = true;

            // Using the Completions property to only search through visible
            // completions.
            foreach (var comp in Completions)
            {
                int value = _comparer.GetSortKey(comp.DisplayText, text);
                if (bestMatch == null || value > bestValue)
                {
                    bestMatch = comp;
                    bestValue = value;
                    isUnique = true;
                }
                else if (value == bestValue)
                {
                    isUnique = false;
                }
            }

            if (Moniker == "PythonOverrides")
            {
                allowSelect = false;
                isUnique = false;
            }

            if (((MemberCompletion)bestMatch).Visible)
            {
                SelectionStatus = new CompletionSelectionStatus(bestMatch,
                    isSelected: allowSelect && bestValue > 0,
                    isUnique: isUnique);
            }
            else
            {
                SelectionStatus = new CompletionSelectionStatus(null,
                    isSelected: false,
                    isUnique: false);
            }

            _previousSelection = bestMatch;
        }

        /// <summary>
        /// Determines and selects the only match in the completion set.
        /// This ignores the user's filtering preferences.
        /// </summary>
        /// <returns>
        /// True if a match is found and selected; otherwise, false if there
        /// is no single match in the completion set.
        /// </returns> 
        public bool SelectSingleBest()
        {
            var text = ApplicableTo.GetText(ApplicableTo.TextBuffer.CurrentSnapshot);

            Completion bestMatch = null;

            // Using the _completions field to search through all completions
            // and ignore filtering settings.
            foreach (var comp in _completions)
            {
                if (_comparer.IsCandidateMatch(comp.DisplayText, text))
                {
                    if (bestMatch == null)
                    {
                        bestMatch = comp;
                    }
                    else
                    {
                        return false;
                    }
                }
            }

            if (bestMatch != null)
            {
                SelectionStatus = new CompletionSelectionStatus(bestMatch,
                    isSelected: true,
                    isUnique: true);
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
