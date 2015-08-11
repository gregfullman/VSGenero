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
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.checkBoxSpaceTriggersCompletionList = new System.Windows.Forms.CheckBox();
            this.checkBoxCompletionList = new System.Windows.Forms.CheckBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.checkBoxNewLineOnFullyTypedWord = new System.Windows.Forms.CheckBox();
            this.checkBoxEnterCommits = new System.Windows.Forms.CheckBox();
            this.checkBoxSpacebarCommits = new System.Windows.Forms.CheckBox();
            this.textBoxCommitChars = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.checkBoxPreselectMRU = new System.Windows.Forms.CheckBox();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.checkBoxSpaceTriggersCompletionList);
            this.groupBox1.Controls.Add(this.checkBoxCompletionList);
            this.groupBox1.Location = new System.Drawing.Point(4, 4);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(473, 70);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Completion Lists";
            // 
            // checkBoxSpaceTriggersCompletionList
            // 
            this.checkBoxSpaceTriggersCompletionList.AutoSize = true;
            this.checkBoxSpaceTriggersCompletionList.Location = new System.Drawing.Point(48, 42);
            this.checkBoxSpaceTriggersCompletionList.Name = "checkBoxSpaceTriggersCompletionList";
            this.checkBoxSpaceTriggersCompletionList.Size = new System.Drawing.Size(192, 17);
            this.checkBoxSpaceTriggersCompletionList.TabIndex = 1;
            this.checkBoxSpaceTriggersCompletionList.Text = "Allow space to show completion list";
            this.checkBoxSpaceTriggersCompletionList.UseVisualStyleBackColor = true;
            this.checkBoxSpaceTriggersCompletionList.CheckedChanged += new System.EventHandler(this.checkBoxSpaceTriggersCompletionList_CheckedChanged);
            // 
            // checkBoxCompletionList
            // 
            this.checkBoxCompletionList.AutoSize = true;
            this.checkBoxCompletionList.Location = new System.Drawing.Point(20, 19);
            this.checkBoxCompletionList.Name = "checkBoxCompletionList";
            this.checkBoxCompletionList.Size = new System.Drawing.Size(242, 17);
            this.checkBoxCompletionList.TabIndex = 0;
            this.checkBoxCompletionList.Text = "Show completion list after a character is typed";
            this.checkBoxCompletionList.UseVisualStyleBackColor = true;
            this.checkBoxCompletionList.CheckedChanged += new System.EventHandler(this.checkBoxCompletionList_CheckedChanged);
            // 
            // groupBox2
            // 
            this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox2.Controls.Add(this.checkBoxNewLineOnFullyTypedWord);
            this.groupBox2.Controls.Add(this.checkBoxEnterCommits);
            this.groupBox2.Controls.Add(this.checkBoxSpacebarCommits);
            this.groupBox2.Controls.Add(this.textBoxCommitChars);
            this.groupBox2.Controls.Add(this.label1);
            this.groupBox2.Location = new System.Drawing.Point(4, 80);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(473, 134);
            this.groupBox2.TabIndex = 1;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Selection In Completion List";
            // 
            // checkBoxNewLineOnFullyTypedWord
            // 
            this.checkBoxNewLineOnFullyTypedWord.AutoSize = true;
            this.checkBoxNewLineOnFullyTypedWord.Location = new System.Drawing.Point(20, 108);
            this.checkBoxNewLineOnFullyTypedWord.Name = "checkBoxNewLineOnFullyTypedWord";
            this.checkBoxNewLineOnFullyTypedWord.Size = new System.Drawing.Size(250, 17);
            this.checkBoxNewLineOnFullyTypedWord.TabIndex = 4;
            this.checkBoxNewLineOnFullyTypedWord.Text = "Add new line on enter at end of fully typed word";
            this.checkBoxNewLineOnFullyTypedWord.UseVisualStyleBackColor = true;
            this.checkBoxNewLineOnFullyTypedWord.CheckedChanged += new System.EventHandler(this.checkBoxNewLineOnFullyTypedWord_CheckedChanged);
            // 
            // checkBoxEnterCommits
            // 
            this.checkBoxEnterCommits.AutoSize = true;
            this.checkBoxEnterCommits.Location = new System.Drawing.Point(20, 84);
            this.checkBoxEnterCommits.Name = "checkBoxEnterCommits";
            this.checkBoxEnterCommits.Size = new System.Drawing.Size(158, 17);
            this.checkBoxEnterCommits.TabIndex = 3;
            this.checkBoxEnterCommits.Text = "Committed by pressing enter";
            this.checkBoxEnterCommits.UseVisualStyleBackColor = true;
            this.checkBoxEnterCommits.CheckedChanged += new System.EventHandler(this.checkBoxEnterCommits_CheckedChanged);
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
            this.groupBox3.Location = new System.Drawing.Point(4, 220);
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
            // Genero4GLIntellisenseOptionsControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Name = "Genero4GLIntellisenseOptionsControl";
            this.Size = new System.Drawing.Size(480, 270);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.CheckBox checkBoxCompletionList;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBoxCommitChars;
        private System.Windows.Forms.CheckBox checkBoxSpacebarCommits;
        private System.Windows.Forms.CheckBox checkBoxNewLineOnFullyTypedWord;
        private System.Windows.Forms.CheckBox checkBoxEnterCommits;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.CheckBox checkBoxPreselectMRU;
        private System.Windows.Forms.CheckBox checkBoxSpaceTriggersCompletionList;
    }
}
