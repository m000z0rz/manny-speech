﻿namespace Manny
{
    partial class frmMain
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmMain));
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.button3 = new System.Windows.Forms.Button();
            this.txtSirRate = new System.Windows.Forms.TextBox();
            this.btFactoryResetInsteon = new System.Windows.Forms.Button();
            this.lblAudioState = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(168, 29);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(108, 22);
            this.button1.TabIndex = 0;
            this.button1.Text = "get temperature";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(88, 120);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(108, 22);
            this.button2.TabIndex = 1;
            this.button2.Text = "fan on";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // button3
            // 
            this.button3.Location = new System.Drawing.Point(100, 187);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(108, 22);
            this.button3.TabIndex = 2;
            this.button3.Text = "fan auto";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.button3_Click);
            // 
            // txtSirRate
            // 
            this.txtSirRate.Location = new System.Drawing.Point(108, 230);
            this.txtSirRate.Name = "txtSirRate";
            this.txtSirRate.Size = new System.Drawing.Size(100, 20);
            this.txtSirRate.TabIndex = 3;
            this.txtSirRate.Text = "+0.1";
            // 
            // btFactoryResetInsteon
            // 
            this.btFactoryResetInsteon.Location = new System.Drawing.Point(214, 115);
            this.btFactoryResetInsteon.Name = "btFactoryResetInsteon";
            this.btFactoryResetInsteon.Size = new System.Drawing.Size(65, 72);
            this.btFactoryResetInsteon.TabIndex = 4;
            this.btFactoryResetInsteon.Text = "factory reset insteon";
            this.btFactoryResetInsteon.UseVisualStyleBackColor = true;
            this.btFactoryResetInsteon.Click += new System.EventHandler(this.btFactoryResetInsteon_Click);
            // 
            // lblAudioState
            // 
            this.lblAudioState.AutoSize = true;
            this.lblAudioState.Location = new System.Drawing.Point(88, 270);
            this.lblAudioState.Name = "lblAudioState";
            this.lblAudioState.Size = new System.Drawing.Size(35, 13);
            this.lblAudioState.TabIndex = 5;
            this.lblAudioState.Text = "label1";
            // 
            // frmMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 296);
            this.Controls.Add(this.lblAudioState);
            this.Controls.Add(this.btFactoryResetInsteon);
            this.Controls.Add(this.txtSirRate);
            this.Controls.Add(this.button3);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "frmMain";
            this.Text = "Manny";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.frmMain_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.frmMain_FormClosed);
            this.Load += new System.EventHandler(this.frmMain_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.TextBox txtSirRate;
        private System.Windows.Forms.Button btFactoryResetInsteon;
        private System.Windows.Forms.Label lblAudioState;
    }
}

