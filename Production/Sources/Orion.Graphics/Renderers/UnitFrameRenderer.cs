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

        public override void Draw(GraphicsContext context)
        {
            string hp = "HP: {0}/{1}".FormatInvariant(unit.Health, unit.MaxHealth);
            context.FillColor = Color.DarkBlue;
            context.Draw(unit.Type.Name, new Vector2(150, 155));
            context.Draw(hp, new Vector2(150, 130));
            base.Draw(context);
        }
    }
}
