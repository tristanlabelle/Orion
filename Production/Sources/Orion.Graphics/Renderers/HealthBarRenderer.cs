﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.GameLogic;
using Orion.Geometry;
using Color = System.Drawing.Color;
using OpenTK.Math;

namespace Orion.Graphics.Renderers
{
    /// <summary>
    /// Responsible for drawing the health bars of the units.
    /// </summary>
    public static class HealthBarRenderer
    {
        #region Fields
        private static readonly Color lowLifeColor = Color.Red;
        private static readonly Color middleLifeColor = Color.Yellow;
        private static readonly Color fullLifeColor = Color.ForestGreen;
        #endregion

        #region Methods
        public static void Draw(GraphicsContext context, Unit unit)
        {
            float healthbarWidth = (float)Math.Log(unit.MaxHealth);
            Rectangle unitBoundingRectangle = unit.BoundingRectangle;
            float y = unitBoundingRectangle.CenterY + unitBoundingRectangle.Height * 0.75f;
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

            float lifeFraction = unit.Health / unit.MaxHealth;
            context.FillColor = Interpolate(lowLifeColor, middleLifeColor, fullLifeColor, lifeFraction);

            Rectangle lifeRect = new Rectangle(origin.X, origin.Y, leftHealthWidth, into.Height);
            context.Fill(lifeRect);
            Rectangle damageRect = new Rectangle(
                origin.X + leftHealthWidth, origin.Y,
                into.Width - leftHealthWidth, into.Height);
            context.FillColor = Color.DarkGray;
            context.Fill(damageRect);
        }

        private static Color Interpolate(Color first, Color second, float progress)
        {
            float opposite = 1 - progress;
            return Color.FromArgb(
                (int)(first.R * opposite + second.R * progress),
                (int)(first.G * opposite + second.G * progress),
                (int)(first.B * opposite + second.B * progress));
        }

        private static Color Interpolate(Color first, Color second, Color third, float progress)
        {
            if (progress >= 0.5f)
            {
                float subprogress = (progress - 0.5f) * 2;
                return Interpolate(middleLifeColor, fullLifeColor, subprogress);
            }
            else
            {
                float subprogress = progress * 2;
                return Interpolate(lowLifeColor, middleLifeColor, subprogress);
            }
        }
        #endregion
    }
}