using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace VSGenero.Options
{
    public partial class Genero4GLAdvancedOptionsControl : UserControl
    {
        public Genero4GLAdvancedOptionsControl()
        {
            InitializeComponent();
            checkBoxShowFunctionParams.Checked = VSGeneroPackage.Instance.AdvancedOptions4GLPage.ShowFunctionParametersInList;
            checkBoxIncludeAllFunctions.Checked = VSGeneroPackage.Instance.AdvancedOptions4GLPage.IncludeAllFunctions;
            checkBoxMinorCollapseRegions.Checked = VSGeneroPackage.Instance.AdvancedOptions4GLPage.MinorCollapseRegionsEnabled;
            checkBoxMajorCollapseRegions.Checked = VSGeneroPackage.Instance.AdvancedOptions4GLPage.MajorCollapseRegionsEnabled;
            checkBoxCustomCollapseRegions.Checked = VSGeneroPackage.Instance.AdvancedOptions4GLPage.CustomCollapseRegionsEnabled;
            checkBoxSemanticErrorChecking.Checked = VSGeneroPackage.Instance.AdvancedOptions4GLPage.SemanticErrorCheckingEnabled;
            checkBoxOpenExternalBrowser.Checked = VSGeneroPackage.Instance.AdvancedOptions4GLPage.OpenExternalBrowser;
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            VSGeneroPackage.Instance.AdvancedOptions4GLPage.ShowFunctionParametersInList = checkBoxShowFunctionParams.Checked;
        }

        private void checkBoxMajorCollapseRegions_CheckedChanged(object sender, EventArgs e)
        {
            VSGeneroPackage.Instance.AdvancedOptions4GLPage.MajorCollapseRegionsEnabled = checkBoxMajorCollapseRegions.Checked;
        }

        private void checkBoxMinorCollapseRegions_CheckedChanged(object sender, EventArgs e)
        {
            VSGeneroPackage.Instance.AdvancedOptions4GLPage.MinorCollapseRegionsEnabled = checkBoxMinorCollapseRegions.Checked;
        }

        private void checkBoxCustomCollapseRegions_CheckedChanged(object sender, EventArgs e)
        {
            VSGeneroPackage.Instance.AdvancedOptions4GLPage.CustomCollapseRegionsEnabled = checkBoxCustomCollapseRegions.Checked;
        }

        private void checkBoxSemanticErrorChecking_CheckedChanged(object sender, EventArgs e)
        {
            VSGeneroPackage.Instance.AdvancedOptions4GLPage.SemanticErrorCheckingEnabled = checkBoxSemanticErrorChecking.Checked;
        }

        private void checkBoxIncludeAllFunctions_CheckedChanged(object sender, EventArgs e)
        {
            VSGeneroPackage.Instance.AdvancedOptions4GLPage.IncludeAllFunctions = checkBoxIncludeAllFunctions.Checked;
        }

        private void checkBoxOpenExternalBrowser_CheckedChanged(object sender, EventArgs e)
        {
            VSGeneroPackage.Instance.AdvancedOptions4GLPage.OpenExternalBrowser = checkBoxOpenExternalBrowser.Checked;
        }
    }
}
