using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.Engine.Gui2;
using Orion.Engine.Graphics;

namespace Orion.Game.Presentation.Gui
{
    partial class MatchUI2
    {
        /// <summary>
        /// A <see cref="Button"/> which is specialized to appear in the action panel.
        /// </summary>
        private sealed class ActionButton : Button
        {
            #region Fields
            private Action action;
            #endregion

            #region Constructors
            public ActionButton()
            {
                AcquireKeyboardFocusWhenPressed = false;
                Content = new ImageBox();
            }
            #endregion

            #region Properties
            /// <summary>
            /// Accesses the texture displayed on this <see cref="ActionButton"/>.
            /// </summary>
            public Texture Texture
            {
                get { return ImageBox.Texture; }
                set { ImageBox.Texture = value; }
            }

            /// <summary>
            /// Accesses the callback that is invoked when this button is pressed.
            /// </summary>
            public Action Action
            {
                get { return action; }
                set { action = value; }
            }

            private ImageBox ImageBox
            {
                get { return (ImageBox)Content; }
            }
            #endregion

            #region Methods
            protected override void OnClicked(MouseButtons button)
            {
                if (action != null) action();
            }
            #endregion
        }
    }
}
