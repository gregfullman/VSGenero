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
using System.Threading.Tasks;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio;
using VSGenero.Navigation;
using VSGenero.Options;

namespace VSGenero
{
    class Genero4GLLanguagePreferences : IVsTextManagerEvents2
    {
        LANGPREFERENCES _preferences;

        public Genero4GLLanguagePreferences(LANGPREFERENCES preferences)
        {
            _preferences = preferences;
        }

        #region IVsTextManagerEvents2 Members

        public int OnRegisterMarkerType(int iMarkerType)
        {
            return VSConstants.S_OK;
        }

        public int OnRegisterView(IVsTextView pView)
        {
            return VSConstants.S_OK;
        }

        public int OnReplaceAllInFilesBegin()
        {
            return VSConstants.S_OK;
        }

        public int OnReplaceAllInFilesEnd()
        {
            return VSConstants.S_OK;
        }

        public int OnUnregisterView(IVsTextView pView)
        {
            return VSConstants.S_OK;
        }

        public int OnUserPreferencesChanged2(VIEWPREFERENCES2[] viewPrefs, FRAMEPREFERENCES2[] framePrefs, LANGPREFERENCES2[] langPrefs, FONTCOLORPREFERENCES2[] colorPrefs)
        {
            if (langPrefs != null && langPrefs.Length > 0 && langPrefs[0].guidLang == this._preferences.guidLang)
            {
                _preferences.IndentStyle = langPrefs[0].IndentStyle;
                _preferences.fAutoListMembers = langPrefs[0].fAutoListMembers;
                _preferences.fAutoListParams = langPrefs[0].fAutoListParams;
                _preferences.fHideAdvancedAutoListMembers = langPrefs[0].fHideAdvancedAutoListMembers;
                if (_preferences.fDropdownBar != (_preferences.fDropdownBar = langPrefs[0].fDropdownBar))
                {
                    VSGeneroCodeWindowManager.ToggleNavigationBar(_preferences.fDropdownBar != 0);
                }
            }

            var optionsChanged = VSGeneroPackage.Instance.AdvancedOptions4GLPage.OptionsChanged;

            if (optionsChanged.HasFlag(AdvancedOptions.ShowFunctionParameters))
            {
                VSGeneroCodeWindowManager.RefreshNavigationBar();
                VSGeneroPackage.Instance.AdvancedOptions4GLPage.SetChangesApplied();
            }
            if(optionsChanged.HasFlag(AdvancedOptions.MajorCollapseRegions) ||
               optionsChanged.HasFlag(AdvancedOptions.MinorCollapseRegions) ||
               optionsChanged.HasFlag(AdvancedOptions.CustomCollapseRegions))
            {
                // TODO: update the outliner

            }
            if(optionsChanged.HasFlag(AdvancedOptions.SemanticErrorChecking))
            {
                // TODO: update the semantic error checker
            }
            return VSConstants.S_OK;
        }

        #endregion

        #region Options

        public vsIndentStyle IndentMode
        {
            get
            {
                return _preferences.IndentStyle;
            }
        }

        public bool NavigationBar
        {
            get
            {
                // TODO: When this value changes we need to update all our views
                return _preferences.fDropdownBar != 0;
            }
        }

        public bool HideAdvancedMembers
        {
            get
            {
                return _preferences.fHideAdvancedAutoListMembers != 0;
            }
        }

        public bool AutoListMembers
        {
            get
            {
                return _preferences.fAutoListMembers != 0;
            }
        }

        public bool AutoListParams
        {
            get
            {
                return _preferences.fAutoListParams != 0;
            }
        }


        #endregion
    }
}
