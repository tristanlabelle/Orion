using System;
using OpenTK;
using Orion.Engine;
using Orion.Engine.Geometry;
using Orion.Engine.Graphics;
using Orion.Game.Simulation;
using Orion.Game.Presentation.Actions.UserCommands;

namespace Orion.Game.Presentation.Renderers
{
    /// <summary>
    /// Draws a deathmatch to the main view and minimap.
    /// </summary>
    public sealed class DeathmatchRenderer : IMatchRenderer
    {
        #region Fields
        private const float shakingDuration = 4;
        private const float shakingMagnitude = 5;
        private const float shakingOscillationsPerSecond = 40;

        private readonly UserInputManager inputManager;
        private readonly GameGraphics graphics;
        private readonly SelectionRenderer selectionRenderer;
        private readonly WorldRenderer worldRenderer;
        private float shakingSecondsLeft = 0;
        #endregion

        #region Constructors
        public DeathmatchRenderer(UserInputManager inputManager, GameGraphics graphics)
        {
            Argument.EnsureNotNull(inputManager, "inputManager");
            Argument.EnsureNotNull(graphics, "graphics");

            this.inputManager = inputManager;
            this.graphics = graphics;
            this.selectionRenderer = new SelectionRenderer(inputManager);
            this.worldRenderer = new WorldRenderer(inputManager.LocalFaction, graphics);

            World world = Faction.World;
            world.Updated += OnWorldUpdated;
            world.EntityAdded += OnEntityAdded;
        }
        #endregion

        #region Properties
        public WorldRenderer WorldRenderer
        {
            get { return worldRenderer; }
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

        private World World
        {
            get { return inputManager.World; }
        }
        #endregion

        #region Methods
        public void Draw(Rectangle visibleBounds)
        {
            GraphicsContext context = graphics.Context;

            using (context.PushTranslate(ShakeOffset))
            {
                worldRenderer.DrawTerrain(context, visibleBounds);
                worldRenderer.DrawBlueprints(context, visibleBounds);
                worldRenderer.DrawEntities(context, visibleBounds);
                selectionRenderer.DrawSelectionMarkers(context);

                if (inputManager.HoveredEntity != null && Faction.CanSee(inputManager.HoveredEntity))
                    HealthBarRenderer.Draw(context, inputManager.HoveredEntity);

                worldRenderer.DrawExplosions(context, visibleBounds);
                worldRenderer.DrawFogOfWar(context, visibleBounds);
                selectionRenderer.DrawRallyPointMarkers(context);

                IRenderableUserCommand selectedCommandRenderer = inputManager.SelectedCommand as IRenderableUserCommand;
                if (selectedCommandRenderer != null)
                    selectedCommandRenderer.Draw(context, visibleBounds);
            }

            selectionRenderer.DrawSelectionRectangle(context);
        }

        public void DrawMinimap()
        {
            worldRenderer.DrawMiniatureTerrain(graphics.Context);
            worldRenderer.DrawMiniatureEntities(graphics.Context);
            worldRenderer.DrawFogOfWar(graphics.Context, World.Bounds);
        }

        public void Dispose()
        {
            worldRenderer.Dispose();
        }

        #region Chuck Norris
        private void OnEntityAdded(World sender, Entity entity)
        {
            if (entity.Identity.Name == "ChuckNorris")
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
