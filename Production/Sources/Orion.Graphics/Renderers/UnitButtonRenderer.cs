using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.GameLogic;
using OpenTK.Math;
using System.Drawing;

namespace Orion.Graphics
{
    public class UnitButtonRenderer : FrameRenderer
    {
        public readonly LinePath Shape;
        public readonly Unit Unit;

        public UnitButtonRenderer(LinePath shape, Unit unit)
        {
            Shape = shape;
            Unit = unit;
            StrokeColor = unit.Faction.Color;
        }

        public override void RenderInto(GraphicsContext context)
        {
            context.StrokeColor = Color.Black;
            context.FillColor = Color.Gainsboro;
            context.Fill(context.CoordinateSystem);
            context.Stroke(context.CoordinateSystem);

            context.StrokeColor = StrokeColor;

            float x = context.CoordinateSystem.Width / 2;
            float y = context.CoordinateSystem.Height / 3 * 2;
            context.Stroke(Shape, new Vector2(x, y));

            float healthRatio = Unit.Health / Unit.MaxHealth;
            float yHealth = context.CoordinateSystem.Height / 3;
            Vector2 start = new Vector2(context.CoordinateSystem.Width / 4, yHealth);
            Vector2 end = new Vector2(context.CoordinateSystem.Width / 4 * 3, yHealth);
            DrawHealthBar(context, start, end, healthRatio);
        }

        private void DrawHealthBar(GraphicsContext graphics,
             Vector2 start, Vector2 end, float ratio)
        {
            float length = (end - start).Length;

            Vector2 healthBarLevelPosition = start + Vector2.UnitX * ratio * length;

            graphics.StrokeColor = Color.Lime;
            graphics.StrokeLineStrip(start, healthBarLevelPosition);
            graphics.StrokeColor = Color.Red;
            graphics.StrokeLineStrip(healthBarLevelPosition, end);
        }
    }
}
