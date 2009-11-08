using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Orion.GameLogic;
using OpenTK.Math;

namespace Orion.Graphics.Renderers
{
    public sealed class HealthBarRenderer
    {
        #region Methods

        public void RenderHealthBar(Unit unit)
        {
            float y = unit.BoundingRectangle.CenterY + unit.BoundingRectangle.Height * 0.75f;
        }

        #endregion
    }
}
