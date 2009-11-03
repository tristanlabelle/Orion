using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

using OpenTK.Math;
using Orion.GameLogic;

namespace Orion.Graphics
{
    public class UnitFrameRenderer : FrameRenderer
    {
        private Unit unit;

        public UnitFrameRenderer(Unit unit)
        {
            this.unit = unit;
        }

        public override void RenderInto(GraphicsContext context)
        {
            string hp = "HP: {0}/{1}".FormatInvariant(unit.Health, unit.MaxHealth);
            context.FillColor = Color.DarkBlue;
            context.DrawText(unit.Type.Name, new Vector2(150, 175));
            context.DrawText(hp, new Vector2(150, 150));
            base.RenderInto(context);
        }
    }
}
