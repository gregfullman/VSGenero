using Microsoft.VisualStudio.VSCommon.Options;
using System;
using System.Drawing;
using System.Windows.Forms;
using VSGenero.Analysis.Parsing.AST_4GL;
using VSGenero.EditorExtensions.Intellisense;

namespace VSGenero.Options
{
    public partial class Genero4GLIntellisenseOptionsControl : BaseOptionsUserControl
    {
        private Timer _timer;

        public Genero4GLIntellisenseOptionsControl()
        {
            InitializeComponent();
            InitializeData();
        }

        protected override void Initialize()
        {
            checkBoxNewLineOnFullyTypedWord.Checked = VSGeneroPackage.Instance.IntellisenseOptions4GL.AddNewLineAtEndOfFullyTypedWord;
            checkBoxPreselectMRU.Checked = VSGeneroPackage.Instance.IntellisenseOptions4GL.PreSelectMRU;
            checkBoxSpacebarCommits.Checked = VSGeneroPackage.Instance.IntellisenseOptions4GL.SpaceCommitsIntellisense;
            textBoxCommitChars.Text = VSGeneroPackage.Instance.IntellisenseOptions4GL.CompletionCommittedBy;
            switch (VSGeneroPackage.Instance.IntellisenseOptions4GL.AnalysisType)
            {
                case CompletionAnalysisType.Context: radioButtonContextCompletionType.Checked = true; break;
                case CompletionAnalysisType.Normal: radioButtonNormalCompletionType.Checked = true; break;
            }
        }

        private void checkBoxSpacebarCommits_CheckedChanged(object sender, EventArgs e)
        {
            if(!IsInitializing)
                VSGeneroPackage.Instance.IntellisenseOptions4GL.SetPendingValue(Genero4GLIntellisenseOptions.SpaceCommitsSetting, checkBoxSpacebarCommits.Checked);
        }

        private void checkBoxNewLineOnFullyTypedWord_CheckedChanged(object sender, EventArgs e)
        {
            if (!IsInitializing)
                VSGeneroPackage.Instance.IntellisenseOptions4GL.SetPendingValue(Genero4GLIntellisenseOptions.NewLineAtEndOfWordSetting, checkBoxNewLineOnFullyTypedWord.Checked);
        }

        private void checkBoxPreselectMRU_CheckedChanged(object sender, EventArgs e)
        {
            if (!IsInitializing)
                VSGeneroPackage.Instance.IntellisenseOptions4GL.SetPendingValue(Genero4GLIntellisenseOptions.PreSelectMRUSetting, checkBoxPreselectMRU.Checked);
        }

        private void textBoxCommitChars_TextChanged(object sender, EventArgs e)
        {
            if (!IsInitializing)
                VSGeneroPackage.Instance.IntellisenseOptions4GL.SetPendingValue(Genero4GLIntellisenseOptions.CompletionCommittedBySetting, textBoxCommitChars.Text);
        }

        private void radioButtonNormalCompletionType_CheckedChanged(object sender, EventArgs e)
        {
            if (!IsInitializing)
                VSGeneroPackage.Instance.IntellisenseOptions4GL.SetPendingValue(Genero4GLIntellisenseOptions.CompletionAnalysisTypeSetting, CompletionAnalysisType.Normal);
        }

        private void radioButtonContextCompletionType_CheckedChanged(object sender, EventArgs e)
        {
            if (!IsInitializing)
                VSGeneroPackage.Instance.IntellisenseOptions4GL.SetPendingValue(Genero4GLIntellisenseOptions.CompletionAnalysisTypeSetting, CompletionAnalysisType.Context);
        }

        private void buttonReloadContexts_Click(object sender, EventArgs e)
        {
            bool success = Genero4glAst.ReloadContextMap();
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
                _timer.Dispose();
                _timer = null;
            }
            if (labelReloadResult.Visible)
                labelReloadResult.Visible = false;
            if (labelDownloadResult.Visible)
                labelDownloadResult.Visible = false;
        }

        private void buttonDownloadLatest_Click(object sender, EventArgs e)
        {
            bool success = Genero4glAst.ReloadContextMap(true);
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
