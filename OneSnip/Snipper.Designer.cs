using System;
using System.Drawing;
using System.Windows.Forms;

namespace OneSnip
{
    partial class Snipper
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
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.prePrintscreen = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.prePrintscreen)).BeginInit();
            this.SuspendLayout();
            // 
            // pictureBox1
            // 
            this.pictureBox1.Location = new System.Drawing.Point(0, 0);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(100, 50);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBox1.TabIndex = 0;
            this.pictureBox1.TabStop = false;
            this.pictureBox1.MouseDown += new System.Windows.Forms.MouseEventHandler(this.pictureBox1_MouseDown);
            this.pictureBox1.MouseMove += new System.Windows.Forms.MouseEventHandler(this.pictureBox1_MouseMove);
            this.pictureBox1.MouseUp += new System.Windows.Forms.MouseEventHandler(this.pictureBox1_MouseUp);
            // 
            // prePrintscreen
            // 
            this.prePrintscreen.Location = new System.Drawing.Point(0, 0);
            this.prePrintscreen.Name = "prePrintscreen";
            this.prePrintscreen.Size = new System.Drawing.Size(100, 50);
            this.prePrintscreen.TabIndex = 1;
            this.prePrintscreen.TabStop = false;
            this.prePrintscreen.MouseDown += new System.Windows.Forms.MouseEventHandler(this.prePrintscreen_MouseDown);
            this.prePrintscreen.MouseMove += new System.Windows.Forms.MouseEventHandler(this.prePrintscreen_MouseMove);
            this.prePrintscreen.MouseUp += new System.Windows.Forms.MouseEventHandler(this.prePrintscreen_MouseUp);
            // 
            // Snipper
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.HotPink;
            this.ClientSize = new System.Drawing.Size(284, 261);
            this.Controls.Add(this.pictureBox1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "Snipper";
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "Snipper";
            this.TransparencyKey = System.Drawing.Color.HotPink;
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.Load += new System.EventHandler(this.Snipper_Load);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.Snipper_KeyDown);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.prePrintscreen)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.PictureBox pictureBox1;
        private PictureBox prePrintscreen;
    }
}