namespace VSGenero.Options
{
    partial class Genero4GLAdvancedOptionsControl
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
            this.checkBoxShowFunctionParams = new System.Windows.Forms.CheckBox();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.checkBoxShowFunctionParams);
            this.groupBox1.Location = new System.Drawing.Point(4, 4);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(473, 46);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Navigation Bars";
            // 
            // checkBoxShowFunctionParams
            // 
            this.checkBoxShowFunctionParams.AutoSize = true;
            this.checkBoxShowFunctionParams.Location = new System.Drawing.Point(16, 20);
            this.checkBoxShowFunctionParams.Name = "checkBoxShowFunctionParams";
            this.checkBoxShowFunctionParams.Size = new System.Drawing.Size(216, 17);
            this.checkBoxShowFunctionParams.TabIndex = 0;
            this.checkBoxShowFunctionParams.Text = "Show function parameters in function list";
            this.checkBoxShowFunctionParams.UseVisualStyleBackColor = true;
            this.checkBoxShowFunctionParams.CheckedChanged += new System.EventHandler(this.checkBox1_CheckedChanged);
            // 
            // Genero4GLAdvancedOptionsControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.groupBox1);
            this.Name = "Genero4GLAdvancedOptionsControl";
            this.Size = new System.Drawing.Size(480, 270);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.CheckBox checkBoxShowFunctionParams;
    }
}
