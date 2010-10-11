using System;
using System.Linq;
using Orion.Engine;
using Orion.Engine.Geometry;
using Orion.Engine.Graphics;
using Orion.Game.Simulation;
using Orion.Game.Matchmaking.TowerDefense;
using Orion.Game.Simulation.Skills;
using Orion.Game.Simulation.Tasks;

namespace Orion.Game.Presentation.Renderers
{
    /// <summary>
    /// Provides methods to draw the game <see cref="World"/>.
    /// </summary>
    public sealed class WorldRenderer : IDisposable
    {
        #region Fields
        private readonly Faction faction;
        private readonly GameGraphics gameGraphics;

        private readonly TerrainRenderer terrainRenderer;
        private readonly ResourcesRenderer resourcesRenderer;
        private readonly UnitsRenderer unitsRenderer;
        private readonly ExplosionRenderer explosionRenderer;
        private readonly FogOfWarRenderer fogOfWarRenderer;
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new <see cref="WorldRenderer"/> from the <see cref="World"/> it is going to render.
        /// </summary>
        /// <param name="faction">
        /// The <see cref="Faction"/> providing the view point to the <see cref="World"/> to be rendered.
        /// </param>
        /// <param name="gameGraphics">The game graphics which provides access to graphics resources.</param>
        public WorldRenderer(Faction faction, GameGraphics gameGraphics)
        {
            Argument.EnsureNotNull(faction, "faction");
            Argument.EnsureNotNull(gameGraphics, "gameGraphics");

            this.faction = faction;
            this.gameGraphics = gameGraphics;

            this.terrainRenderer = new TerrainRenderer(World.Terrain, gameGraphics);
            this.resourcesRenderer = new ResourcesRenderer(faction, gameGraphics);
            this.unitsRenderer = new UnitsRenderer(faction, gameGraphics);
            this.explosionRenderer = new ExplosionRenderer(faction.World, gameGraphics);
            this.fogOfWarRenderer = new FogOfWarRenderer(faction);
        }
        #endregion

        #region Properties
        public Faction Faction
        {
            get { return faction; }
        }

        public Rectangle WorldBounds
        {
            get { return World.Bounds; }
        }

        public bool DrawHealthBars
        {
            get { return unitsRenderer.DrawHealthBars; }
            set { unitsRenderer.DrawHealthBars = value; }
        }

        public World World
        {
            get { return faction.World; }
        }
        #endregion

        #region Methods
        public void DrawTerrain(GraphicsContext graphicsContext, Rectangle viewBounds)
        {
            Argument.EnsureNotNull(graphicsContext, "graphicsContext");
            terrainRenderer.Draw(graphicsContext);
        }

        public void DrawMiniatureTerrain(GraphicsContext graphicsContext)
        {
            Argument.EnsureNotNull(graphicsContext, "graphicsContext");
            terrainRenderer.DrawMiniature(graphicsContext);
        }

        public void DrawResources(GraphicsContext graphicsContext, Rectangle viewBounds)
        {
            Argument.EnsureNotNull(graphicsContext, "graphicsContext");
            resourcesRenderer.Draw(graphicsContext, viewBounds);
        }

        public void DrawMiniatureResources(GraphicsContext graphicsContext)
        {
            Argument.EnsureNotNull(graphicsContext, "graphicsContext");
            resourcesRenderer.DrawMiniature(graphicsContext);
        }

        public void DrawUnits(GraphicsContext graphicsContext, Rectangle viewBounds)
        {
            Argument.EnsureNotNull(graphicsContext, "graphicsContext");
            unitsRenderer.Draw(graphicsContext, viewBounds);
        }

        public void DrawMiniatureUnits(GraphicsContext graphicsContext)
        {
            Argument.EnsureNotNull(graphicsContext, "graphicsContext");
            unitsRenderer.DrawMiniature(graphicsContext);
        }

        public void DrawExplosions(GraphicsContext graphicsContext, Rectangle viewBounds)
        {
            Argument.EnsureNotNull(graphicsContext, "graphicsContext");
            explosionRenderer.Draw(graphicsContext);
        }

        public void DrawFogOfWar(GraphicsContext graphicsContext, Rectangle viewBounds)
        {
            Argument.EnsureNotNull(graphicsContext, "graphicsContext");
            fogOfWarRenderer.Draw(graphicsContext);
        }

        public void DrawBlueprints(GraphicsContext graphicsContext, Rectangle viewBounds)
        {
            var plans = World.Entities
                .Intersecting(viewBounds)
                .OfType<Unit>()
                .Where(u => u.Faction.GetDiplomaticStance(faction).HasFlag(DiplomaticStance.SharedVision)
                    && u.HasSkill<BuildSkill>())
                .SelectMany(u => u.TaskQueue)
                .OfType<BuildTask>()
                .Select(t => t.BuildingPlan)
                .Distinct();

            ColorRgba tint = new ColorRgba(Colors.DarkBlue, 0.5f);
            foreach (BuildingPlan plan in plans)
            {
                Texture buildingTexture = gameGraphics.GetUnitTexture(plan.BuildingType.Name);
                Rectangle buildingRectangle = plan.GridRegion.ToRectangle();
                graphicsContext.Fill(buildingRectangle, buildingTexture, tint);
            }
        }

        public void Dispose()
        {
            terrainRenderer.Dispose();
            fogOfWarRenderer.Dispose();
        }
        #endregion
    }
}
