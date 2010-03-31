using OpenTK.Graphics;

namespace Orion.Game.Presentation
{
    partial class Window
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
            this.glControl = new Orion.Engine.Graphics.CustomGLControl();
            this.SuspendLayout();
            // 
            // glControl
            // 
            this.glControl.BackColor = System.Drawing.Color.Black;
            this.glControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.glControl.Location = new System.Drawing.Point(0, 0);
            this.glControl.Name = "glControl";
            this.glControl.Size = new System.Drawing.Size(961, 678);
            this.glControl.TabIndex = 0;
            this.glControl.VSync = true;
            this.glControl.MouseWheel += new System.Windows.Forms.MouseEventHandler(this.glControl_MouseWheel);
            this.glControl.MouseMove += new System.Windows.Forms.MouseEventHandler(this.glControl_MouseMove);
            this.glControl.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.glControl_MouseDoubleClick);
            this.glControl.KeyUp += new System.Windows.Forms.KeyEventHandler(this.glControl_KeyUp);
            this.glControl.MouseDown += new System.Windows.Forms.MouseEventHandler(this.glControl_MouseDown);
            this.glControl.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.glControl_KeyPress);
            this.glControl.MouseUp += new System.Windows.Forms.MouseEventHandler(this.glControl_MouseUp);
            this.glControl.KeyDown += new System.Windows.Forms.KeyEventHandler(this.glControl_KeyDown);
            // 
            // Window
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(961, 678);
            this.Controls.Add(this.glControl);
            this.Name = "Window";
            this.ShowIcon = false;
            this.Text = "Orion";
            this.ResumeLayout(false);

        }

        #endregion

        private Orion.Engine.Graphics.CustomGLControl glControl;
    }
}