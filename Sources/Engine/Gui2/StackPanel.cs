using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Orion.Engine.Gui2
{
    public sealed class StackPanel : Panel
    {
        #region Fields
        private Orientation orientation = Orientation.Vertical;
        private Alignment itemAlignment = Alignment.Stretch;
        private int itemPadding;
        #endregion

        #region Constructors
        #endregion

        #region Properties
        public Orientation Orientation
        {
            get { return orientation; }
            set
            {
                Argument.EnsureDefined(value, "Orientation");
                orientation = value;
            }
        }

        public Alignment ItemAlignment
        {
            get { return itemAlignment; }
            set
            {
                Argument.EnsureDefined(value, "ItemAlignment");
                itemAlignment = value;
            }
        }

        public int ItemPadding
        {
            get { return itemPadding; }
            set
            {
                Argument.EnsurePositive(value, "ItemPadding");
                itemPadding = value;
            }
        }
        #endregion

        #region Methods
        #endregion
    }
}
