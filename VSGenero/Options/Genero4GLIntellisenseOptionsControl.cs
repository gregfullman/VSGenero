using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using VSGenero.EditorExtensions.Intellisense;

namespace VSGenero.Options
{
    public partial class Genero4GLIntellisenseOptionsControl : UserControl
    {
        public Genero4GLIntellisenseOptionsControl()
        {
            InitializeComponent();
            checkBoxCompletionList.Checked = VSGeneroPackage.Instance.IntellisenseOptions4GLPage.ShowCompletionList;
            checkBoxNewLineOnFullyTypedWord.Checked = VSGeneroPackage.Instance.IntellisenseOptions4GLPage.AddNewLineAtEndOfFullyTypedWord;
            checkBoxPreselectMRU.Checked = VSGeneroPackage.Instance.IntellisenseOptions4GLPage.PreSelectMRU;
            checkBoxSpacebarCommits.Checked = VSGeneroPackage.Instance.IntellisenseOptions4GLPage.SpaceCommitsIntellisense;
            textBoxCommitChars.Text = VSGeneroPackage.Instance.IntellisenseOptions4GLPage.CompletionCommittedBy;
            switch(VSGeneroPackage.Instance.IntellisenseOptions4GLPage.AnalysisType)
            {
                case CompletionAnalysisType.Context: radioButtonContextCompletionType.Checked = true; break;
                case CompletionAnalysisType.Normal: radioButtonNormalCompletionType.Checked = true; break;
            }
        }

        private void checkBoxSpacebarCommits_CheckedChanged(object sender, EventArgs e)
        {
            VSGeneroPackage.Instance.IntellisenseOptions4GLPage.SpaceCommitsIntellisense = checkBoxSpacebarCommits.Checked;
        }

        private void checkBoxNewLineOnFullyTypedWord_CheckedChanged(object sender, EventArgs e)
        {
            VSGeneroPackage.Instance.IntellisenseOptions4GLPage.AddNewLineAtEndOfFullyTypedWord = checkBoxNewLineOnFullyTypedWord.Checked;
        }

        private void checkBoxPreselectMRU_CheckedChanged(object sender, EventArgs e)
        {
            VSGeneroPackage.Instance.IntellisenseOptions4GLPage.PreSelectMRU = checkBoxPreselectMRU.Checked;
        }

        private void textBoxCommitChars_TextChanged(object sender, EventArgs e)
        {
            VSGeneroPackage.Instance.IntellisenseOptions4GLPage.CompletionCommittedBy = textBoxCommitChars.Text;
        }

        private void radioButtonNormalCompletionType_CheckedChanged(object sender, EventArgs e)
        {
            VSGeneroPackage.Instance.IntellisenseOptions4GLPage.AnalysisType = CompletionAnalysisType.Normal;
        }

        private void radioButtonContextCompletionType_CheckedChanged(object sender, EventArgs e)
        {
            VSGeneroPackage.Instance.IntellisenseOptions4GLPage.AnalysisType = CompletionAnalysisType.Context;
        }
    }
}
