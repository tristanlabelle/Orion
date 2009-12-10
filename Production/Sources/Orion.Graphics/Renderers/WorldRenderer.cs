using System;
using System.Linq;
using Orion.GameLogic;
using Orion.Geometry;

using Color = System.Drawing.Color;

namespace Orion.Graphics.Renderers
{
    /// <summary>
    /// Provides methods to draw the game <see cref="World"/>.
    /// </summary>
    public sealed class WorldRenderer : IDisposable
    {
        #region Fields
        private static readonly Color AladdiumColor = Color.LightBlue;
        private static readonly Color AlageneColor = Color.Green;

        private readonly Faction faction;
        private readonly TextureManager textureManager;

        private readonly TerrainRenderer terrainRenderer;
        private readonly ResourcesRenderer resourcesRenderer;
        private readonly RuinsRenderer ruinsRenderer;
        private readonly UnitsRenderer unitsRenderer;
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
            this.fogOfWarRenderer = new FogOfWarRenderer(faction);
        }
        #endregion

        #region Properties
        public Rectangle WorldBounds
        {
            get { return World.Bounds; }
        }

        public bool DrawHealthBars
        {
            get { return unitsRenderer.DrawHealthBars; }
            set { unitsRenderer.DrawHealthBars = value; }
        }

        private World World
        {
            get { return faction.World; }
        }
        #endregion

        #region Methods
        #region Terrain
        public void DrawTerrain(GraphicsContext graphics)
        {
            Argument.EnsureNotNull(graphics, "graphics");
            terrainRenderer.Draw(graphics);
        }

        public void DrawMiniatureTerrain(GraphicsContext graphics)
        {
            Argument.EnsureNotNull(graphics, "graphics");
            terrainRenderer.DrawMiniature(graphics);
        }
        #endregion

        #region Resources
        public void DrawResources(GraphicsContext graphics)
        {
            Argument.EnsureNotNull(graphics, "graphics");
            resourcesRenderer.Draw(graphics);
        }

        public void DrawMiniatureResources(GraphicsContext graphics)
        {
            Argument.EnsureNotNull(graphics, "graphics");
            resourcesRenderer.DrawMiniature(graphics);
        }
        #endregion

        #region Units
        public void DrawUnits(GraphicsContext graphics)
        {
            Argument.EnsureNotNull(graphics, "graphics");
            ruinsRenderer.Draw(graphics);
            unitsRenderer.Draw(graphics);
        }

        public void DrawMiniatureUnits(GraphicsContext graphics)
        {
            Argument.EnsureNotNull(graphics, "graphics");
            unitsRenderer.DrawMiniature(graphics);
        }
        #endregion

        #region Fog of War
        public void DrawFogOfWar(GraphicsContext graphics)
        {
            Argument.EnsureNotNull(graphics, "graphics");
            fogOfWarRenderer.Draw(graphics);
        }
        #endregion

        public void Dispose()
        {
            terrainRenderer.Dispose();
            fogOfWarRenderer.Dispose();
        }
        #endregion
    }
}
