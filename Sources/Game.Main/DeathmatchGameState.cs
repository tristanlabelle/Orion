using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.Engine;
using Orion.Engine.Gui;
using Orion.Game.Matchmaking;
using Orion.Game.Matchmaking.Commands.Pipeline;
using Orion.Game.Presentation;
using Orion.Game.Presentation.Gui;
using Orion.Game.Simulation;
using Orion.Game.Simulation.Skills;
using Orion.Game.Presentation.Renderers;
using Orion.Game.Presentation.Audio;
using Orion.Engine.Geometry;
using OpenTK;
using Orion.Engine.Gui2;
using Input = Orion.Engine.Input;
using MouseButtons = System.Windows.Forms.MouseButtons;

namespace Orion.Game.Main
{
    /// <summary>
    /// Handles the initialisation, updating and clean up of the state of the game when
    /// a single player deathmatch is being played.
    /// </summary>
    public sealed class DeathmatchGameState : GameState
    {
        #region Fields
        private readonly GameGraphics graphics;
        private readonly GameAudio audio;
        private readonly Match match;
        private readonly CommandPipeline commandPipeline;
        private readonly SlaveCommander localCommander;
        private readonly UserInputManager userInputManager;
        private readonly MatchUI2 ui;
        private readonly Camera camera;
        private readonly IMatchRenderer matchRenderer;
        private readonly MatchAudioPresenter audioPresenter;
        private SimulationStep lastSimulationStep;
        #endregion

        #region Constructors
        public DeathmatchGameState(GameStateManager manager, GameGraphics graphics,
            Match match, CommandPipeline commandPipeline, SlaveCommander localCommander)
            : base(manager)
        {
            Argument.EnsureNotNull(graphics, "graphics");
            Argument.EnsureNotNull(match, "match");
            Argument.EnsureNotNull(commandPipeline, "commandPipeline");
            Argument.EnsureNotNull(localCommander, "localCommander");

            this.graphics = graphics;
            this.audio = new GameAudio();
            this.match = match;
            this.commandPipeline = commandPipeline;
            this.localCommander = localCommander;

            this.userInputManager = new UserInputManager(match, localCommander);
            this.ui = new MatchUI2(graphics.GuiStyle);
            this.ui.MinimapCameraMoved += OnMinimapCameraMoved;
            this.ui.MinimapRendering += OnMinimapRendering;
            this.ui.MouseMoved += OnViewportMouseMoved;
            this.ui.MouseButton += OnViewportMouseButton;
            this.camera = new Camera(match.World.Size, graphics.Window.ClientAreaSize);
            this.matchRenderer = new DeathmatchRenderer(userInputManager, graphics);
            this.audioPresenter = new MatchAudioPresenter(audio, userInputManager);
            this.lastSimulationStep = new SimulationStep(-1, 0, 0);

            //this.ui.QuitPressed += OnQuitPressed;
            this.match.World.EntityRemoved += OnEntityRemoved;
            this.match.World.FactionDefeated += OnFactionDefeated;
        }
        #endregion

        #region Properties
        private World World
        {
            get { return match.World; }
        }
        #endregion

        #region Methods
        protected internal override void OnEntered()
        {
            graphics.UIManager.Content = ui;
        }

        protected internal override void OnShadowed()
        {
            graphics.UIManager.Content = null;
        }

        protected internal override void OnUnshadowed()
        {
            OnEntered();
        }

        protected internal override void Update(float timeDeltaInSeconds)
        {
            if (match.IsRunning)
            {
                SimulationStep step = new SimulationStep(
                    lastSimulationStep.Number + 1,
                    lastSimulationStep.TimeInSeconds + timeDeltaInSeconds,
                    timeDeltaInSeconds);

                match.World.Update(step);

                lastSimulationStep = step;
            }

            commandPipeline.Update(lastSimulationStep);

            graphics.UpdateGui(timeDeltaInSeconds);
            camera.ViewportSize = ui.ViewportRectangle.Size;
            camera.ScrollDirection = ui.ScrollDirection;
            camera.Update(timeDeltaInSeconds);
            audioPresenter.SetViewBounds(camera.ViewBounds);
        }

        protected internal override void Draw(GameGraphics graphics)
        {
            graphics.UIManager.Arrange();
            Size clientSize = graphics.Window.ClientAreaSize;
            Region viewportRectangle = ui.ViewportRectangle;
            if (viewportRectangle.Area > 0)
            {
                Rectangle worldViewBounds = camera.ViewBounds;
                graphics.Context.ProjectionBounds = new Rectangle(
                    worldViewBounds.MinX - worldViewBounds.Width * viewportRectangle.MinX / viewportRectangle.Width,
                    worldViewBounds.MinY - worldViewBounds.Height * viewportRectangle.MinY / viewportRectangle.Height,
                    worldViewBounds.Width * clientSize.Width / viewportRectangle.Width,
                    worldViewBounds.Height * clientSize.Height / viewportRectangle.Height);
                matchRenderer.Draw(worldViewBounds);
            }

            graphics.Context.ProjectionBounds = new Rectangle(clientSize.Width, clientSize.Height);
            graphics.DrawGui();
        }

        private void OnMinimapCameraMoved(MatchUI2 sender, Vector2 normalizedPosition)
        {
            camera.Target = new Vector2(normalizedPosition.X * World.Width, normalizedPosition.Y * World.Height);
        }

        private void OnMinimapRendering(MatchUI2 sender, Region rectangle)
        {
            if (rectangle.Area == 0) return;

            Rectangle previousProjectionBounds = graphics.Context.ProjectionBounds;

            Size clientSize = graphics.Window.ClientAreaSize;

            graphics.Context.ProjectionBounds = new Rectangle(
                -World.Bounds.Width * rectangle.MinX / rectangle.Width,
                -World.Bounds.Height * rectangle.MinY / rectangle.Height,
                World.Bounds.Width * clientSize.Width / rectangle.Width,
                World.Bounds.Height * clientSize.Height / rectangle.Height);
            matchRenderer.DrawMinimap();

            graphics.Context.Stroke(camera.ViewBounds, Colors.Red);

            graphics.Context.ProjectionBounds = previousProjectionBounds;
        }

        private bool OnViewportMouseMoved(Control sender, MouseState mouseState)
        {
            Vector2 worldPosition = camera.ViewportToWorld(mouseState.Position - ui.ViewportRectangle.Min);
            Input.MouseEventArgs args = new Input.MouseEventArgs(worldPosition, Input.MouseButton.None, 0, 0);
            userInputManager.HandleMouseMove(args);
            return true;
        }

        private bool OnViewportMouseButton(Control sender, MouseState mouseState, MouseButtons button, int pressCount)
        {
            Vector2 worldPosition = camera.ViewportToWorld(mouseState.Position - ui.ViewportRectangle.Min);

            Input.MouseButton inputButton;
            if (button == MouseButtons.Left) inputButton = Input.MouseButton.Left;
            else if (button == MouseButtons.Middle) inputButton = Input.MouseButton.Middle;
            else if (button == MouseButtons.Right) inputButton = Input.MouseButton.Right;
            else return false;

            Input.MouseEventArgs args = new Input.MouseEventArgs(worldPosition, inputButton, pressCount, 0);

            if (pressCount == 0) userInputManager.HandleMouseUp(args);
            else userInputManager.HandleMouseDown(args);

            return true;
        }

        public override void Dispose()
        {
            matchRenderer.Dispose();
            audioPresenter.Dispose();
            commandPipeline.Dispose();
            audio.Dispose();
        }

        private void OnQuitPressed(MatchUI sender)
        {
            Manager.PopTo<MainMenuGameState>();
        }

        private void OnEntityRemoved(World sender, Entity entity)
        {
            Unit unit = entity as Unit;
            if (unit == null) return;

            Faction faction = unit.Faction;
            if (faction.Status == FactionStatus.Defeated) return;

            bool hasKeepAliveUnit = faction.Units.Any(u => u.IsAlive && u.Type.KeepsFactionAlive);
            if (hasKeepAliveUnit) return;
            
            faction.MarkAsDefeated();
        }

        private void OnFactionDefeated(World sender, Faction faction)
        {
            faction.MassSuicide();

            if (faction == localCommander.Faction)
            {
                audioPresenter.PlayDefeatSound();
                //ui.DisplayDefeatMessage(() => Manager.PopTo<MainMenuGameState>());
                return;
            }

            bool allEnemyFactionsDefeated = sender.Factions
                .Where(f => !localCommander.Faction.GetDiplomaticStance(f).HasFlag(DiplomaticStance.AlliedVictory))
                .All(f => f == faction || f.Status == FactionStatus.Defeated);
            if (!allEnemyFactionsDefeated) return;

            audioPresenter.PlayVictorySound();
            //ui.DisplayVictoryMessage(() => Manager.PopTo<MainMenuGameState>());
        }
        #endregion
    }
}
