using Microsoft.VisualStudio.VSCommon.Options;

namespace VSGenero.Options
{
    public class Genero4GLAdvancedOptions : BaseOptions
    {
        public Genero4GLAdvancedOptions() 
            : base(VSGeneroConstants.BaseRegistryKey, "Advanced")
        {
        }

        public bool ShowFunctionParametersInList { get { return GetValue<bool>(ShowFunctionParametersSetting); } }

        public bool IncludeAllFunctions {  get { return GetValue<bool>(IncludeAllFunctionsSetting); } }

        public bool MajorCollapseRegionsEnabled { get { return GetValue<bool>(MajorCollapseRegionsEnabledSetting); } }

        public bool MinorCollapseRegionsEnabled {  get { return GetValue<bool>(MinorCollapseRegionsEnabledSetting); } }

        public bool CustomCollapseRegionsEnabled {  get { return GetValue<bool>(CustomCollapseRegionsEnabledSetting); } }

        public bool SemanticErrorCheckingEnabled { get { return GetValue<bool>(SemanticErrorCheckingEnabledSetting); } }

        public bool OpenExternalBrowser { get { return GetValue<bool>(OpenExternalBrowserEnabledSetting); } }

        public const string ShowFunctionParametersSetting = "ShowFunctionParametersInList";
        public const string MajorCollapseRegionsEnabledSetting = "MajorCollapseRegionsEnabled";
        public const string MinorCollapseRegionsEnabledSetting = "MinorCollapseRegionsEnabled";
        public const string CustomCollapseRegionsEnabledSetting = "CustomCollapseRegionsEnabled";
        public const string SemanticErrorCheckingEnabledSetting = "SemanticErrorCheckingEnabled";
        public const string OpenExternalBrowserEnabledSetting = "OpenExternalBrowserEnabled";
        public const string IncludeAllFunctionsSetting = "IncludeAllFunctions";


        protected override void LoadSettingsFromStorage()
        {
            LoadBool(ShowFunctionParametersSetting, true);
            LoadBool(IncludeAllFunctionsSetting, true);
            LoadBool(MinorCollapseRegionsEnabledSetting, true);
            LoadBool(MajorCollapseRegionsEnabledSetting, true);
            LoadBool(CustomCollapseRegionsEnabledSetting, true);
            LoadBool(SemanticErrorCheckingEnabledSetting, false);
            LoadBool(OpenExternalBrowserEnabledSetting, false);
        }
    }
}
