using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.Engine.Gui2;
using Orion.Engine.Graphics;
using Orion.Game.Presentation.Actions;

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
            private ActionDescriptor descriptor;
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
            /// Accesses the <see cref="ActionDescriptor"/> of this button's action.
            /// </summary>
            public ActionDescriptor Descriptor
            {
                get { return descriptor; }
                set
                {
                    descriptor = value;
                    if (descriptor == null)
                    {
                        VisibilityFlag = Visibility.Hidden;
                        return;
                    }

                    VisibilityFlag = Visibility.Visible;
                    ((ImageBox)Content).Texture = descriptor.Texture;
                }
            }
            #endregion

            #region Methods
            protected override void OnClicked(MouseButtons button)
            {
                if (descriptor != null && descriptor.Action != null) descriptor.Action();
            }
            #endregion
        }
    }
}
