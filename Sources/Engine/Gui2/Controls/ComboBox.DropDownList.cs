using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Orion.Engine.Gui2
{
    partial class ComboBox
    {
        public sealed class DropDownList : Popup
        {
            #region Fields
            private readonly ComboBox comboBox;
            private readonly StackLayout itemStack;
            private Control highlightedItem;
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
            public override bool IsModal
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
                if (!Rectangle.Contains(@event.Position)) return true;

                foreach (Control control in itemStack.Children)
                {
                    if (control.OuterRectangle.Contains(@event.Position))
                    {
                        highlightedItem = control;
                        break;
                    }
                }

                return true;
            }

            protected override bool OnMouseButton(MouseEvent @event)
            {
                if (!Rectangle.Contains(@event.Position))
                {
                    highlightedItem = null;
                }
                else if (highlightedItem != null)
                {
                    comboBox.SelectedItem = highlightedItem;
                    highlightedItem = null;
                }

                VisibilityFlag = Visibility.Hidden;

                return true;
            }

            protected internal override void Draw()
            {
                if (highlightedItem != null)
                {
                    Renderer.DrawRectangle(highlightedItem.OuterRectangle, highlightColor);
                }
            }
            #endregion
        }
    }
}
