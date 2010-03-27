using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.Engine.Geometry;

namespace Orion.Engine.Gui
{
    public class Checkbox : RenderedView
    {
        #region Fields
        private bool state;
        #endregion

        #region Constructors
        public Checkbox(Rectangle frame)
            : this(frame, false)
        { }

        public Checkbox(Rectangle frame, bool state)
            : this(frame, state, new FilledFrameRenderer())
        { }

        public Checkbox(Rectangle frame, bool state, IViewRenderer renderer)
            : base(frame, renderer)
        {
            this.state = state;
        }
        #endregion

        #region Properties
        public bool State
        {
            get { return state; }
            set { state = value; }
        }
        #endregion
    }
}
