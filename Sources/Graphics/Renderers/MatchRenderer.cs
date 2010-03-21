using System;
using System.Linq;
using OpenTK.Math;
using Orion.Engine;
using Orion.Engine.Graphics;
using Orion.Matchmaking;
using Orion.GameLogic;
using Orion.Engine.Geometry;
using Orion.Matchmaking.TowerDefense;

namespace Orion.Graphics.Renderers
{
    public sealed class MatchRenderer : IRenderer, IDisposable
    {
        #region Fields
        private readonly UserInputManager inputManager;
        private readonly GameGraphics gameGraphics;
        private readonly SelectionRenderer selectionRenderer;
        private readonly WorldRenderer worldRenderer;
        private readonly MinimapRenderer minimap;

        #region Camera
        private const float shakingDuration = 4;
        private const float shakingMagnitude = 5;
        private const float shakingOscillationsPerSecond = 40;

        private float shakingSecondsLeft = 0;
        #endregion
        #endregion

        #region Constructors
        public MatchRenderer(UserInputManager inputManager, GameGraphics gameGraphics)
        {
            Argument.EnsureNotNull(inputManager, "inputManager");
            Argument.EnsureNotNull(gameGraphics, "gameGraphics");

            this.inputManager = inputManager;
            this.gameGraphics = gameGraphics;
            this.selectionRenderer = new SelectionRenderer(inputManager);
            this.worldRenderer = new WorldRenderer(inputManager.LocalFaction, gameGraphics);
            this.minimap = new MinimapRenderer(worldRenderer);

            World world = Faction.World;
            world.Updated += OnWorldUpdated;
            world.Entities.Added += OnEntityAdded;
        }
        #endregion

        #region Properties
        public MinimapRenderer MinimapRenderer
        {
            get { return minimap; }
        }

        public WorldRenderer WorldRenderer
        {
            get { return worldRenderer; }
        }

        public bool DrawAllHealthBars
        {
            get { return worldRenderer.DrawHealthBars; }
            set { worldRenderer.DrawHealthBars = value; }
        }

        private SelectionManager SelectionManager
        {
            get { return inputManager.SelectionManager; }
        }

        private Faction Faction
        {
            get { return inputManager.LocalFaction; }
        }

        /// <summary>
        /// Gets the camera offset due to screen shaking.
        /// </summary>
        private Vector2 ShakeOffset
        {
            get
            {
                if (shakingSecondsLeft == 0) return Vector2.Zero;
                return new Vector2(
                    (float)Math.Cos(shakingSecondsLeft * (shakingOscillationsPerSecond * 0.8f)),
                    (float)Math.Sin(shakingSecondsLeft * (shakingOscillationsPerSecond + 1.2f)))
                     * shakingMagnitude * (shakingSecondsLeft / shakingDuration);
            }
        }
        #endregion

        #region Methods
        public void Draw(GraphicsContext context, Rectangle bounds)
        {
            Argument.EnsureNotNull(context, "context");

            minimap.VisibleRect = bounds;

            using (context.PushTranslate(ShakeOffset))
            {
                worldRenderer.DrawTerrain(context, bounds);
                worldRenderer.DrawResources(context, bounds);
                worldRenderer.DrawUnits(context, bounds);
                selectionRenderer.DrawSelectionMarkers(context);

                if (inputManager.HoveredUnit != null && Faction.CanSee(inputManager.HoveredUnit))
                    HealthBarRenderer.Draw(context, inputManager.HoveredUnit);

                worldRenderer.DrawExplosions(context, bounds);
                worldRenderer.DrawFogOfWar(context, bounds);

                IRenderer selectedCommandRenderer = inputManager.SelectedCommand as IRenderer;
                if (selectedCommandRenderer != null)
                    selectedCommandRenderer.Draw(context, bounds);
            }

            selectionRenderer.DrawSelectionRectangle(context);
        }

        public void Dispose()
        {
            worldRenderer.Dispose();
        }

        #region Chuck Norris
        private void OnEntityAdded(EntityManager manager, Entity entity)
        {
            Unit unit = entity as Unit;
            if (unit == null) return;

            if (unit.Type.Name == "Chuck Norris")
                shakingSecondsLeft = shakingDuration;
        }

        private void OnWorldUpdated(World world, SimulationStep step)
        {
            shakingSecondsLeft -= step.TimeDeltaInSeconds;
            if (shakingSecondsLeft < 0) shakingSecondsLeft = 0;
        }
        #endregion
        #endregion
    }
}
