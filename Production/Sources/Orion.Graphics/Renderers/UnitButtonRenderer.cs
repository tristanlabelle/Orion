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
        private bool hasFocus;

        public UnitButtonRenderer(LinePath shape, Unit unit)
        {
            Shape = shape;
            Unit = unit;
            StrokeColor = unit.Faction.Color;
        }

        public bool HasFocus
        {
            get { return hasFocus; }
            set { hasFocus = value; }
        }

        public override void RenderInto(GraphicsContext context)
        {
            context.StrokeColor = Color.Black;
            context.FillColor = hasFocus ? Color.Gray : Color.Gainsboro;
            context.Fill(context.CoordinateSystem);
            context.Stroke(context.CoordinateSystem);

            context.StrokeColor = StrokeColor;

            float x = context.CoordinateSystem.Width / 2;
            float y = context.CoordinateSystem.Height / 3 * 2;
            context.Stroke(Shape, new Vector2(x, y));

            float healthRatio = Unit.Health / Unit.MaxHealth;
            float yHealth = context.CoordinateSystem.Height / 4;
            Vector2 start = new Vector2(context.CoordinateSystem.Width / 5, yHealth);
            Vector2 end = new Vector2(context.CoordinateSystem.Width / 5 * 4, yHealth);
            DrawHealthBar(context, start, end, healthRatio);
        }

        private void DrawHealthBar(GraphicsContext graphics,
             Vector2 start, Vector2 end, float ratio)
        {
            float length = (end - start).Length;

            Vector2 healthBarLevelPosition = start + Vector2.UnitX * ratio * length;
            healthBarLevelPosition.Y -= 0.5f;

            graphics.FillColor = Color.Green;
            graphics.Fill(new Orion.Geometry.Rectangle(start, healthBarLevelPosition - start));
            graphics.FillColor = Color.Red;
            graphics.Fill(new Orion.Geometry.Rectangle(healthBarLevelPosition, end - healthBarLevelPosition));
        }
    }
}
