using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.Game.Simulation;
using Orion.Engine.Gui;
using Orion.Engine;
using System.Diagnostics;
using Orion.Game.Presentation.Renderers;
using Orion.Game.Simulation.Components;

namespace Orion.Game.Presentation.Gui
{
    partial class MultipleUnitSelectionPanel
    {
        private sealed class UnitButton : Button
        {
            #region Fields
            private readonly MultipleUnitSelectionPanel panel;
            private readonly ImageBox imageBox;
            private Entity unit;
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
            /// Accesses the <see cref="Entity"/> displayed by this button.
            /// </summary>
            public Entity Unit
            {
                get { return unit; }
                set
                {
                    unit = value;
                    imageBox.Texture = unit == null ? null : panel.graphics.GetEntityTexture(unit);
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
                float healthFraction = 1;
                if (unit != null)
                {
                    Health health = unit.Components.TryGet<Health>();
                    if (health != null)
                        healthFraction = health.Value / (float)unit.GetStatValue(Health.MaxValueStat);
                }
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
