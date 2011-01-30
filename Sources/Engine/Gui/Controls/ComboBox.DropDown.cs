using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;

namespace Orion.Engine.Gui
{
    partial class ComboBox
    {
        /// <summary>
        /// Represents the popup which displays the list of items selectable from the <see cref="ComboBox"/>.
        /// </summary>
        public sealed class DropDownPopup : Popup
        {
            #region Fields
            private readonly ComboBox comboBox;
            #endregion

            #region Constructors
            internal DropDownPopup(ComboBox comboBox)
            {
                Argument.EnsureNotNull(comboBox, "comboBox");

                this.comboBox = comboBox;
                Content = new ListBox();
            }
            #endregion

            #region Properties
            protected override bool IsModalImpl
            {
                get { return false; }
            }

            /// <summary>
            /// Gets the <see cref="ListBox"/> displaying the items in this <see cref="DropDownPopup"/>.
            /// </summary>
            public ListBox ListBox
            {
                get { return (ListBox)Content; }
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

            protected override bool OnMouseButton(MouseEvent @event)
            {
                if (@event.IsPressed)
                {
                    comboBox.IsOpen = false;
                    return true;
                }

                return false;
            }
            #endregion
        }
    }
}
