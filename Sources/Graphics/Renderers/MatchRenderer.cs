using System;
using System.Linq;
using OpenTK.Math;
using Orion.Engine.Graphics;
using Orion.Matchmaking;
using Orion.GameLogic;
using Orion.Geometry;
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

        #region Chuck Norris
        private const float secondsToShakeWhenChuckNorrisSpawns = 5;
        private float offsetX;
        private float offsetY;
        private float shakingSecondsLeft = 0;
        #endregion
        #endregion

        #region Constructors
        public MatchRenderer(UserInputManager inputManager, GameGraphics gameGraphics, CreepPath creepPath)
        {
            Argument.EnsureNotNull(inputManager, "inputManager");
            Argument.EnsureNotNull(gameGraphics, "gameGraphics");

            this.inputManager = inputManager;
            this.gameGraphics = gameGraphics;
            this.selectionRenderer = new SelectionRenderer(inputManager);
            this.worldRenderer = new WorldRenderer(inputManager.LocalFaction, gameGraphics, creepPath);
            this.minimap = new MinimapRenderer(worldRenderer);

            World world = Faction.World;
            world.Updated += UpdateShaking;
            world.Entities.Added += CheckForChuckNorris;
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
            get { return inputManager.LocalCommander.Faction; }
        }
        #endregion

        #region Methods
        public void Draw(GraphicsContext context, Rectangle bounds)
        {
            Argument.EnsureNotNull(context, "context");

            minimap.VisibleRect = bounds;

            using (context.PushTranslate(new Vector2(offsetX, offsetY)))
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
        private void CheckForChuckNorris(EntityManager manager, Entity entity)
        {
            Unit unit = entity as Unit;
            if (unit == null) return;

            if (unit.Type.Name == "Chuck Norris")
                shakingSecondsLeft += secondsToShakeWhenChuckNorrisSpawns;
        }

        private void UpdateShaking(World world, SimulationStep step)
        {
            if (shakingSecondsLeft != 0)
            {
                offsetX = (float)(world.Random.NextDouble() - 0.5) * 5;
                offsetY = (float)(world.Random.NextDouble() - 0.5) * 5;
                Console.WriteLine("{0}, {1}", offsetX, offsetY);
                shakingSecondsLeft -= step.TimeDeltaInSeconds;
                if (shakingSecondsLeft < 0)
                    shakingSecondsLeft = 0;
            }
            else
            {
                offsetX = 0;
                offsetY = 0;
            }
        }
        #endregion
        #endregion
    }
}
