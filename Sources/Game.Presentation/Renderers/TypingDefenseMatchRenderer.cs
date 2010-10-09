using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.Engine.Graphics;
using Orion.Engine.Geometry;
using Orion.Game.Simulation;
using Orion.Engine.Gui;
using Orion.Engine;
using Orion.Game.Matchmaking.TowerDefense;
using OpenTK;

namespace Orion.Game.Presentation.Renderers
{
    public sealed class TypingDefenseMatchRenderer : IMatchRenderer
    {
        #region Fields
        private readonly UserInputManager inputManager;
        private readonly GameGraphics graphics;
        private readonly TypingCreepCommander creepCommander;
        private readonly SelectionRenderer selectionRenderer;
        private readonly WorldRenderer worldRenderer;
        private readonly CreepPathRenderer creepPathRenderer;
        #endregion

        #region Constructors
        public TypingDefenseMatchRenderer(UserInputManager inputManager, GameGraphics graphics,
            TypingCreepCommander creepCommander)
        {
            Argument.EnsureNotNull(inputManager, "inputManager");
            Argument.EnsureNotNull(graphics, "graphics");
            Argument.EnsureNotNull(creepCommander, "creepCommander");

            this.inputManager = inputManager;
            this.graphics = graphics;
            this.creepCommander = creepCommander;
            this.selectionRenderer = new SelectionRenderer(inputManager);
            this.worldRenderer = new WorldRenderer(inputManager.LocalFaction, graphics);
            this.creepPathRenderer = new CreepPathRenderer(creepCommander.Path, graphics);
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

        private World World
        {
            get { return inputManager.World; }
        }
        #endregion

        #region Methods
        public void Draw(Rectangle visibleBounds)
        {
            GraphicsContext context = graphics.Context;

            worldRenderer.DrawTerrain(context, visibleBounds);
            creepPathRenderer.Draw(context, visibleBounds);
            worldRenderer.DrawResources(context, visibleBounds);
            worldRenderer.DrawUnits(context, visibleBounds);
            DrawCreepPhrases();
            selectionRenderer.DrawSelectionMarkers(context);

            if (inputManager.HoveredUnit != null && Faction.CanSee(inputManager.HoveredUnit))
                HealthBarRenderer.Draw(context, inputManager.HoveredUnit);

            worldRenderer.DrawExplosions(context, visibleBounds);
            worldRenderer.DrawFogOfWar(context, visibleBounds);

            IViewRenderer selectedCommandRenderer = inputManager.SelectedCommand as IViewRenderer;
            if (selectedCommandRenderer != null)
                selectedCommandRenderer.Draw(context, visibleBounds);

            selectionRenderer.DrawSelectionRectangle(context);
        }

        private void DrawCreepPhrases()
        {
            const float textScale = 0.05f;
            foreach (Unit creep in creepCommander.Faction.Units)
            {
                CreepPhrase phrase = creepCommander.GetCreepPhrase(creep);

                Text text = new Text(phrase.Text);
                Text typedText = new Text(phrase.Text.Substring(0, phrase.TypedCharacterCount));

                Vector2 size = text.Frame.Size;
                Vector2 extent = size * 0.5f;

                using (graphics.Context.PushTransform(creep.Center, 0, textScale))
                {
                    float offsetY = 5;
                    Rectangle rectangle = new Rectangle(-extent.X, offsetY, extent.X * 2, extent.Y * 2);
                    graphics.Context.FillRoundedRectangle(rectangle, 1, Colors.Black);
                    graphics.Context.Draw(text.Value,
                        new Vector2(-extent.X + 1, offsetY),
                        phrase.IsFocused ? Colors.Cyan : Colors.Red);
                    graphics.Context.Draw(typedText.Value,
                        new Vector2(-extent.X + 1, offsetY),
                        phrase.IsFocused ? Colors.White : Colors.Orange);
                    graphics.Context.StrokeRoundedRectangle(rectangle, 1, phrase.IsFocused ? Colors.Yellow : Colors.White);
                }
            }
        }

        public void DrawMinimap()
        {
            worldRenderer.DrawMiniatureTerrain(graphics.Context);
            creepPathRenderer.Draw(graphics.Context, World.Bounds);
            worldRenderer.DrawMiniatureResources(graphics.Context);
            worldRenderer.DrawMiniatureUnits(graphics.Context);
            worldRenderer.DrawFogOfWar(graphics.Context, World.Bounds);
        }

        public void Dispose()
        {
            worldRenderer.Dispose();
        }
        #endregion
    }
}
