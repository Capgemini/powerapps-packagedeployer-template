namespace Capgemini.Xrm.PackageDeployer.TestUI
{
    partial class TemplateMigration
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
            this.btExport = new System.Windows.Forms.Button();
            this.btImport = new System.Windows.Forms.Button();
            this.tbSourceConnection = new System.Windows.Forms.TextBox();
            this.tbTargetConnection = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.tbTemplateName = new System.Windows.Forms.TextBox();
            this.tbFilePath = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.tbSurcePath = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.tbTargetTemplateName = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // btExport
            // 
            this.btExport.Location = new System.Drawing.Point(22, 19);
            this.btExport.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.btExport.Name = "btExport";
            this.btExport.Size = new System.Drawing.Size(120, 32);
            this.btExport.TabIndex = 0;
            this.btExport.Text = "Export Template";
            this.btExport.UseVisualStyleBackColor = true;
            this.btExport.Click += new System.EventHandler(this.btExport_Click);
            // 
            // btImport
            // 
            this.btImport.Location = new System.Drawing.Point(22, 154);
            this.btImport.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.btImport.Name = "btImport";
            this.btImport.Size = new System.Drawing.Size(120, 30);
            this.btImport.TabIndex = 1;
            this.btImport.Text = "Import Tmeaplate";
            this.btImport.UseVisualStyleBackColor = true;
            this.btImport.Click += new System.EventHandler(this.btImport_Click);
            // 
            // tbSourceConnection
            // 
            this.tbSourceConnection.Location = new System.Drawing.Point(194, 58);
            this.tbSourceConnection.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.tbSourceConnection.Name = "tbSourceConnection";
            this.tbSourceConnection.Size = new System.Drawing.Size(662, 20);
            this.tbSourceConnection.TabIndex = 2;
            // 
            // tbTargetConnection
            // 
            this.tbTargetConnection.Location = new System.Drawing.Point(194, 198);
            this.tbTargetConnection.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.tbTargetConnection.Name = "tbTargetConnection";
            this.tbTargetConnection.Size = new System.Drawing.Size(662, 20);
            this.tbTargetConnection.TabIndex = 3;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(26, 58);
            this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(128, 13);
            this.label1.TabIndex = 4;
            this.label1.Text = "Source Connection String";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(26, 81);
            this.label2.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(82, 13);
            this.label2.TabIndex = 5;
            this.label2.Text = "Template Name";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(28, 198);
            this.label3.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(125, 13);
            this.label3.TabIndex = 7;
            this.label3.Text = "Target Connection String";
            // 
            // tbTemplateName
            // 
            this.tbTemplateName.Location = new System.Drawing.Point(194, 81);
            this.tbTemplateName.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.tbTemplateName.Name = "tbTemplateName";
            this.tbTemplateName.Size = new System.Drawing.Size(662, 20);
            this.tbTemplateName.TabIndex = 6;
            this.tbTemplateName.Text = "test";
            // 
            // tbFilePath
            // 
            this.tbFilePath.Location = new System.Drawing.Point(194, 105);
            this.tbFilePath.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.tbFilePath.Name = "tbFilePath";
            this.tbFilePath.Size = new System.Drawing.Size(662, 20);
            this.tbFilePath.TabIndex = 9;
            this.tbFilePath.Text = "test.docx";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(26, 105);
            this.label4.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(48, 13);
            this.label4.TabIndex = 8;
            this.label4.Text = "File Path";
            // 
            // tbSurcePath
            // 
            this.tbSurcePath.Location = new System.Drawing.Point(194, 247);
            this.tbSurcePath.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.tbSurcePath.Name = "tbSurcePath";
            this.tbSurcePath.Size = new System.Drawing.Size(662, 20);
            this.tbSurcePath.TabIndex = 13;
            this.tbSurcePath.Text = "test.docx";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(28, 247);
            this.label5.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(48, 13);
            this.label5.TabIndex = 12;
            this.label5.Text = "File Path";
            // 
            // tbTargetTemplateName
            // 
            this.tbTargetTemplateName.Location = new System.Drawing.Point(194, 223);
            this.tbTargetTemplateName.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.tbTargetTemplateName.Name = "tbTargetTemplateName";
            this.tbTargetTemplateName.Size = new System.Drawing.Size(662, 20);
            this.tbTargetTemplateName.TabIndex = 11;
            this.tbTargetTemplateName.Text = "test";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(26, 223);
            this.label6.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(82, 13);
            this.label6.TabIndex = 10;
            this.label6.Text = "Template Name";
            // 
            // TemplateMigration
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(875, 287);
            this.Controls.Add(this.tbSurcePath);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.tbTargetTemplateName);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.tbFilePath);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.tbTemplateName);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.tbTargetConnection);
            this.Controls.Add(this.tbSourceConnection);
            this.Controls.Add(this.btImport);
            this.Controls.Add(this.btExport);
            this.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.Name = "TemplateMigration";
            this.Text = "TemplateMigration";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btExport;
        private System.Windows.Forms.Button btImport;
        private System.Windows.Forms.TextBox tbSourceConnection;
        private System.Windows.Forms.TextBox tbTargetConnection;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox tbTemplateName;
        private System.Windows.Forms.TextBox tbFilePath;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox tbSurcePath;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox tbTargetTemplateName;
        private System.Windows.Forms.Label label6;
    }
}