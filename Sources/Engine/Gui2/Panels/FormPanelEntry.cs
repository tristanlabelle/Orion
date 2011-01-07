using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace Orion.Engine.Gui2
{
    /// <summary>
    /// Represents an entry in a <see cref="FormPanel"/>, with a header and a content control.
    /// </summary>
    [ImmutableObject(true)]
    public struct FormPanelEntry
    {
        #region Fields
        private readonly Control header;
        private readonly Control content;
        #endregion

        #region Constructors
        public FormPanelEntry(Control header, Control content)
        {
            this.header = header;
            this.content = content;
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets the header control of this entry.
        /// </summary>
        public Control Header
        {
            get { return header; }
        }

        /// <summary>
        /// Gets the content control of this entry.
        /// </summary>
        public Control Content
        {
            get { return content; }
        }
        #endregion
    }
}
