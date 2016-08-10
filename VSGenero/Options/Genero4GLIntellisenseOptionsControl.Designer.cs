namespace VSGenero.Options
{
    partial class Genero4GLIntellisenseOptionsControl
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.checkBoxNewLineOnFullyTypedWord = new System.Windows.Forms.CheckBox();
            this.checkBoxSpacebarCommits = new System.Windows.Forms.CheckBox();
            this.textBoxCommitChars = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.checkBoxPreselectMRU = new System.Windows.Forms.CheckBox();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.labelDownloadResult = new System.Windows.Forms.Label();
            this.labelReloadResult = new System.Windows.Forms.Label();
            this.buttonDownloadLatest = new System.Windows.Forms.Button();
            this.buttonReloadContexts = new System.Windows.Forms.Button();
            this.radioButtonContextCompletionType = new System.Windows.Forms.RadioButton();
            this.radioButtonNormalCompletionType = new System.Windows.Forms.RadioButton();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.toolTip2 = new System.Windows.Forms.ToolTip(this.components);
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox2
            // 
            this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox2.Controls.Add(this.checkBoxNewLineOnFullyTypedWord);
            this.groupBox2.Controls.Add(this.checkBoxSpacebarCommits);
            this.groupBox2.Controls.Add(this.textBoxCommitChars);
            this.groupBox2.Controls.Add(this.label1);
            this.groupBox2.Location = new System.Drawing.Point(4, 127);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(473, 99);
            this.groupBox2.TabIndex = 1;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Selection In Completion List";
            // 
            // checkBoxNewLineOnFullyTypedWord
            // 
            this.checkBoxNewLineOnFullyTypedWord.AutoSize = true;
            this.checkBoxNewLineOnFullyTypedWord.Location = new System.Drawing.Point(20, 78);
            this.checkBoxNewLineOnFullyTypedWord.Name = "checkBoxNewLineOnFullyTypedWord";
            this.checkBoxNewLineOnFullyTypedWord.Size = new System.Drawing.Size(250, 17);
            this.checkBoxNewLineOnFullyTypedWord.TabIndex = 4;
            this.checkBoxNewLineOnFullyTypedWord.Text = "Add new line on enter at end of fully typed word";
            this.checkBoxNewLineOnFullyTypedWord.UseVisualStyleBackColor = true;
            this.checkBoxNewLineOnFullyTypedWord.CheckedChanged += new System.EventHandler(this.checkBoxNewLineOnFullyTypedWord_CheckedChanged);
            // 
            // checkBoxSpacebarCommits
            // 
            this.checkBoxSpacebarCommits.AutoSize = true;
            this.checkBoxSpacebarCommits.Location = new System.Drawing.Point(20, 60);
            this.checkBoxSpacebarCommits.Name = "checkBoxSpacebarCommits";
            this.checkBoxSpacebarCommits.Size = new System.Drawing.Size(199, 17);
            this.checkBoxSpacebarCommits.TabIndex = 2;
            this.checkBoxSpacebarCommits.Text = "Committed by pressing the space bar";
            this.checkBoxSpacebarCommits.UseVisualStyleBackColor = true;
            this.checkBoxSpacebarCommits.CheckedChanged += new System.EventHandler(this.checkBoxSpacebarCommits_CheckedChanged);
            // 
            // textBoxCommitChars
            // 
            this.textBoxCommitChars.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxCommitChars.Location = new System.Drawing.Point(20, 33);
            this.textBoxCommitChars.Name = "textBoxCommitChars";
            this.textBoxCommitChars.Size = new System.Drawing.Size(447, 20);
            this.textBoxCommitChars.TabIndex = 1;
            this.textBoxCommitChars.TextChanged += new System.EventHandler(this.textBoxCommitChars_TextChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(17, 16);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(219, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Committed by typing the following characters:";
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.checkBoxPreselectMRU);
            this.groupBox3.Location = new System.Drawing.Point(4, 230);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(473, 47);
            this.groupBox3.TabIndex = 2;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Intellisense Member Selection";
            // 
            // checkBoxPreselectMRU
            // 
            this.checkBoxPreselectMRU.AutoSize = true;
            this.checkBoxPreselectMRU.Location = new System.Drawing.Point(20, 20);
            this.checkBoxPreselectMRU.Name = "checkBoxPreselectMRU";
            this.checkBoxPreselectMRU.Size = new System.Drawing.Size(204, 17);
            this.checkBoxPreselectMRU.TabIndex = 0;
            this.checkBoxPreselectMRU.Text = "Pre-select most recently used member";
            this.checkBoxPreselectMRU.UseVisualStyleBackColor = true;
            this.checkBoxPreselectMRU.CheckedChanged += new System.EventHandler(this.checkBoxPreselectMRU_CheckedChanged);
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.labelDownloadResult);
            this.groupBox4.Controls.Add(this.labelReloadResult);
            this.groupBox4.Controls.Add(this.buttonDownloadLatest);
            this.groupBox4.Controls.Add(this.buttonReloadContexts);
            this.groupBox4.Controls.Add(this.radioButtonContextCompletionType);
            this.groupBox4.Controls.Add(this.radioButtonNormalCompletionType);
            this.groupBox4.Location = new System.Drawing.Point(4, 4);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(473, 118);
            this.groupBox4.TabIndex = 3;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Completion Type";
            // 
            // labelDownloadResult
            // 
            this.labelDownloadResult.AutoSize = true;
            this.labelDownloadResult.Location = new System.Drawing.Point(207, 92);
            this.labelDownloadResult.Name = "labelDownloadResult";
            this.labelDownloadResult.Size = new System.Drawing.Size(83, 13);
            this.labelDownloadResult.TabIndex = 5;
            this.labelDownloadResult.Text = "Download result";
            this.labelDownloadResult.Visible = false;
            // 
            // labelReloadResult
            // 
            this.labelReloadResult.AutoSize = true;
            this.labelReloadResult.Location = new System.Drawing.Point(206, 64);
            this.labelReloadResult.Name = "labelReloadResult";
            this.labelReloadResult.Size = new System.Drawing.Size(69, 13);
            this.labelReloadResult.TabIndex = 4;
            this.labelReloadResult.Text = "Reload result";
            this.labelReloadResult.Visible = false;
            // 
            // buttonDownloadLatest
            // 
            this.buttonDownloadLatest.Location = new System.Drawing.Point(43, 87);
            this.buttonDownloadLatest.Name = "buttonDownloadLatest";
            this.buttonDownloadLatest.Size = new System.Drawing.Size(158, 23);
            this.buttonDownloadLatest.TabIndex = 3;
            this.buttonDownloadLatest.Text = "Download latest contexts";
            this.toolTip1.SetToolTip(this.buttonDownloadLatest, "Load latest changes to the Completion Context XML map");
            this.toolTip2.SetToolTip(this.buttonDownloadLatest, "Download latest context changes from Github");
            this.buttonDownloadLatest.UseVisualStyleBackColor = true;
            this.buttonDownloadLatest.Click += new System.EventHandler(this.buttonDownloadLatest_Click);
            // 
            // buttonReloadContexts
            // 
            this.buttonReloadContexts.Location = new System.Drawing.Point(43, 59);
            this.buttonReloadContexts.Name = "buttonReloadContexts";
            this.buttonReloadContexts.Size = new System.Drawing.Size(158, 23);
            this.buttonReloadContexts.TabIndex = 2;
            this.buttonReloadContexts.Text = "Reload completion contexts";
            this.toolTip1.SetToolTip(this.buttonReloadContexts, "Load latest changes to the Completion Context XML map");
            this.buttonReloadContexts.UseVisualStyleBackColor = true;
            this.buttonReloadContexts.Click += new System.EventHandler(this.buttonReloadContexts_Click);
            // 
            // radioButtonContextCompletionType
            // 
            this.radioButtonContextCompletionType.AutoSize = true;
            this.radioButtonContextCompletionType.Location = new System.Drawing.Point(20, 39);
            this.radioButtonContextCompletionType.Name = "radioButtonContextCompletionType";
            this.radioButtonContextCompletionType.Size = new System.Drawing.Size(162, 17);
            this.radioButtonContextCompletionType.TabIndex = 1;
            this.radioButtonContextCompletionType.TabStop = true;
            this.radioButtonContextCompletionType.Text = "Context-Aware (experimental)";
            this.radioButtonContextCompletionType.UseVisualStyleBackColor = true;
            this.radioButtonContextCompletionType.CheckedChanged += new System.EventHandler(this.radioButtonContextCompletionType_CheckedChanged);
            // 
            // radioButtonNormalCompletionType
            // 
            this.radioButtonNormalCompletionType.AutoSize = true;
            this.radioButtonNormalCompletionType.Location = new System.Drawing.Point(20, 19);
            this.radioButtonNormalCompletionType.Name = "radioButtonNormalCompletionType";
            this.radioButtonNormalCompletionType.Size = new System.Drawing.Size(234, 17);
            this.radioButtonNormalCompletionType.TabIndex = 0;
            this.radioButtonNormalCompletionType.TabStop = true;
            this.radioButtonNormalCompletionType.Text = "Normal (include all keywords, members, etc.)";
            this.radioButtonNormalCompletionType.UseVisualStyleBackColor = true;
            this.radioButtonNormalCompletionType.CheckedChanged += new System.EventHandler(this.radioButtonNormalCompletionType_CheckedChanged);
            // 
            // Genero4GLIntellisenseOptionsControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.groupBox4);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox2);
            this.Name = "Genero4GLIntellisenseOptionsControl";
            this.Size = new System.Drawing.Size(480, 327);
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBoxCommitChars;
        private System.Windows.Forms.CheckBox checkBoxSpacebarCommits;
        private System.Windows.Forms.CheckBox checkBoxNewLineOnFullyTypedWord;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.CheckBox checkBoxPreselectMRU;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.RadioButton radioButtonContextCompletionType;
        private System.Windows.Forms.RadioButton radioButtonNormalCompletionType;
        private System.Windows.Forms.Button buttonReloadContexts;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.Button buttonDownloadLatest;
        private System.Windows.Forms.ToolTip toolTip2;
        private System.Windows.Forms.Label labelDownloadResult;
        private System.Windows.Forms.Label labelReloadResult;
    }
}
