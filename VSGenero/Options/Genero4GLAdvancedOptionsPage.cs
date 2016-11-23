using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace VSGenero.Options
{
    public enum AdvancedOptions
    {
        None = 0,
        ShowFunctionParameters = 1,
        MajorCollapseRegions = 2,
        MinorCollapseRegions = 4,
        CustomCollapseRegions = 8,
        SemanticErrorChecking = 16,
        IncludeAllFunctions = 32,
        OpenExternalBrowser = 64
    }

    [ComVisible(true)]
    public class Genero4GLAdvancedOptionsPage : GeneroDialogPage
    {
        private Genero4GLAdvancedOptionsControl _window;

        private AdvancedOptions _optionsChanged;
        private bool _showFunctionParameters;
        private bool _includeAllFunctions;

        private bool _majorCollapseRegionsEnabled;
        private bool _minorCollapseRegionsEnabled;
        private bool _customCollapseRegionsEnabled;
        private bool _semanticErrorCheckingEnabled;
        private bool _openExternalBrowser;

        public Genero4GLAdvancedOptionsPage()
            : base("Advanced")
        {
            _optionsChanged = AdvancedOptions.None;
        }

        // replace the default UI of the dialog page w/ our own UI.
        protected override System.Windows.Forms.IWin32Window Window
        {
            get
            {
                if (_window == null)
                {
                    _window = new Genero4GLAdvancedOptionsControl();
                }
                return _window;
            }
        }

        public bool ShowFunctionParametersInList
        {
            get { return _showFunctionParameters; }
            set 
            {
                if (_showFunctionParameters != value)
                {
                    _showFunctionParameters = value;
                    _optionsChanged |= AdvancedOptions.ShowFunctionParameters;
                }
            }
        }

        public bool IncludeAllFunctions
        {
            get { return _includeAllFunctions; }
            set
            {
                if (_includeAllFunctions != value)
                {
                    _includeAllFunctions = value;
                    _optionsChanged |= AdvancedOptions.IncludeAllFunctions;
                }
            }
        }

        public bool MajorCollapseRegionsEnabled
        {
            get { return _majorCollapseRegionsEnabled; }
            set
            {
                if(_majorCollapseRegionsEnabled != value)
                {
                    _majorCollapseRegionsEnabled = value;
                    _optionsChanged |= AdvancedOptions.MajorCollapseRegions;
                }
            }
        }

        public bool MinorCollapseRegionsEnabled
        {
            get { return _minorCollapseRegionsEnabled; }
            set
            {
                if (_minorCollapseRegionsEnabled != value)
                {
                    _minorCollapseRegionsEnabled = value;
                    _optionsChanged |= AdvancedOptions.MinorCollapseRegions;
                }
            }
        }

        public bool CustomCollapseRegionsEnabled
        {
            get { return _customCollapseRegionsEnabled; }
            set
            {
                if (_customCollapseRegionsEnabled != value)
                {
                    _customCollapseRegionsEnabled = value;
                    _optionsChanged |= AdvancedOptions.CustomCollapseRegions;
                }
            }
        }

        public bool SemanticErrorCheckingEnabled
        {
            get { return _semanticErrorCheckingEnabled; }
            set
            {
                if (_semanticErrorCheckingEnabled != value)
                {
                    _semanticErrorCheckingEnabled = value;
                    _optionsChanged |= AdvancedOptions.SemanticErrorChecking;
                }
            }
        }

        public bool OpenExternalBrowser
        {
            get { return _openExternalBrowser; }
            set
            {
                if(_openExternalBrowser != value)
                {
                    _openExternalBrowser = value;
                    _optionsChanged |= AdvancedOptions.OpenExternalBrowser;
                }
            }
        }

        public AdvancedOptions OptionsChanged
        {
            get
            {
                return _optionsChanged;
            }
        }

        public void SetChangesApplied()
        {
            _optionsChanged = AdvancedOptions.None;
        }

        public override void ResetSettings()
        {
            _showFunctionParameters = true;
        }

        private const string ShowFunctionParametersSetting = "ShowFunctionParametersInList";
        private const string MajorCollapseRegionsEnabledSetting = "MajorCollapseRegionsEnabled";
        private const string MinorCollapseRegionsEnabledSetting = "MinorCollapseRegionsEnabled";
        private const string CustomCollapseRegionsEnabledSetting = "CustomCollapseRegionsEnabled";
        private const string SemanticErrorCheckingEnabledSetting = "SemanticErrorCheckingEnabled";
        private const string OpenExternalBrowserEnabledSetting = "OpenExternalBrowserEnabled";
        private const string IncludeAllFunctionsSetting = "IncludeAllFunctions";

        public override void LoadSettingsFromStorage()
        {
            _showFunctionParameters = LoadBool(ShowFunctionParametersSetting) ?? true;
            _includeAllFunctions = LoadBool(IncludeAllFunctionsSetting) ?? true;
            _minorCollapseRegionsEnabled = LoadBool(MinorCollapseRegionsEnabledSetting) ?? true;
            _majorCollapseRegionsEnabled = LoadBool(MajorCollapseRegionsEnabledSetting) ?? true;
            _customCollapseRegionsEnabled = LoadBool(CustomCollapseRegionsEnabledSetting) ?? true;
            _semanticErrorCheckingEnabled = LoadBool(SemanticErrorCheckingEnabledSetting) ?? false;     // TODO: for right now, I'm defaulting the semantic error checking to false
            _openExternalBrowser = LoadBool(OpenExternalBrowserEnabledSetting) ?? false;

            if (_optionsChanged != AdvancedOptions.None)
            {
                VSGeneroPackage.Instance.LangPrefs.OnUserPreferencesChanged2(null, null, null, null);
            }

            _optionsChanged = AdvancedOptions.None;
        }

        public override void SaveSettingsToStorage()
        {
            SaveBool(ShowFunctionParametersSetting, _showFunctionParameters);
            SaveBool(IncludeAllFunctionsSetting, _includeAllFunctions);
            SaveBool(MinorCollapseRegionsEnabledSetting, _minorCollapseRegionsEnabled);
            SaveBool(MajorCollapseRegionsEnabledSetting, _majorCollapseRegionsEnabled);
            SaveBool(CustomCollapseRegionsEnabledSetting, _customCollapseRegionsEnabled);
            SaveBool(SemanticErrorCheckingEnabledSetting, _semanticErrorCheckingEnabled);
            SaveBool(OpenExternalBrowserEnabledSetting, _openExternalBrowser);
        }
    }
}
