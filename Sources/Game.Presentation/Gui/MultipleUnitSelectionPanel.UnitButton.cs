using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.Game.Simulation;
using Orion.Engine.Gui2;
using Orion.Engine;
using System.Diagnostics;
using Orion.Game.Presentation.Renderers;

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
                    UpdateImageTint();
                }
            }
            #endregion

            #region Methods
            /// <summary>
            /// Updates the tint of the unit image to reflect its health.
            /// </summary>
            public void UpdateImageTint()
            {
                float healthFraction = unit == null ? 1 : unit.Health / unit.MaxHealth;
                imageBox.Tint = HealthBarRenderer.GetColor(healthFraction);
            }

            protected override void Draw()
            {
                UpdateImageTint();
                base.Draw();
            }

            protected override void OnClicked(ButtonClickEvent @event)
            {
                Debug.Assert(unit != null);

                if (@event.Type == ButtonClickType.Mouse && @event.MouseEvent.IsShiftDown)
                    panel.UnitDeselected.Raise(panel, unit);
                else
                    panel.UnitSelected.Raise(panel, unit);
            }
            #endregion
        }
    }
}
