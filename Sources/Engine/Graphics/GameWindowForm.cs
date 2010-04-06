using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Orion.Engine.Graphics
{
    /// <summary>
    /// The form used the WindowsFormsGameWindow.
    /// </summary>
    internal class GameWindowForm : Form
    {
        #region Fields
        private readonly CustomGLControl glControl;
        #endregion

        #region Constructors
        public GameWindowForm()
        {
            ShowIcon = false;

            glControl = new CustomGLControl();
            glControl.Dock = DockStyle.Fill;
            glControl.VSync = true;

            Controls.Add(glControl);
            CreateControl();
            Show();
        }
        #endregion

        #region Events
        public event Action<GameWindowForm, string> TextPasted;
        #endregion

        #region Properties
        public CustomGLControl GLControl
        {
            get { return glControl; }
        }
        #endregion

        #region Methods
        protected override void WndProc(ref Message m)
        {
            const int WM_PASTE = 0x0302;
            if (m.Msg == WM_PASTE)
            {
                string pastedText = Clipboard.GetText();
                if (!string.IsNullOrEmpty(pastedText))
                {
                    TextPasted.Raise(this, pastedText);
                    return;
                }
            }

            base.WndProc(ref m);
        }
        #endregion
    }
}
