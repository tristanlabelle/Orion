using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;
using Orion.Engine;
using Orion.Engine.Geometry;
using Orion.Engine.Graphics;
using Orion.Game.Simulation;

namespace Orion.Game.Presentation.Renderers
{
    /// <summary>
    /// Responsible for drawing the health bars of the units.
    /// </summary>
    public static class HealthBarRenderer
    {
        #region Fields
        private static readonly ColorRgb noHealthColor = Colors.Red;
        private static readonly ColorRgb middleHealthColor = Colors.Yellow;
        private static readonly ColorRgb fullHealthColor = Colors.ForestGreen;
        private static readonly ColorRgb borderColor = Colors.DarkGray;
        #endregion

        #region Methods
        public static void Draw(GraphicsContext context, Unit unit)
        {
            float healthbarWidth = (float)Math.Log(unit.MaxHealth);
            Rectangle unitBoundingRectangle = unit.BoundingRectangle;
            float y = unitBoundingRectangle.CenterY - unitBoundingRectangle.Height * 0.75f;
            float x = unitBoundingRectangle.CenterX - healthbarWidth / 2f;
            Draw(context, unit, new Vector2(x, y));
        }

        public static void Draw(GraphicsContext context, Unit unit, Vector2 origin)
        {
            float healthbarWidth = (float)Math.Log(unit.MaxHealth);
            float leftHealthWidth = unit.Health * 0.1f;
            Rectangle healthBarBounds = new Rectangle(origin, new Vector2(healthbarWidth, 0.15f));
            Draw(context, unit, healthBarBounds);
        }

        public static void Draw(GraphicsContext context, Unit unit, Rectangle into)
        {
            float leftHealthWidth = into.Width * (unit.Health / unit.MaxHealth);
            Vector2 origin = into.Min;

            float healthFraction = unit.Health / unit.MaxHealth;
            ColorRgb healthColor = GetColor(healthFraction);

            Rectangle lifeRect = new Rectangle(origin.X, origin.Y, leftHealthWidth, into.Height);
            context.Fill(lifeRect, healthColor);
            Rectangle damageRect = new Rectangle(
                origin.X + leftHealthWidth, origin.Y,
                into.Width - leftHealthWidth, into.Height);

            context.Fill(damageRect, borderColor);
        }

        public static ColorRgb GetColor(float healthFraction)
        {
            if (healthFraction >= 0.5f)
            {
                float subprogress = (healthFraction - 0.5f) * 2;
                return ColorRgb.Lerp(middleHealthColor, fullHealthColor, subprogress);
            }
            else
            {
                float subprogress = healthFraction * 2;
                return ColorRgb.Lerp(noHealthColor, middleHealthColor, subprogress);
            }
        }
        #endregion
    }
}
