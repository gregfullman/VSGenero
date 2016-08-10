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
using VSGenero.Analysis.Parsing.AST_4GL;

namespace VSGenero.Options
{
    public partial class Genero4GLIntellisenseOptionsControl : UserControl
    {
        private Timer _timer;

        public Genero4GLIntellisenseOptionsControl()
        {
            InitializeComponent();
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

        private async void buttonReloadContexts_Click(object sender, EventArgs e)
        {
            bool success = await Genero4glAst.ReloadContextMap();
            if(success)
            {
                labelReloadResult.Text = "Reload succeeded!";
                labelReloadResult.ForeColor = Color.DarkGreen;
                labelReloadResult.Visible = true;
            }
            else
            {
                labelReloadResult.Text = "Reload failed.";
                labelReloadResult.ForeColor = Color.DarkRed;
                labelReloadResult.Visible = true;
            }

            _timer = new Timer();
            _timer.Interval = 5000;
            _timer.Tick += _timer_Tick;
            _timer.Start();
        }

        private void _timer_Tick(object sender, EventArgs e)
        {
            if (_timer != null)
            {
                _timer.Stop();
                _timer.Tick -= _timer_Tick;
                if (labelReloadResult.Visible)
                    labelReloadResult.Visible = false;
                if (labelDownloadResult.Visible)
                    labelDownloadResult.Visible = false;
                _timer.Dispose();
                _timer = null;
            }
        }

        private async void buttonDownloadLatest_Click(object sender, EventArgs e)
        {
            bool success = await Genero4glAst.ReloadContextMap(true);
            if (success)
            {
                labelDownloadResult.Text = "Download/update succeeded!";
                labelDownloadResult.ForeColor = Color.DarkGreen;
                labelDownloadResult.Visible = true;
            }
            else
            {
                labelDownloadResult.Text = "Download/update failed.";
                labelDownloadResult.ForeColor = Color.DarkRed;
                labelDownloadResult.Visible = true;
            }

            _timer = new Timer();
            _timer.Interval = 5000;
            _timer.Tick += _timer_Tick;
            _timer.Start();
        }
    }
}
