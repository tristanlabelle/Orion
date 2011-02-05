using System;
using Orion.Engine;
using Orion.Engine.Geometry;
using Orion.Engine.Graphics;
using Orion.Game.Simulation;

namespace Orion.Game.Presentation.Renderers
{
    /// <summary>
    /// A renderer which draws the world as seen through the minimap, as well as overlays.
    /// </summary>
    public sealed class MinimapRenderer
    {
        #region Fields
        private static readonly ColorRgb ViewRectangleColor = Colors.Orange;
        private static readonly ColorRgb BorderColor = Colors.Gray;

        private readonly Func<Rectangle> visibleBoundsGetter;
        private readonly IMatchRenderer matchRenderer;
        private readonly UnderAttackWarningRenderer underAttackWarningRenderer;
        #endregion

        #region Constructors
        public MinimapRenderer(Func<Rectangle> visibleBoundsGetter,
            Faction localFaction, IMatchRenderer matchRenderer)
        {
            Argument.EnsureNotNull(visibleBoundsGetter, "visibleBoundsGetter");
            Argument.EnsureNotNull(localFaction, "localFaction");
            Argument.EnsureNotNull(matchRenderer, "matchRenderer");

            this.visibleBoundsGetter = visibleBoundsGetter;
            this.matchRenderer = matchRenderer;
            this.underAttackWarningRenderer = new UnderAttackWarningRenderer(localFaction);
        }
        #endregion

        #region Methods
        public void Draw(GraphicsContext context, Rectangle bounds)
        {
            matchRenderer.DrawMinimap();
            underAttackWarningRenderer.Draw(context);

            Rectangle visibleBounds = visibleBoundsGetter();
            Rectangle intersection = Rectangle.Intersection(bounds, visibleBounds).GetValueOrDefault();
            context.Stroke(intersection, ViewRectangleColor);
            context.Stroke(bounds, BorderColor);
        }
        #endregion
    }
}
