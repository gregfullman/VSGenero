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
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            VSGeneroPackage.Instance.AdvancedOptions4GLPage.ShowFunctionParametersInList = checkBoxShowFunctionParams.Checked;
        }
    }
}
