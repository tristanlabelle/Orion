using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;
using Orion.Engine;
using Orion.Engine.Collections;
using Orion.Engine.Geometry;
using Orion.Engine.Graphics;
using Orion.Game.Simulation;
using Orion.Game.Simulation.Utilities;
using Orion.Game.Simulation.Components;

namespace Orion.Game.Presentation.Renderers
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
        #endregion

        #region Constructors
        public RuinsRenderer(Faction faction, GameGraphics gameGraphics)
        {
            Argument.EnsureNotNull(faction, "faction");
            Argument.EnsureNotNull(gameGraphics, "gameGraphics");

            this.faction = faction;
            this.buildingRuinTexture = gameGraphics.GetUnitTexture("Ruins");
            this.skeletonTexture = gameGraphics.GetUnitTexture("Skeleton");
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

            foreach (Entity ruin in faction.World.Entities.Where(e => e.Components.Has<TimedExistence>()))
            {
                Region gridRegion = ruin.Components.Get<Spatial>().GridRegion;
                Rectangle rectangle = gridRegion.ToRectangle();
                if (!Rectangle.Intersects(rectangle, viewBounds))
                    continue;

                if (!faction.CanSee(gridRegion)) continue;

                TimedExistence timeout = ruin.Components.Get<TimedExistence>();
                Identity identity = ruin.Components.Get<Identity>();

                ColorRgb factionColor = ruin.Components.Has<FactionMembership>()
                    ? ruin.Components.Get<FactionMembership>().Faction.Color
                    : Colors.White;

                float alpha = timeout.TimeLeft / ruinFadeDurationInSeconds;
                if (alpha < 0) alpha = 0;
                if (alpha > maxRuinAlpha) alpha = maxRuinAlpha;

                Texture texture = identity.TrainType == TrainType.OnSite ? buildingRuinTexture : skeletonTexture;

                ColorRgba color = new ColorRgba(factionColor, alpha);
                graphicsContext.Fill(rectangle, texture, color);
            }
        }
        #endregion
    }
}
