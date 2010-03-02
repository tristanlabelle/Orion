using System;
using System.Linq;
using Orion.Engine.Graphics;
using Orion.GameLogic;
using Orion.Geometry;
using Orion.Matchmaking.TowerDefense;

namespace Orion.Graphics.Renderers
{
    /// <summary>
    /// Provides methods to draw the game <see cref="World"/>.
    /// </summary>
    public sealed class WorldRenderer : IDisposable
    {
        #region Fields
        private readonly Faction faction;
        private readonly TextureManager textureManager;

        private readonly TerrainRenderer terrainRenderer;
        private readonly ResourcesRenderer resourcesRenderer;
        private readonly RuinsRenderer ruinsRenderer;
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
        public WorldRenderer(Faction faction, TextureManager textureManager)
        {
            Argument.EnsureNotNull(faction, "faction");
            Argument.EnsureNotNull(textureManager, "textureManager");

            this.faction = faction;
            this.textureManager = textureManager;

            this.terrainRenderer = new TerrainRenderer(World.Terrain, textureManager);
            this.resourcesRenderer = new ResourcesRenderer(faction, textureManager);
            this.ruinsRenderer = new RuinsRenderer(faction, textureManager);
            this.unitsRenderer = new UnitsRenderer(faction, textureManager);
            this.explosionRenderer = new ExplosionRenderer(faction.World, textureManager);
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

        public void DrawMiniatureTerrain(GraphicsContext graphicsContext, Rectangle viewBounds)
        {
            Argument.EnsureNotNull(graphicsContext, "graphicsContext");
            terrainRenderer.DrawMiniature(graphicsContext);
        }

        public void DrawResources(GraphicsContext graphicsContext, Rectangle viewBounds)
        {
            Argument.EnsureNotNull(graphicsContext, "graphicsContext");
            resourcesRenderer.Draw(graphicsContext, viewBounds);
        }

        public void DrawMiniatureResources(GraphicsContext graphicsContext, Rectangle viewBounds)
        {
            Argument.EnsureNotNull(graphicsContext, "graphicsContext");
            resourcesRenderer.DrawMiniature(graphicsContext, viewBounds);
        }

        public void DrawUnits(GraphicsContext graphicsContext, Rectangle viewBounds)
        {
            Argument.EnsureNotNull(graphicsContext, "graphicsContext");
            ruinsRenderer.Draw(graphicsContext, viewBounds);
            unitsRenderer.Draw(graphicsContext, viewBounds);
        }

        public void DrawMiniatureUnits(GraphicsContext graphicsContext, Rectangle viewBounds)
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

        public void Dispose()
        {
            terrainRenderer.Dispose();
            fogOfWarRenderer.Dispose();
        }
        #endregion
    }
}
