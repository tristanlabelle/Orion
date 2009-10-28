namespace Orion.UserInterface
{
    partial class MatchSettingsDialog
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
            System.Windows.Forms.Label label1;
            System.Windows.Forms.GroupBox groupBox1;
            System.Windows.Forms.GroupBox groupBox2;
            this.startSoloGameButton = new System.Windows.Forms.Button();
            this.joinMultiplayerGameButton = new System.Windows.Forms.Button();
            this.hostMultiplayerGameButton = new System.Windows.Forms.Button();
            this.multiplayerHostTextBox = new System.Windows.Forms.TextBox();
            label1 = new System.Windows.Forms.Label();
            groupBox1 = new System.Windows.Forms.GroupBox();
            groupBox2 = new System.Windows.Forms.GroupBox();
            groupBox1.SuspendLayout();
            groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(11, 51);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(32, 13);
            label1.TabIndex = 5;
            label1.Text = "Host:";
            // 
            // groupBox1
            // 
            groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            groupBox1.Controls.Add(this.startSoloGameButton);
            groupBox1.Location = new System.Drawing.Point(2, 3);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new System.Drawing.Size(265, 55);
            groupBox1.TabIndex = 7;
            groupBox1.TabStop = false;
            groupBox1.Text = "Solo";
            // 
            // startSoloGameButton
            // 
            this.startSoloGameButton.Location = new System.Drawing.Point(10, 19);
            this.startSoloGameButton.Name = "startSoloGameButton";
            this.startSoloGameButton.Size = new System.Drawing.Size(249, 23);
            this.startSoloGameButton.TabIndex = 4;
            this.startSoloGameButton.Text = "Start Game";
            this.startSoloGameButton.UseVisualStyleBackColor = true;
            this.startSoloGameButton.Click += new System.EventHandler(this.startSoloGameButton_Click);
            // 
            // groupBox2
            // 
            groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            groupBox2.Controls.Add(this.joinMultiplayerGameButton);
            groupBox2.Controls.Add(this.hostMultiplayerGameButton);
            groupBox2.Controls.Add(this.multiplayerHostTextBox);
            groupBox2.Controls.Add(label1);
            groupBox2.Location = new System.Drawing.Point(2, 64);
            groupBox2.Name = "groupBox2";
            groupBox2.Size = new System.Drawing.Size(265, 75);
            groupBox2.TabIndex = 8;
            groupBox2.TabStop = false;
            groupBox2.Text = "Multiplayer";
            // 
            // joinMultiplayerGameButton
            // 
            this.joinMultiplayerGameButton.Location = new System.Drawing.Point(190, 46);
            this.joinMultiplayerGameButton.Name = "joinMultiplayerGameButton";
            this.joinMultiplayerGameButton.Size = new System.Drawing.Size(69, 23);
            this.joinMultiplayerGameButton.TabIndex = 7;
            this.joinMultiplayerGameButton.Text = "Join";
            this.joinMultiplayerGameButton.UseVisualStyleBackColor = true;
            this.joinMultiplayerGameButton.Click += new System.EventHandler(this.joinMultiplayerGameButton_Click);
            // 
            // hostMultiplayerGameButton
            // 
            this.hostMultiplayerGameButton.Location = new System.Drawing.Point(10, 19);
            this.hostMultiplayerGameButton.Name = "hostMultiplayerGameButton";
            this.hostMultiplayerGameButton.Size = new System.Drawing.Size(249, 23);
            this.hostMultiplayerGameButton.TabIndex = 3;
            this.hostMultiplayerGameButton.Text = "Host Game";
            this.hostMultiplayerGameButton.UseVisualStyleBackColor = true;
            this.hostMultiplayerGameButton.Click += new System.EventHandler(this.hostMultiplayerGameButton_Click);
            // 
            // multiplayerHostTextBox
            // 
            this.multiplayerHostTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.multiplayerHostTextBox.Location = new System.Drawing.Point(49, 48);
            this.multiplayerHostTextBox.Name = "multiplayerHostTextBox";
            this.multiplayerHostTextBox.Size = new System.Drawing.Size(135, 20);
            this.multiplayerHostTextBox.TabIndex = 2;
            // 
            // MatchSettingsDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(270, 141);
            this.Controls.Add(groupBox2);
            this.Controls.Add(groupBox1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "MatchSettingsDialog";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Match Settings";
            groupBox1.ResumeLayout(false);
            groupBox2.ResumeLayout(false);
            groupBox2.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TextBox multiplayerHostTextBox;
        private System.Windows.Forms.Button hostMultiplayerGameButton;
        private System.Windows.Forms.Button startSoloGameButton;
        private System.Windows.Forms.Button joinMultiplayerGameButton;

    }
}