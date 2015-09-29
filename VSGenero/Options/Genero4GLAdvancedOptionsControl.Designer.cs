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
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.checkBoxSemanticErrorChecking = new System.Windows.Forms.CheckBox();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.checkBoxMajorCollapseRegions = new System.Windows.Forms.CheckBox();
            this.checkBoxMinorCollapseRegions = new System.Windows.Forms.CheckBox();
            this.checkBoxCustomCollapseRegions = new System.Windows.Forms.CheckBox();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
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
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.checkBoxSemanticErrorChecking);
            this.groupBox2.Location = new System.Drawing.Point(4, 147);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(473, 46);
            this.groupBox2.TabIndex = 1;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Error Checking";
            // 
            // checkBoxSemanticErrorChecking
            // 
            this.checkBoxSemanticErrorChecking.AutoSize = true;
            this.checkBoxSemanticErrorChecking.Location = new System.Drawing.Point(16, 20);
            this.checkBoxSemanticErrorChecking.Name = "checkBoxSemanticErrorChecking";
            this.checkBoxSemanticErrorChecking.Size = new System.Drawing.Size(246, 17);
            this.checkBoxSemanticErrorChecking.TabIndex = 0;
            this.checkBoxSemanticErrorChecking.Text = "Enable Semantic Error checking (experimental)";
            this.checkBoxSemanticErrorChecking.UseVisualStyleBackColor = true;
            this.checkBoxSemanticErrorChecking.CheckedChanged += new System.EventHandler(this.checkBoxSemanticErrorChecking_CheckedChanged);
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.checkBoxCustomCollapseRegions);
            this.groupBox3.Controls.Add(this.checkBoxMinorCollapseRegions);
            this.groupBox3.Controls.Add(this.checkBoxMajorCollapseRegions);
            this.groupBox3.Location = new System.Drawing.Point(4, 52);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(473, 89);
            this.groupBox3.TabIndex = 2;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Code Collapse/Expand";
            // 
            // checkBoxMajorCollapseRegions
            // 
            this.checkBoxMajorCollapseRegions.AutoSize = true;
            this.checkBoxMajorCollapseRegions.Location = new System.Drawing.Point(16, 20);
            this.checkBoxMajorCollapseRegions.Name = "checkBoxMajorCollapseRegions";
            this.checkBoxMajorCollapseRegions.Size = new System.Drawing.Size(319, 17);
            this.checkBoxMajorCollapseRegions.TabIndex = 0;
            this.checkBoxMajorCollapseRegions.Text = "Enable major collapse regions (functions, reports, globals, etc.)";
            this.checkBoxMajorCollapseRegions.UseVisualStyleBackColor = true;
            this.checkBoxMajorCollapseRegions.CheckedChanged += new System.EventHandler(this.checkBoxMajorCollapseRegions_CheckedChanged);
            // 
            // checkBoxMinorCollapseRegions
            // 
            this.checkBoxMinorCollapseRegions.AutoSize = true;
            this.checkBoxMinorCollapseRegions.Location = new System.Drawing.Point(16, 43);
            this.checkBoxMinorCollapseRegions.Name = "checkBoxMinorCollapseRegions";
            this.checkBoxMinorCollapseRegions.Size = new System.Drawing.Size(386, 17);
            this.checkBoxMinorCollapseRegions.TabIndex = 1;
            this.checkBoxMinorCollapseRegions.Text = "Enable minor collapse regions (code blocks, such as if, for, while statements)";
            this.checkBoxMinorCollapseRegions.UseVisualStyleBackColor = true;
            this.checkBoxMinorCollapseRegions.CheckedChanged += new System.EventHandler(this.checkBoxMinorCollapseRegions_CheckedChanged);
            // 
            // checkBoxCustomCollapseRegions
            // 
            this.checkBoxCustomCollapseRegions.AutoSize = true;
            this.checkBoxCustomCollapseRegions.Location = new System.Drawing.Point(16, 66);
            this.checkBoxCustomCollapseRegions.Name = "checkBoxCustomCollapseRegions";
            this.checkBoxCustomCollapseRegions.Size = new System.Drawing.Size(245, 17);
            this.checkBoxCustomCollapseRegions.TabIndex = 2;
            this.checkBoxCustomCollapseRegions.Text = "Enable C#-like \"#region/#endregion\" sections";
            this.checkBoxCustomCollapseRegions.UseVisualStyleBackColor = true;
            this.checkBoxCustomCollapseRegions.CheckedChanged += new System.EventHandler(this.checkBoxCustomCollapseRegions_CheckedChanged);
            // 
            // Genero4GLAdvancedOptionsControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Name = "Genero4GLAdvancedOptionsControl";
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
        private System.Windows.Forms.CheckBox checkBoxShowFunctionParams;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.CheckBox checkBoxSemanticErrorChecking;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.CheckBox checkBoxCustomCollapseRegions;
        private System.Windows.Forms.CheckBox checkBoxMinorCollapseRegions;
        private System.Windows.Forms.CheckBox checkBoxMajorCollapseRegions;
    }
}
