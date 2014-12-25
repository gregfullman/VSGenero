namespace Microsoft.VisualStudio.VSCommon.Utilities
{
    public partial class ExceptionForm
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabGeneralInfo = new System.Windows.Forms.TabPage();
            this.textBoxGeneralTargetMethod = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.textBoxGeneralSource = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.textBoxGeneralMessage = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.tabStackTrace = new System.Windows.Forms.TabPage();
            this.listViewStackTrace = new System.Windows.Forms.ListView();
            this.tabInnerException = new System.Windows.Forms.TabPage();
            this.treeViewInnerException = new System.Windows.Forms.TreeView();
            this.tabOtherInfo = new System.Windows.Forms.TabPage();
            this.listViewOtherInfo = new System.Windows.Forms.ListView();
            this.columnHeaderName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderDesc = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.label1 = new System.Windows.Forms.Label();
            this.textBoxExceptionType = new System.Windows.Forms.TextBox();
            this.buttonCopy = new System.Windows.Forms.Button();
            this.buttonOK = new System.Windows.Forms.Button();
            this.tabControl1.SuspendLayout();
            this.tabGeneralInfo.SuspendLayout();
            this.tabStackTrace.SuspendLayout();
            this.tabInnerException.SuspendLayout();
            this.tabOtherInfo.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControl1
            // 
            this.tabControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl1.Controls.Add(this.tabGeneralInfo);
            this.tabControl1.Controls.Add(this.tabStackTrace);
            this.tabControl1.Controls.Add(this.tabInnerException);
            this.tabControl1.Controls.Add(this.tabOtherInfo);
            this.tabControl1.Location = new System.Drawing.Point(16, 39);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(496, 200);
            this.tabControl1.TabIndex = 0;
            this.tabControl1.SelectedIndexChanged += new System.EventHandler(this.tabControl1_SelectedIndexChanged);
            // 
            // tabGeneralInfo
            // 
            this.tabGeneralInfo.Controls.Add(this.textBoxGeneralTargetMethod);
            this.tabGeneralInfo.Controls.Add(this.label4);
            this.tabGeneralInfo.Controls.Add(this.textBoxGeneralSource);
            this.tabGeneralInfo.Controls.Add(this.label3);
            this.tabGeneralInfo.Controls.Add(this.textBoxGeneralMessage);
            this.tabGeneralInfo.Controls.Add(this.label2);
            this.tabGeneralInfo.Location = new System.Drawing.Point(4, 22);
            this.tabGeneralInfo.Name = "tabGeneralInfo";
            this.tabGeneralInfo.Padding = new System.Windows.Forms.Padding(3);
            this.tabGeneralInfo.Size = new System.Drawing.Size(488, 174);
            this.tabGeneralInfo.TabIndex = 0;
            this.tabGeneralInfo.Text = "General Information";
            this.tabGeneralInfo.UseVisualStyleBackColor = true;
            // 
            // textBoxGeneralTargetMethod
            // 
            this.textBoxGeneralTargetMethod.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxGeneralTargetMethod.Location = new System.Drawing.Point(97, 146);
            this.textBoxGeneralTargetMethod.Name = "textBoxGeneralTargetMethod";
            this.textBoxGeneralTargetMethod.ReadOnly = true;
            this.textBoxGeneralTargetMethod.Size = new System.Drawing.Size(385, 20);
            this.textBoxGeneralTargetMethod.TabIndex = 5;
            // 
            // label4
            // 
            this.label4.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(6, 149);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(80, 13);
            this.label4.TabIndex = 4;
            this.label4.Text = "Target Method:";
            // 
            // textBoxGeneralSource
            // 
            this.textBoxGeneralSource.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxGeneralSource.Location = new System.Drawing.Point(97, 119);
            this.textBoxGeneralSource.Name = "textBoxGeneralSource";
            this.textBoxGeneralSource.ReadOnly = true;
            this.textBoxGeneralSource.Size = new System.Drawing.Size(385, 20);
            this.textBoxGeneralSource.TabIndex = 3;
            // 
            // label3
            // 
            this.label3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(42, 122);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(44, 13);
            this.label3.TabIndex = 2;
            this.label3.Text = "Source:";
            // 
            // textBoxGeneralMessage
            // 
            this.textBoxGeneralMessage.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxGeneralMessage.Location = new System.Drawing.Point(97, 7);
            this.textBoxGeneralMessage.Multiline = true;
            this.textBoxGeneralMessage.Name = "textBoxGeneralMessage";
            this.textBoxGeneralMessage.ReadOnly = true;
            this.textBoxGeneralMessage.Size = new System.Drawing.Size(385, 106);
            this.textBoxGeneralMessage.TabIndex = 1;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(33, 7);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(53, 13);
            this.label2.TabIndex = 0;
            this.label2.Text = "Message:";
            // 
            // tabStackTrace
            // 
            this.tabStackTrace.Controls.Add(this.listViewStackTrace);
            this.tabStackTrace.Location = new System.Drawing.Point(4, 22);
            this.tabStackTrace.Name = "tabStackTrace";
            this.tabStackTrace.Padding = new System.Windows.Forms.Padding(3);
            this.tabStackTrace.Size = new System.Drawing.Size(488, 174);
            this.tabStackTrace.TabIndex = 1;
            this.tabStackTrace.Text = "Stack Trace";
            this.tabStackTrace.UseVisualStyleBackColor = true;
            // 
            // listViewStackTrace
            // 
            this.listViewStackTrace.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listViewStackTrace.Location = new System.Drawing.Point(7, 4);
            this.listViewStackTrace.Name = "listViewStackTrace";
            this.listViewStackTrace.Size = new System.Drawing.Size(475, 164);
            this.listViewStackTrace.TabIndex = 0;
            this.listViewStackTrace.UseCompatibleStateImageBehavior = false;
            this.listViewStackTrace.View = System.Windows.Forms.View.List;
            // 
            // tabInnerException
            // 
            this.tabInnerException.Controls.Add(this.treeViewInnerException);
            this.tabInnerException.Location = new System.Drawing.Point(4, 22);
            this.tabInnerException.Name = "tabInnerException";
            this.tabInnerException.Size = new System.Drawing.Size(488, 174);
            this.tabInnerException.TabIndex = 2;
            this.tabInnerException.Text = "Inner Exception Trace";
            this.tabInnerException.UseVisualStyleBackColor = true;
            // 
            // treeViewInnerException
            // 
            this.treeViewInnerException.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.treeViewInnerException.Location = new System.Drawing.Point(4, 4);
            this.treeViewInnerException.Name = "treeViewInnerException";
            this.treeViewInnerException.Size = new System.Drawing.Size(481, 167);
            this.treeViewInnerException.TabIndex = 0;
            // 
            // tabOtherInfo
            // 
            this.tabOtherInfo.Controls.Add(this.listViewOtherInfo);
            this.tabOtherInfo.Location = new System.Drawing.Point(4, 22);
            this.tabOtherInfo.Name = "tabOtherInfo";
            this.tabOtherInfo.Size = new System.Drawing.Size(488, 174);
            this.tabOtherInfo.TabIndex = 3;
            this.tabOtherInfo.Text = "Other Information";
            this.tabOtherInfo.UseVisualStyleBackColor = true;
            // 
            // listViewOtherInfo
            // 
            this.listViewOtherInfo.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listViewOtherInfo.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeaderName,
            this.columnHeaderDesc});
            this.listViewOtherInfo.Location = new System.Drawing.Point(4, 4);
            this.listViewOtherInfo.Name = "listViewOtherInfo";
            this.listViewOtherInfo.Size = new System.Drawing.Size(481, 167);
            this.listViewOtherInfo.TabIndex = 0;
            this.listViewOtherInfo.UseCompatibleStateImageBehavior = false;
            this.listViewOtherInfo.View = System.Windows.Forms.View.Details;
            // 
            // columnHeaderName
            // 
            this.columnHeaderName.Text = "Name";
            this.columnHeaderName.Width = 100;
            // 
            // columnHeaderDesc
            // 
            this.columnHeaderDesc.Text = "Description";
            this.columnHeaderDesc.Width = 417;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 16);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(84, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Exception Type:";
            // 
            // textBoxExceptionType
            // 
            this.textBoxExceptionType.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxExceptionType.Location = new System.Drawing.Point(104, 13);
            this.textBoxExceptionType.Name = "textBoxExceptionType";
            this.textBoxExceptionType.ReadOnly = true;
            this.textBoxExceptionType.Size = new System.Drawing.Size(408, 20);
            this.textBoxExceptionType.TabIndex = 2;
            // 
            // buttonCopy
            // 
            this.buttonCopy.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonCopy.Location = new System.Drawing.Point(19, 246);
            this.buttonCopy.Name = "buttonCopy";
            this.buttonCopy.Size = new System.Drawing.Size(75, 23);
            this.buttonCopy.TabIndex = 3;
            this.buttonCopy.Text = "Save Info";
            this.buttonCopy.UseVisualStyleBackColor = true;
            this.buttonCopy.Click += new System.EventHandler(this.buttonSave_Click);
            // 
            // buttonOK
            // 
            this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOK.Location = new System.Drawing.Point(432, 246);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(75, 23);
            this.buttonOK.TabIndex = 4;
            this.buttonOK.Text = "OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            // 
            // ExceptionForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(524, 280);
            this.Controls.Add(this.buttonOK);
            this.Controls.Add(this.buttonCopy);
            this.Controls.Add(this.textBoxExceptionType);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.tabControl1);
            this.Name = "ExceptionForm";
            this.Text = "Exception Information";
            this.Load += new System.EventHandler(this.ExceptionForm_Load);
            this.tabControl1.ResumeLayout(false);
            this.tabGeneralInfo.ResumeLayout(false);
            this.tabGeneralInfo.PerformLayout();
            this.tabStackTrace.ResumeLayout(false);
            this.tabInnerException.ResumeLayout(false);
            this.tabOtherInfo.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabGeneralInfo;
        private System.Windows.Forms.TabPage tabStackTrace;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBoxExceptionType;
        private System.Windows.Forms.Button buttonCopy;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.TabPage tabInnerException;
        private System.Windows.Forms.TabPage tabOtherInfo;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textBoxGeneralTargetMethod;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox textBoxGeneralSource;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox textBoxGeneralMessage;
        private System.Windows.Forms.TreeView treeViewInnerException;
        private System.Windows.Forms.ListView listViewOtherInfo;
        private System.Windows.Forms.ColumnHeader columnHeaderName;
        private System.Windows.Forms.ColumnHeader columnHeaderDesc;
        private System.Windows.Forms.ListView listViewStackTrace;
    }
}