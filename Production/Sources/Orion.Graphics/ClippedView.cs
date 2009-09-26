using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Orion.Geometry;

namespace Orion.Graphics
{
    /// <summary>
    /// A <see cref="View"/> which clips its content within its display frame.
    /// </summary>
    public abstract class ClippedView : View
    {
        #region Constructors
        public ClippedView(Rectangle frame)
            : base(frame)
        { }
        #endregion
    }
}
