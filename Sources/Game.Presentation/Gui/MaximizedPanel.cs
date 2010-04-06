using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.Engine.Gui;

namespace Orion.Game.Presentation.Gui
{
    /// <summary>
    /// A panel which takes the whole screen area.
    /// </summary>
    public class MaximizedPanel : Responder
    {
        #region Constructors
        public MaximizedPanel()
        {
            Frame = RootView.ContentsBounds;
            Bounds = Frame;
        }
        #endregion
    }
}
