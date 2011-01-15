using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.Engine.Gui2;
using Orion.Engine;
using Orion.Game.Simulation;
using Orion.Game.Presentation.Actions;

namespace Orion.Game.Presentation.Gui
{
    partial class MatchUI2
    {
        private sealed class ActionToolTip : ContentControl
        {
            #region Fields
            private readonly Label nameLabel;
            private readonly ImageBox aladdiumImageBox;
            private readonly Label aladdiumCostLabel;
            private readonly ImageBox alageneImageBox;
            private readonly Label alageneCostLabel;
            private readonly ImageBox foodImageBox;
            private readonly Label foodCostLabel;
            private ActionDescriptor descriptor;
            #endregion

            #region Constructors
            public ActionToolTip(OrionGuiStyle style)
            {
                Argument.EnsureNotNull(style, "style");

                Margin = new Borders(4);
                Padding = new Borders(2);
                MinWidth = 200;

                DockLayout dock = new DockLayout();

                nameLabel = style.Create<Label>();
                dock.Dock(nameLabel, Direction.MinY);

                StackLayout costsStack = new StackLayout()
                {
                    ChildGap = 5,
                    MinYMargin = 5,
                    Direction = Direction.MaxX
                };

                StackResource(style, costsStack, "Aladdium", out aladdiumImageBox, out aladdiumCostLabel);
                StackResource(style, costsStack, "Alagene", out alageneImageBox, out alageneCostLabel);
                StackResource(style, costsStack, "Gui/Food", out foodImageBox, out foodCostLabel);

                dock.Dock(costsStack, Direction.MaxY);

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
                    nameLabel.Text = descriptor.Name ?? string.Empty;
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

            private void StackResource(OrionGuiStyle style, StackLayout stack, string textureName, out ImageBox imageBox, out Label costLabel)
            {
                imageBox = new ImageBox()
                {
                    Width = 16,
                    Height = 16,
                    Texture = style.GetTexture(textureName),
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
