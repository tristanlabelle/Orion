using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Orion.Engine.Collections;
using Orion.Engine.Graphics;
using Orion.Engine.Input;

namespace Orion.Engine.Gui
{
    /// <summary>
    /// Base class of the UI hierarchy.
    /// </summary>
    public abstract partial class Control
    {
        #region Fields
        private object tag;
        #endregion

        #region Constructors
        protected Control()
        {
            manager = this as UIManager;
        }
        #endregion

        #region Properties
        /// <summary>
        /// Accesses the tag of this <see cref="Control"/>, which is a user data object associated with it.
        /// </summary>
        public object Tag
        {
            get { return tag; }
            set { tag = value; }
        }
        #endregion
    }
}
