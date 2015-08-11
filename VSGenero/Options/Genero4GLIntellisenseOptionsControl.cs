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
    public partial class Genero4GLIntellisenseOptionsControl : UserControl
    {
        public Genero4GLIntellisenseOptionsControl()
        {
            InitializeComponent();
            checkBoxCompletionList.Checked = VSGeneroPackage.Instance.IntellisenseOptions4GLPage.ShowCompletionList;
            if(checkBoxCompletionList.Checked)
            {
                checkBoxSpaceTriggersCompletionList.Checked = VSGeneroPackage.Instance.IntellisenseOptions4GLPage.SpacebarShowsCompletionList;
                checkBoxSpaceTriggersCompletionList.Enabled = true;
            }
            else
            {
                checkBoxSpaceTriggersCompletionList.Checked = false;
                checkBoxSpaceTriggersCompletionList.Enabled = false;
            }
            checkBoxEnterCommits.Checked = VSGeneroPackage.Instance.IntellisenseOptions4GLPage.EnterCommitsIntellisense;
            checkBoxNewLineOnFullyTypedWord.Checked = VSGeneroPackage.Instance.IntellisenseOptions4GLPage.AddNewLineAtEndOfFullyTypedWord;
            checkBoxPreselectMRU.Checked = VSGeneroPackage.Instance.IntellisenseOptions4GLPage.PreSelectMRU;
            checkBoxSpacebarCommits.Checked = VSGeneroPackage.Instance.IntellisenseOptions4GLPage.SpaceCommitsIntellisense;
            textBoxCommitChars.Text = VSGeneroPackage.Instance.IntellisenseOptions4GLPage.CompletionCommittedBy;
        }

        private void checkBoxCompletionList_CheckedChanged(object sender, EventArgs e)
        {
            VSGeneroPackage.Instance.IntellisenseOptions4GLPage.ShowCompletionList = checkBoxCompletionList.Checked;
            if (checkBoxCompletionList.Checked)
            {
                checkBoxSpaceTriggersCompletionList.Enabled = true;
            }
            else
            {
                checkBoxSpaceTriggersCompletionList.Checked = false;
                checkBoxSpaceTriggersCompletionList.Enabled = false;
            }
        }

        private void checkBoxSpacebarCommits_CheckedChanged(object sender, EventArgs e)
        {
            VSGeneroPackage.Instance.IntellisenseOptions4GLPage.SpaceCommitsIntellisense = checkBoxSpacebarCommits.Checked;
        }

        private void checkBoxEnterCommits_CheckedChanged(object sender, EventArgs e)
        {
            VSGeneroPackage.Instance.IntellisenseOptions4GLPage.EnterCommitsIntellisense = checkBoxEnterCommits.Checked;
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

        private void checkBoxSpaceTriggersCompletionList_CheckedChanged(object sender, EventArgs e)
        {
            VSGeneroPackage.Instance.IntellisenseOptions4GLPage.SpacebarShowsCompletionList = checkBoxSpaceTriggersCompletionList.Checked;
        }
    }
}
