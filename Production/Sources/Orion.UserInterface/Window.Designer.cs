using OpenTK.Graphics;

namespace Orion.UserInterface
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
            this.glControl = new Orion.UserInterface.CustomGLControl();
            this.SuspendLayout();
            // 
            // glControl
            // 
            this.glControl.BackColor = System.Drawing.Color.Black;
            this.glControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.glControl.Location = new System.Drawing.Point(0, 0);
            this.glControl.Margin = new System.Windows.Forms.Padding(5, 5, 5, 5);
            this.glControl.Name = "glControl";
            this.glControl.Size = new System.Drawing.Size(1056, 702);
            this.glControl.TabIndex = 0;
            this.glControl.VSync = false;
            this.glControl.MouseWheel += new System.Windows.Forms.MouseEventHandler(this.glControl_MouseWheel);
            this.glControl.Paint += new System.Windows.Forms.PaintEventHandler(this.glControl_Paint);
            this.glControl.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.glControl_MouseDoubleClick);
            this.glControl.KeyUp += new System.Windows.Forms.KeyEventHandler(this.glControl_KeyUp);
            this.glControl.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.glControl_KeyPress);
            this.glControl.KeyDown += new System.Windows.Forms.KeyEventHandler(this.glControl_KeyDown);
            // 
            // Window
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1056, 702);
            this.Controls.Add(this.glControl);
            this.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.ShowIcon = false;
            this.Text = "Orion";
            this.ResumeLayout(false);

        }

        #endregion

        private Orion.UserInterface.CustomGLControl glControl;
    }
}