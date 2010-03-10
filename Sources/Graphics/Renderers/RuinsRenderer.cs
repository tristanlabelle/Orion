using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK.Math;
using Orion.Collections;
using Orion.Engine.Graphics;
using Orion.GameLogic;
using Orion.Geometry;

namespace Orion.Graphics.Renderers
{
    /// <summary>
    /// Provides functionality to draw the ruins left by destroyed buildings or dead units.
    /// </summary>
    public sealed class RuinsRenderer
    {
        #region Fields
        private const float maxRuinAlpha = 0.8f;
        private const float buildingRuinDurationInSeconds = 60 * 4;
        private const float skeletonDurationInSeconds = 60;
        private const float ruinFadeDurationInSeconds = 1;

        private readonly Faction faction;
        private readonly Texture buildingRuinTexture;
        private readonly Texture skeletonTexture;

        /// <remarks>
        /// OPTIM: Ruins are allocated on the heap so we pool them
        /// in order to reuse them as much as possible and avoid GC pressure.
        /// </remarks>
        private readonly Pool<Ruin> ruinPool = new Pool<Ruin>();
        private readonly List<Ruin> ruins = new List<Ruin>();
        private float simulationTimeInSeconds;
        #endregion

        #region Constructors
        public RuinsRenderer(Faction faction, GameGraphics gameGraphics)
        {
            Argument.EnsureNotNull(faction, "faction");
            Argument.EnsureNotNull(gameGraphics, "gameGraphics");

            this.faction = faction;
            this.buildingRuinTexture = gameGraphics.GetUnitTexture("Ruins");
            this.skeletonTexture = gameGraphics.GetUnitTexture("Skeleton");

            World.Updated += OnWorldUpdated;
            World.Entities.Removed += OnEntityRemoved;
        }
        #endregion

        #region Properties
        private World World
        {
            get { return faction.World; }
        }
        #endregion

        #region Methods
        #region Event Handlers
        private void OnWorldUpdated(World sender, SimulationStep args)
        {
            simulationTimeInSeconds = args.TimeInSeconds;
            ClearExpiredRuins();
        }

        private void OnEntityRemoved(EntityManager sender, Entity args)
        {
            Unit unit = args as Unit;
            if (unit == null) return;
            AddRuin(unit);
        }
        #endregion

        #region Drawing
        public void Draw(GraphicsContext graphicsContext, Rectangle viewBounds)
        {
            Argument.EnsureNotNull(graphicsContext, "graphicsContext");

            foreach (Ruin ruin in ruins)
            {
                Vector2 size = new Vector2(ruin.Size.Width, ruin.Size.Height);
                Rectangle rectangle = new Rectangle(ruin.Min, size);
                if (!Rectangle.Intersects(rectangle, viewBounds))
                    continue;

                Region gridRegion = Entity.GetGridRegion(ruin.Min, ruin.Size);
                if (!faction.CanSee(gridRegion)) continue;

                float durationInSeconds = GetDurationInSeconds(ruin.Type);
                float ageInSeconds = simulationTimeInSeconds - ruin.CreationTimeInSeconds;
                float lifetimeRemainingInSeconds = durationInSeconds - ageInSeconds;
                float alpha = lifetimeRemainingInSeconds / ruinFadeDurationInSeconds;
                if (alpha < 0) alpha = 0;
                if (alpha > maxRuinAlpha) alpha = maxRuinAlpha;

                Texture texture = ruin.Type == RuinType.Building ? buildingRuinTexture : skeletonTexture;

                ColorRgba color = new ColorRgba(ruin.Tint, alpha);
                graphicsContext.Fill(rectangle, texture, color);
            }
        }
        #endregion

        #region Adding/Removing
        private void AddRuin(Unit unit)
        {
            Ruin ruin = ruinPool.Get();
            ruin.Reset(simulationTimeInSeconds,
                unit.IsBuilding ? RuinType.Building : RuinType.Unit,
                unit.Position, unit.Size, unit.Faction);
            ruins.Add(ruin);
        }

        private void ClearExpiredRuins()
        {
            // As the ruins are added to the list in the order they appeared,
            // we only need to check the first ruin until we find one
            // which is not old enough to be removed.
            while (ruins.Count > 0)
            {
                Ruin oldestRuin = ruins[0];

                float durationInSeconds = GetDurationInSeconds(oldestRuin.Type);
                float ageInSeconds = simulationTimeInSeconds - oldestRuin.CreationTimeInSeconds;
                if (ageInSeconds < durationInSeconds) break;

                ruins.RemoveAt(0);
                ruinPool.Add(oldestRuin);
            }
        }
        #endregion

        private static float GetDurationInSeconds(RuinType ruinType)
        {
            return ruinType == RuinType.Building ? buildingRuinDurationInSeconds : skeletonDurationInSeconds;
        }
        #endregion
    }
}
