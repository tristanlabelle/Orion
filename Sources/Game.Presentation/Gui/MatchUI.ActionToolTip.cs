using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.Engine;
using Orion.Engine.Graphics;
using Orion.Engine.Gui;
using Orion.Game.Presentation.Actions;
using Orion.Game.Simulation;
using Key = OpenTK.Input.Key;

namespace Orion.Game.Presentation.Gui
{
    partial class MatchUI
    {
        private sealed class ActionToolTip : ContentControl
        {
            #region Fields
            private readonly Label headerLabel;
            private readonly ImageBox aladdiumImageBox;
            private readonly Label aladdiumCostLabel;
            private readonly ImageBox alageneImageBox;
            private readonly Label alageneCostLabel;
            private readonly ImageBox foodImageBox;
            private readonly Label foodCostLabel;
            private ActionDescriptor descriptor;
            #endregion

            #region Constructors
            public ActionToolTip(GameGraphics graphics)
            {
                Argument.EnsureNotNull(graphics, "graphics");

                var style = graphics.GuiStyle;
                
                Margin = new Borders(4);
                Padding = new Borders(2);
                MinWidth = 200;

                DockLayout dock = new DockLayout();

                headerLabel = style.Create<Label>();
                dock.Dock(headerLabel, Direction.NegativeY);

                StackLayout costsStack = new StackLayout()
                {
                    ChildGap = 5,
                    MinYMargin = 5,
                    Direction = Direction.PositiveX
                };

                StackResource(style, costsStack, graphics.GetMiscTexture("Aladdium"), out aladdiumImageBox, out aladdiumCostLabel);
                StackResource(style, costsStack, graphics.GetMiscTexture("Alagene"), out alageneImageBox, out alageneCostLabel);
                StackResource(style, costsStack, graphics.GetGuiTexture("Food"), out foodImageBox, out foodCostLabel);

                dock.Dock(costsStack, Direction.PositiveY);

                Content = dock;

                VisibilityFlag = Visibility.Hidden;
            }
            #endregion

            #region Properties
            /// <summary>
            /// Accesses the <see cref="ActionDescriptor"/> of the action described by this tooltip.
            /// </summary>
            public ActionDescriptor Descriptor
            {
                get { return descriptor; }
                set
                {
                    if (value == descriptor) return;

                    descriptor = value;
                    if (descriptor == null)
                    {
                        VisibilityFlag = Visibility.Hidden;
                        return;
                    }

                    VisibilityFlag = Visibility.Visible;

                    string headerText = descriptor.Name ?? string.Empty;
                    if (descriptor.HotKey != Key.Unknown)
                        headerText += " (" + descriptor.HotKey.ToStringInvariant() + ")";

                    headerLabel.Text = headerText;
                    UpdateResource(descriptor.Cost.Aladdium, aladdiumImageBox, aladdiumCostLabel);
                    UpdateResource(descriptor.Cost.Alagene, alageneImageBox, alageneCostLabel);
                    UpdateResource(descriptor.Cost.Food, foodImageBox, foodCostLabel);
                }
            }
            #endregion

            #region Methods
            private void UpdateResource(int amount, ImageBox imageBox, Label costLabel)
            {
                if (amount > 0)
                {
                    imageBox.VisibilityFlag = Visibility.Visible;
                    costLabel.VisibilityFlag = Visibility.Visible;
                    costLabel.Text = amount.ToStringInvariant();
                }
                else
                {
                    imageBox.VisibilityFlag = Visibility.Collapsed;
                    costLabel.VisibilityFlag = Visibility.Collapsed;
                }
            }

            private void StackResource(OrionGuiStyle style, StackLayout stack, Texture texture, out ImageBox imageBox, out Label costLabel)
            {
                imageBox = new ImageBox()
                {
                    Width = 16,
                    Height = 16,
                    Texture = texture
                };
                stack.Stack(imageBox);

                costLabel = style.Create<Label>();
                costLabel.MinWidth = 40;
                stack.Stack(costLabel);
            }
            #endregion
        }
    }
}
