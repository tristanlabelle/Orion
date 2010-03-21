using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK.Math;
using Orion.Engine;
using Orion.Engine.Collections;
using Orion.Engine.Geometry;
using Orion.Engine.Graphics;
using Orion.GameLogic;
using Orion.GameLogic.Utilities;

namespace Orion.Graphics.Renderers
{
    /// <summary>
    /// Provides functionality to draw the ruins left by destroyed buildings or dead units.
    /// </summary>
    public sealed class RuinsRenderer
    {
        #region Fields
        private const float maxRuinAlpha = 0.8f;
        private const float ruinFadeDurationInSeconds = 1;

        private readonly Faction faction;
        private readonly Texture buildingRuinTexture;
        private readonly Texture skeletonTexture;
        private readonly RuinsMonitor monitor;
        #endregion

        #region Constructors
        public RuinsRenderer(Faction faction, GameGraphics gameGraphics)
        {
            Argument.EnsureNotNull(faction, "faction");
            Argument.EnsureNotNull(gameGraphics, "gameGraphics");

            this.faction = faction;
            this.buildingRuinTexture = gameGraphics.GetUnitTexture("Ruins");
            this.skeletonTexture = gameGraphics.GetUnitTexture("Skeleton");
            this.monitor = new RuinsMonitor(faction.World);
        }
        #endregion

        #region Properties
        private World World
        {
            get { return faction.World; }
        }
        #endregion

        #region Methods
        public void Draw(GraphicsContext graphicsContext, Rectangle viewBounds)
        {
            Argument.EnsureNotNull(graphicsContext, "graphicsContext");

            foreach (Ruin ruin in monitor.Ruins)
            {
                Rectangle rectangle = ruin.Rectangle;
                if (!Rectangle.Intersects(rectangle, viewBounds))
                    continue;

                Region gridRegion = new Region(
                    (int)rectangle.MinX, (int)rectangle.MinY,
                    (int)rectangle.Width, (int)rectangle.Height);

                if (!faction.CanSee(gridRegion)) continue;

                float alpha = ruin.RemainingTimeToLive / ruinFadeDurationInSeconds;
                if (alpha < 0) alpha = 0;
                if (alpha > maxRuinAlpha) alpha = maxRuinAlpha;

                Texture texture = ruin.WasBuilding ? buildingRuinTexture : skeletonTexture;

                ColorRgba color = new ColorRgba(ruin.FactionColor, alpha);
                graphicsContext.Fill(rectangle, texture, color);
            }
        }
        #endregion
    }
}
