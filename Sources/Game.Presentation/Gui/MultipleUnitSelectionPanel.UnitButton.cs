using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.Game.Simulation;
using Orion.Engine.Gui2;
using Orion.Engine;
using System.Diagnostics;

namespace Orion.Game.Presentation.Gui
{
    partial class MultipleUnitSelectionPanel
    {
        private sealed class UnitButton : Button
        {
            #region Fields
            private readonly MultipleUnitSelectionPanel panel;
            private readonly ImageBox imageBox;
            private Unit unit;
            #endregion

            #region Constructors
            public UnitButton(MultipleUnitSelectionPanel panel)
            {
                Argument.EnsureNotNull(panel, "panel");

                this.panel = panel;

                panel.graphics.GuiStyle.ApplyStyle(this);
                AcquireKeyboardFocusWhenPressed = false;

                Content = imageBox = new ImageBox()
                {
                    Width = 24,
                    Height = 24
                };
            }
            #endregion

            #region Properties
            /// <summary>
            /// Accesses the <see cref="Unit"/> displayed by this button.
            /// </summary>
            public Unit Unit
            {
                get { return unit; }
                set
                {
                    unit = value;
                    imageBox.Texture = unit == null ? null : panel.graphics.GetUnitTexture(unit);
                }
            }
            #endregion

            #region Methods
            protected override void OnClicked(MouseButtons button)
            {
                Debug.Assert(unit != null);
                panel.UnitClicked(panel, unit);
            }
            #endregion
        }
    }
}
