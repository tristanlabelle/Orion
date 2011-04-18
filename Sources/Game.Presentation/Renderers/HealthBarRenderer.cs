using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;
using Orion.Engine;
using Orion.Engine.Geometry;
using Orion.Engine.Graphics;
using Orion.Game.Simulation;
using Orion.Game.Simulation.Components;

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
        public static void Draw(GraphicsContext context, Entity entity)
        {
            Spatial spatial = entity.Spatial;
            Health health = entity.Components.TryGet<Health>();
            if (spatial == null || health == null) return;
            
            float healthbarWidth = (float)Math.Log((int)entity.GetStatValue(Health.MaxValueStat));
            Rectangle unitBoundingRectangle = spatial.BoundingRectangle;
            float y = unitBoundingRectangle.CenterY - unitBoundingRectangle.Height * 0.75f;
            float x = unitBoundingRectangle.CenterX - healthbarWidth / 2f;
            Draw(context, entity, new Vector2(x, y));
        }

        public static void Draw(GraphicsContext context, Entity entity, Vector2 origin)
        {
            Health health = entity.Components.TryGet<Health>();
            if (health == null) return;
            
            float healthbarWidth = (float)Math.Log((int)entity.GetStatValue(Health.MaxValueStat));
            float leftHealthWidth = health.Value * 0.1f;
            Rectangle healthBarBounds = new Rectangle(origin, new Vector2(healthbarWidth, 0.15f));
            Draw(context, entity, healthBarBounds);
        }

        public static void Draw(GraphicsContext context, Entity entity, Rectangle into)
        {
            Health health = entity.Components.TryGet<Health>();
            if (health != null)
            {
                float healthFraction = health.Value / (float)entity.GetStatValue(Health.MaxValueStat);
                float leftHealthWidth = into.Width * healthFraction;
                Vector2 origin = into.Min;

                ColorRgb healthColor = GetColor(healthFraction);

                Rectangle lifeRect = new Rectangle(origin.X, origin.Y, leftHealthWidth, into.Height);
                context.Fill(lifeRect, healthColor);
                Rectangle damageRect = new Rectangle(
                    origin.X + leftHealthWidth, origin.Y,
                    into.Width - leftHealthWidth, into.Height);

                context.Fill(damageRect, borderColor);
            }
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
