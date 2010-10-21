using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Orion.Engine.Gui2
{
    public abstract class Panel : UIElement
    {
        #region Fields
        private Borders padding;
        #endregion

        #region Constructors
        #endregion

        #region Properties
        public Borders Padding
        {
            get { return padding; }
            set { padding = value; }
        }
        #endregion

        #region Methods
        #endregion
    }
}
