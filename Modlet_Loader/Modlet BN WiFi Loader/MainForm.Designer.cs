using System;
using System.Reflection;

namespace ThinkEco
{
    partial class MainForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.selectMode = new System.Windows.Forms.ComboBox();
            this.labelPASS = new System.Windows.Forms.Label();
            this.labelFAIL = new System.Windows.Forms.Label();
            this.mainLabel = new System.Windows.Forms.TextBox();
            this.userInfo = new System.Windows.Forms.Label();
            this.Label_Scan2 = new System.Windows.Forms.Label();
            this.Label_Scan1 = new System.Windows.Forms.Label();
            this.wifiLabel = new System.Windows.Forms.TextBox();
            this.Label_SelectOpr = new System.Windows.Forms.Label();
            this.progressBar = new System.Windows.Forms.ProgressBar();
            this.SuspendLayout();
            // 
            // selectMode
            // 
            this.selectMode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.selectMode.Items.AddRange(new object[] {
            "Load Firmware",
            "Erase Firmware"});
            this.selectMode.Location = new System.Drawing.Point(15, 39);
            this.selectMode.Name = "selectMode";
            this.selectMode.Size = new System.Drawing.Size(121, 21);
            this.selectMode.TabIndex = 14;
            this.selectMode.SelectedIndexChanged += new System.EventHandler(this.selectModeChanged);
            // 
            // labelPASS
            // 
            this.labelPASS.AutoSize = true;
            this.labelPASS.BackColor = System.Drawing.Color.LimeGreen;
            this.labelPASS.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.labelPASS.Font = new System.Drawing.Font("Calibri", 18F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelPASS.Location = new System.Drawing.Point(15, 307);
            this.labelPASS.MinimumSize = new System.Drawing.Size(130, 100);
            this.labelPASS.Name = "labelPASS";
            this.labelPASS.Size = new System.Drawing.Size(130, 100);
            this.labelPASS.TabIndex = 12;
            this.labelPASS.Text = "PASS";
            this.labelPASS.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // labelFAIL
            // 
            this.labelFAIL.AutoSize = true;
            this.labelFAIL.BackColor = System.Drawing.Color.Red;
            this.labelFAIL.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.labelFAIL.Font = new System.Drawing.Font("Calibri", 18F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelFAIL.Location = new System.Drawing.Point(205, 307);
            this.labelFAIL.MinimumSize = new System.Drawing.Size(130, 100);
            this.labelFAIL.Name = "labelFAIL";
            this.labelFAIL.Size = new System.Drawing.Size(130, 100);
            this.labelFAIL.TabIndex = 11;
            this.labelFAIL.Text = "FAIL";
            this.labelFAIL.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // mainLabel
            // 
            this.mainLabel.Location = new System.Drawing.Point(15, 95);
            this.mainLabel.Name = "mainLabel";
            this.mainLabel.Size = new System.Drawing.Size(320, 20);
            this.mainLabel.TabIndex = 9;
            this.mainLabel.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.mainLabelHandler);
            // 
            // userInfo
            // 
            this.userInfo.BackColor = System.Drawing.SystemColors.Window;
            this.userInfo.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.userInfo.Font = new System.Drawing.Font("Calibri", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.userInfo.Location = new System.Drawing.Point(15, 199);
            this.userInfo.MinimumSize = new System.Drawing.Size(320, 50);
            this.userInfo.Name = "userInfo";
            this.userInfo.Size = new System.Drawing.Size(320, 85);
            this.userInfo.TabIndex = 8;
            this.userInfo.Text = "User Information";
            // 
            // Label_Scan2
            // 
            this.Label_Scan2.AutoSize = true;
            this.Label_Scan2.Location = new System.Drawing.Point(12, 127);
            this.Label_Scan2.Name = "Label_Scan2";
            this.Label_Scan2.Size = new System.Drawing.Size(57, 13);
            this.Label_Scan2.TabIndex = 18;
            this.Label_Scan2.Text = "WiFi Label";
            // 
            // Label_Scan1
            // 
            this.Label_Scan1.AutoSize = true;
            this.Label_Scan1.Location = new System.Drawing.Point(12, 79);
            this.Label_Scan1.Name = "Label_Scan1";
            this.Label_Scan1.Size = new System.Drawing.Size(59, 13);
            this.Label_Scan1.TabIndex = 17;
            this.Label_Scan1.Text = "Main Label";
            // 
            // wifiLabel
            // 
            this.wifiLabel.Location = new System.Drawing.Point(15, 143);
            this.wifiLabel.Name = "wifiLabel";
            this.wifiLabel.Size = new System.Drawing.Size(320, 20);
            this.wifiLabel.TabIndex = 16;
            this.wifiLabel.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.wifiLabelHandler);
            // 
            // Label_SelectOpr
            // 
            this.Label_SelectOpr.AutoSize = true;
            this.Label_SelectOpr.Location = new System.Drawing.Point(12, 13);
            this.Label_SelectOpr.Name = "Label_SelectOpr";
            this.Label_SelectOpr.Size = new System.Drawing.Size(86, 13);
            this.Label_SelectOpr.TabIndex = 15;
            this.Label_SelectOpr.Text = "Select Operation";
            // 
            // progressBar
            // 
            this.progressBar.BackColor = System.Drawing.Color.WhiteSmoke;
            this.progressBar.ForeColor = System.Drawing.Color.RoyalBlue;
            this.progressBar.Location = new System.Drawing.Point(15, 178);
            this.progressBar.MinimumSize = new System.Drawing.Size(300, 10);
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new System.Drawing.Size(320, 18);
            this.progressBar.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
            this.progressBar.TabIndex = 10;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(347, 416);
            this.Controls.Add(this.Label_SelectOpr);
            this.Controls.Add(this.Label_Scan1);
            this.Controls.Add(this.selectMode);
            this.Controls.Add(this.Label_Scan2);
            this.Controls.Add(this.labelFAIL);
            this.Controls.Add(this.mainLabel);
            this.Controls.Add(this.labelPASS);
            this.Controls.Add(this.wifiLabel);
            this.Controls.Add(this.userInfo);
            this.Controls.Add(this.progressBar);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Modlet BN WiFi Loader " + Convert.ToString(Assembly.GetExecutingAssembly().GetName().Version.Major) + "." + Convert.ToString(Assembly.GetExecutingAssembly().GetName().Version.Minor);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox selectMode;
        private System.Windows.Forms.Label labelPASS;
        private System.Windows.Forms.Label labelFAIL;
        private System.Windows.Forms.TextBox mainLabel;
        private System.Windows.Forms.Label userInfo;
        private System.Windows.Forms.Label Label_SelectOpr;
        private System.Windows.Forms.Label Label_Scan2;
        private System.Windows.Forms.Label Label_Scan1;
        private System.Windows.Forms.TextBox wifiLabel;
        private System.Windows.Forms.ProgressBar progressBar;

    }
}

