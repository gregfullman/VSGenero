using Microsoft.VisualStudio.VSCommon.Options;
using System;

namespace VSGenero.Options
{
    public partial class Genero4GLAdvancedOptionsControl : BaseOptionsUserControl
    {
        public Genero4GLAdvancedOptionsControl()
        {
            InitializeComponent();
            InitializeData();
        }

        protected override void Initialize()
        {
            checkBoxShowFunctionParams.Checked = VSGeneroPackage.Instance.AdvancedOptions4GL.ShowFunctionParametersInList;
            checkBoxIncludeAllFunctions.Checked = VSGeneroPackage.Instance.AdvancedOptions4GL.IncludeAllFunctions;
            checkBoxMinorCollapseRegions.Checked = VSGeneroPackage.Instance.AdvancedOptions4GL.MinorCollapseRegionsEnabled;
            checkBoxMajorCollapseRegions.Checked = VSGeneroPackage.Instance.AdvancedOptions4GL.MajorCollapseRegionsEnabled;
            checkBoxCustomCollapseRegions.Checked = VSGeneroPackage.Instance.AdvancedOptions4GL.CustomCollapseRegionsEnabled;
            checkBoxSemanticErrorChecking.Checked = VSGeneroPackage.Instance.AdvancedOptions4GL.SemanticErrorCheckingEnabled;
            checkBoxOpenExternalBrowser.Checked = VSGeneroPackage.Instance.AdvancedOptions4GL.OpenExternalBrowser;
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if(!IsInitializing)
                VSGeneroPackage.Instance.AdvancedOptions4GL.SetPendingValue(Genero4GLAdvancedOptions.ShowFunctionParametersSetting, checkBoxShowFunctionParams.Checked);
        }

        private void checkBoxMajorCollapseRegions_CheckedChanged(object sender, EventArgs e)
        {
            if (!IsInitializing)
                VSGeneroPackage.Instance.AdvancedOptions4GL.SetPendingValue(Genero4GLAdvancedOptions.MajorCollapseRegionsEnabledSetting, checkBoxMajorCollapseRegions.Checked);
        }

        private void checkBoxMinorCollapseRegions_CheckedChanged(object sender, EventArgs e)
        {
            if (!IsInitializing)
                VSGeneroPackage.Instance.AdvancedOptions4GL.SetPendingValue(Genero4GLAdvancedOptions.MinorCollapseRegionsEnabledSetting, checkBoxMinorCollapseRegions.Checked);
        }

        private void checkBoxCustomCollapseRegions_CheckedChanged(object sender, EventArgs e)
        {
            if (!IsInitializing)
                VSGeneroPackage.Instance.AdvancedOptions4GL.SetPendingValue(Genero4GLAdvancedOptions.CustomCollapseRegionsEnabledSetting, checkBoxCustomCollapseRegions.Checked);
        }

        private void checkBoxSemanticErrorChecking_CheckedChanged(object sender, EventArgs e)
        {
            if (!IsInitializing)
                VSGeneroPackage.Instance.AdvancedOptions4GL.SetPendingValue(Genero4GLAdvancedOptions.SemanticErrorCheckingEnabledSetting, checkBoxSemanticErrorChecking.Checked);
        }

        private void checkBoxIncludeAllFunctions_CheckedChanged(object sender, EventArgs e)
        {
            if (!IsInitializing)
                VSGeneroPackage.Instance.AdvancedOptions4GL.SetPendingValue(Genero4GLAdvancedOptions.IncludeAllFunctionsSetting, checkBoxIncludeAllFunctions.Checked);
        }

        private void checkBoxOpenExternalBrowser_CheckedChanged(object sender, EventArgs e)
        {
            if (!IsInitializing)
                VSGeneroPackage.Instance.AdvancedOptions4GL.SetPendingValue(Genero4GLAdvancedOptions.OpenExternalBrowserEnabledSetting, checkBoxOpenExternalBrowser.Checked);
        }
    }
}
