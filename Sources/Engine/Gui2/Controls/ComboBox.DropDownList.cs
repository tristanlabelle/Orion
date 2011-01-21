using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;

namespace Orion.Engine.Gui2
{
    partial class ComboBox
    {
        public sealed class DropDownList : Popup
        {
            #region Fields
            private readonly ComboBox comboBox;
            private readonly StackLayout itemStack;
            private int highlightedItemIndex = -1;
            private ColorRgba highlightColor = Colors.TransparentBlack;
            #endregion

            #region Constructors
            internal DropDownList(ComboBox comboBox)
            {
                Argument.EnsureNotNull(comboBox, "comboBox");

                this.comboBox = comboBox;
                Content = itemStack = new StackLayout()
                {
                    Direction = Direction.PositiveY
                };
            }
            #endregion

            #region Properties
            protected override bool IsModalImpl
            {
                get { return false; }
            }

            /// <summary>
            /// Accesses the items within this <see cref="DropDownList"/>.
            /// </summary>
            internal IList<Control> Items
            {
                get { return itemStack.Children; }
            }

            /// <summary>
            /// Accesses the color of the highlighting of the item under the mouse.
            /// </summary>
            public ColorRgba HighlightColor
            {
                get { return highlightColor; }
                set { highlightColor = value; }
            }

            /// <summary>
            /// Gets the gap, in pixels, between successive items.
            /// </summary>
            public int ItemGap
            {
                get { return itemStack.ChildGap; }
                set { itemStack.ChildGap = value; }
            }

            private Control HighlightedItem
            {
                get { return highlightedItemIndex == -1 ? null : Items[highlightedItemIndex]; }
            }
            #endregion

            #region Methods
            public override Region GetDesiredRectangle()
            {
                Region comboBoxRectangle = comboBox.Rectangle;
                Size desiredSize = DesiredOuterSize;
                
                return new Region(
                    comboBoxRectangle.MinX, comboBoxRectangle.ExclusiveMaxY,
                    desiredSize.Width, desiredSize.Height);
            }

            protected override bool OnMouseMoved(MouseEvent @event)
            {
                if (!Rectangle.Contains(@event.Position))
                {
                    highlightedItemIndex = -1;
                    return true;
                }

                // Highlight the item that is the closest to the cursor, the cheap way.
                float closestSquaredDistance = float.PositiveInfinity;
                for (int i = 0; i < itemStack.Children.Count; ++i)
                {
                    Control item = itemStack.Children[i];
                    float squaredDistance = ((Vector2)(@event.Position - item.Rectangle.Clamp(@event.Position))).LengthSquared;
                    if (squaredDistance < closestSquaredDistance)
                    {
                        highlightedItemIndex = i;
                        closestSquaredDistance = squaredDistance;
                    }
                }

                return true;
            }

            protected override bool OnMouseButton(MouseEvent @event)
            {
                if (!Rectangle.Contains(@event.Position))
                {
                    highlightedItemIndex = -1;
                }
                else if (highlightedItemIndex != -1)
                {
                    comboBox.SelectedItemIndex = highlightedItemIndex;
                    highlightedItemIndex = -1;
                }

                VisibilityFlag = Visibility.Hidden;

                return true;
            }

            protected internal override void Draw()
            {
                if (highlightedItemIndex != -1)
                {
                    Region innerRectangle = InnerRectangle;
                    Region itemOuterRectangle = HighlightedItem.OuterRectangle;

                    Region highlightRectangle = new Region(
                        innerRectangle.MinX, itemOuterRectangle.MinY,
                        innerRectangle.Width, itemOuterRectangle.Height);

                    Renderer.DrawRectangle(highlightRectangle, highlightColor);
                }
            }
            #endregion
        }
    }
}
