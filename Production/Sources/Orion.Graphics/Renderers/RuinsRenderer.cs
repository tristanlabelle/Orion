using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.GameLogic;
using OpenTK.Math;
using Orion.Geometry;

using Color = System.Drawing.Color;

namespace Orion.Graphics.Renderers
{
    /// <summary>
    /// Provides functionality to draw the ruins left by destroyed buildings or dead units.
    /// </summary>
    public sealed class RuinsRenderer : IRenderer
    {
        #region Fields
        private const float buildingRuinDurationInSeconds = 60;
        private const float skeletonDurationInSeconds = 15;
        private const float ruinFadeDurationInSeconds = 1;

        private readonly Faction faction;
        private readonly TextureManager textureManager;

        /// <remarks>
        /// OPTIM: Ruins are allocated on the heap so we pool them
        /// in order to reuse them as much as possible and avoid GC pressure.
        /// </remarks>
        private readonly Pool<Ruin> ruinPool = new Pool<Ruin>();
        private readonly List<Ruin> ruins = new List<Ruin>();
        private float simulationTimeInSeconds;
        #endregion

        #region Constructors
        public RuinsRenderer(Faction faction, TextureManager textureManager)
        {
            Argument.EnsureNotNull(faction, "faction");
            Argument.EnsureNotNull(textureManager, "textureManager");

            this.faction = faction;
            this.textureManager = textureManager;

            World.Updated += OnWorldUpdated;
            World.Entities.Removed += OnEntityRemoved;
        }
        #endregion

        #region Properties
        private Texture BuildingRuinTexture
        {
            get { return textureManager.GetUnit("Ruins"); }
        }

        private Texture SkeletonTexture
        {
            get { return textureManager.GetUnit("Skeleton"); }
        }

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
        public void Draw(GraphicsContext graphics)
        {
            Argument.EnsureNotNull(graphics, "graphics");

            foreach (Ruin ruin in ruins)
            {
                Vector2 size = new Vector2(ruin.Size.Width, ruin.Size.Height);
                Rectangle rectangle = new Rectangle(ruin.Min, size);
                if (!Rectangle.Intersects(rectangle, graphics.CoordinateSystem))
                    continue;

                Region gridRegion = Entity.GetGridRegion(ruin.Min, ruin.Size);
                if (!faction.CanSee(gridRegion)) continue;

                float durationInSeconds = GetDurationInSeconds(ruin.Type);
                float ageInSeconds = simulationTimeInSeconds - ruin.CreationTimeInSeconds;
                float lifetimeRemainingInSeconds = durationInSeconds - ageInSeconds;
                float alpha = lifetimeRemainingInSeconds / ruinFadeDurationInSeconds;
                if (alpha < 0) alpha = 0;
                if (alpha > 1) alpha = 1;

                Texture texture = ruin.Type == RuinType.Building ? BuildingRuinTexture : SkeletonTexture;

                int alphaComponent = (int)(alpha * 255);
                Color color = Color.FromArgb(alphaComponent, ruin.Tint);

                graphics.Fill(rectangle, texture, color);
            }
        }
        #endregion

        #region Adding/Removing
        private void AddRuin(Unit unit)
        {
            Ruin ruin = ruinPool.Get();
            ruin.Reset(simulationTimeInSeconds,
                unit.IsBuilding ? RuinType.Building : RuinType.Unit,
                unit.Position, unit.Size, unit.Faction.Color);
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
