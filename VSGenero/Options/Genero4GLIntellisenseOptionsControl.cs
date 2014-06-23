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
            checkBoxCompletionList.Checked = VSGeneroPackage.Instance.IntellisenseOptionsPage.ShowCompletionList;
            checkBoxEnterCommits.Checked = VSGeneroPackage.Instance.IntellisenseOptionsPage.EnterCommitsIntellisense;
            checkBoxNewLineOnFullyTypedWord.Checked = VSGeneroPackage.Instance.IntellisenseOptionsPage.AddNewLineAtEndOfFullyTypedWord;
            checkBoxPreselectMRU.Checked = VSGeneroPackage.Instance.IntellisenseOptionsPage.PreSelectMRU;
            checkBoxSpacebarCommits.Checked = VSGeneroPackage.Instance.IntellisenseOptionsPage.SpaceCommitsIntellisense;
            textBoxCommitChars.Text = VSGeneroPackage.Instance.IntellisenseOptionsPage.CompletionCommittedBy;
        }

        private void checkBoxCompletionList_CheckedChanged(object sender, EventArgs e)
        {
            VSGeneroPackage.Instance.IntellisenseOptionsPage.ShowCompletionList = checkBoxCompletionList.Checked;
        }

        private void checkBoxSpacebarCommits_CheckedChanged(object sender, EventArgs e)
        {
            VSGeneroPackage.Instance.IntellisenseOptionsPage.SpaceCommitsIntellisense = checkBoxSpacebarCommits.Checked;
        }

        private void checkBoxEnterCommits_CheckedChanged(object sender, EventArgs e)
        {
            VSGeneroPackage.Instance.IntellisenseOptionsPage.EnterCommitsIntellisense = checkBoxEnterCommits.Checked;
        }

        private void checkBoxNewLineOnFullyTypedWord_CheckedChanged(object sender, EventArgs e)
        {
            VSGeneroPackage.Instance.IntellisenseOptionsPage.AddNewLineAtEndOfFullyTypedWord = checkBoxNewLineOnFullyTypedWord.Checked;
        }

        private void checkBoxPreselectMRU_CheckedChanged(object sender, EventArgs e)
        {
            VSGeneroPackage.Instance.IntellisenseOptionsPage.PreSelectMRU = checkBoxPreselectMRU.Checked;
        }

        private void textBoxCommitChars_TextChanged(object sender, EventArgs e)
        {
            VSGeneroPackage.Instance.IntellisenseOptionsPage.CompletionCommittedBy = textBoxCommitChars.Text;
        }
    }
}
