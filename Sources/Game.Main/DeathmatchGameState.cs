using System;
using System.Linq;
using OpenTK;
using Orion.Engine;
using Orion.Engine.Collections;
using Orion.Engine.Data;
using Orion.Engine.Geometry;
using Orion.Engine.Gui2;
using Orion.Game.Matchmaking;
using Orion.Game.Matchmaking.Commands.Pipeline;
using Orion.Game.Presentation;
using Orion.Game.Presentation.Actions;
using Orion.Game.Presentation.Audio;
using Orion.Game.Presentation.Gui;
using Orion.Game.Presentation.Renderers;
using Orion.Game.Simulation;
using Orion.Game.Simulation.Utilities;
using Input = Orion.Engine.Input;

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
        private readonly SingleEntitySelectionPanel singleEntitySelectionPanel;
        private readonly ActionPanel actionPanel;
        private readonly WorkerActivityMonitor workerActivityMonitor;
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
            this.userInputManager.Selection.Changed += OnSelectionChanged;

            this.ui = new MatchUI2(graphics.GuiStyle);
            this.ui.MinimapCameraMoved += OnMinimapCameraMoved;
            this.ui.MinimapRightClicked += OnMinimapRightClicked;
            this.ui.MinimapRendering += OnMinimapRendering;
            this.ui.MouseMoved += OnViewportMouseMoved;
            this.ui.MouseButton += OnViewportMouseButton;
            this.ui.SelectingIdleWorkers += OnSelectingIdleWorkers;
            this.ui.ViewportZoomed += OnViewportZoomed;
            this.ui.KeyEvent += OnViewportKeyEvent;
            this.ui.Chatted += (sender, message) => userInputManager.LaunchChatMessage(message);

            this.singleEntitySelectionPanel = new SingleEntitySelectionPanel(graphics);

            this.actionPanel = new ActionPanel(ui);
            
            this.workerActivityMonitor = new WorkerActivityMonitor(localCommander.Faction);

            Binding.CreateOneWay(() => localCommander.Faction.AladdiumAmount, () => ui.AladdiumAmount);
            Binding.CreateOneWay(() => localCommander.Faction.AlageneAmount, () => ui.AlageneAmount);
            Binding.CreateOneWay(() => localCommander.Faction.UsedFoodAmount, () => ui.UsedFoodAmount);
            Binding.CreateOneWay(() => localCommander.Faction.MaxFoodAmount, () => ui.FoodLimit);
            Binding.CreateOneWay(() => workerActivityMonitor.InactiveWorkerCount, () => ui.IdleWorkerCount);

            this.camera = new Camera(match.World.Size, graphics.Window.ClientAreaSize);
            this.matchRenderer = new DeathmatchRenderer(userInputManager, graphics);
            this.audioPresenter = new MatchAudioPresenter(audio, userInputManager);
            this.lastSimulationStep = new SimulationStep(-1, 0, 0);

            //this.ui.QuitPressed += OnQuitPressed;
            this.match.FactionMessageReceived += OnFactionMessageReceived;
            this.match.World.EntityRemoved += OnEntityRemoved;
            this.match.World.FactionDefeated += OnFactionDefeated;
        }
        #endregion

        #region Properties
        private World World
        {
            get { return match.World; }
        }

        private Selection Selection
        {
            get { return userInputManager.Selection; }
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

            UpdateCamera(timeDeltaInSeconds);
            audioPresenter.SetViewBounds(camera.ViewBounds);
        }

        private void UpdateCamera(float timeDeltaInSeconds)
        {
            camera.ViewportSize = ui.ViewportRectangle.Size;

            Entity firstSelectedEntity = Selection.FirstOrDefault();
            if (ui.IsFollowingSelection && firstSelectedEntity != null)
            {
                camera.ScrollDirection = Point.Zero;
                camera.Target = firstSelectedEntity.Position;
            }
            else
            {
                camera.ScrollDirection = ui.ScrollDirection;
            }

            camera.Update(timeDeltaInSeconds);
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

        private void OnSelectionChanged(Selection selection)
        {
            actionPanel.Clear();
            ui.ClearActionButtons();
            
            if (selection.Count == 1)
            {
            	Entity entity = selection.Single();
                singleEntitySelectionPanel.Entity = entity;
                ui.SelectionInfoPanel = singleEntitySelectionPanel;
                
                Unit unit = entity as Unit;
                if (unit != null)
                {
                	actionPanel.Push(new UnitActionProvider(actionPanel, userInputManager, graphics, unit.Type));
                }
            }
            else
            {
                ui.SelectionInfoPanel = null;
                singleEntitySelectionPanel.Entity = null;
            }
        }

        private void OnMinimapCameraMoved(MatchUI2 sender, Vector2 normalizedPosition)
        {
            camera.Target = new Vector2(normalizedPosition.X * World.Width, normalizedPosition.Y * World.Height);
        }

        private void OnMinimapRightClicked(MatchUI2 sender, Vector2 normalizedPosition)
        {
            Vector2 worldPosition = new Vector2(normalizedPosition.X * World.Width, normalizedPosition.Y * World.Height);
            Input.MouseEventArgs args = new Input.MouseEventArgs(worldPosition, Input.MouseButton.Right, 1, 0);
            userInputManager.HandleMouseDown(args);
            args = new Input.MouseEventArgs(worldPosition, Input.MouseButton.Right, 0, 0);
            userInputManager.HandleMouseUp(args);
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

        private void OnSelectingIdleWorkers(MatchUI2 sender, bool all)
        {
            if (workerActivityMonitor.InactiveWorkerCount == 0) return;

            var inactiveWorkers = workerActivityMonitor.InactiveWorkers;

            if (all)
            {
                Selection.Set(inactiveWorkers);
                camera.Target = inactiveWorkers.First().Position;
            }
            else
            {
                Unit unitToSelect;
                if (Selection.Type == SelectionType.Units && Selection.Count == 1)
                {
                    Unit selectedUnit = Selection.Units.First();
                    int index = (inactiveWorkers.IndexOf(selectedUnit) + 1) % workerActivityMonitor.InactiveWorkerCount;
                    unitToSelect = inactiveWorkers.ElementAt(index);
                }
                else
                {
                    unitToSelect = inactiveWorkers.First();
                }

                Selection.Set(unitToSelect);
                camera.Target = unitToSelect.Position;
            }
        }

        private bool OnViewportMouseMoved(Control sender, MouseEvent @event)
        {
            Vector2 worldPosition = camera.ViewportToWorld(@event.Position - ui.ViewportRectangle.Min);
            Input.MouseEventArgs args = new Input.MouseEventArgs(worldPosition, Input.MouseButton.None, 0, 0);
            userInputManager.HandleMouseMove(args);
            return true;
        }

        private void OnViewportZoomed(MatchUI2 sender, float delta)
        {
            if (delta >= 1) camera.ZoomIn();
            if (delta <= -1) camera.ZoomOut();
        }

        private bool OnViewportMouseButton(Control sender, MouseEvent @event)
        {
            if (@event.Button == MouseButtons.Left)
            {
                if (@event.IsPressed) sender.AcquireMouseCapture();
                else sender.ReleaseMouseCapture();
            }

            Vector2 worldPosition = camera.ViewportToWorld(@event.Position - ui.ViewportRectangle.Min);

            Input.MouseButton inputButton;
            if (@event.Button == MouseButtons.Left) inputButton = Input.MouseButton.Left;
            else if (@event.Button == MouseButtons.Middle) inputButton = Input.MouseButton.Middle;
            else if (@event.Button == MouseButtons.Right) inputButton = Input.MouseButton.Right;
            else return false;

            Input.MouseEventArgs args = new Input.MouseEventArgs(worldPosition, inputButton, @event.IsPressed ? 1 : 0, 0);

            if (@event.IsPressed) userInputManager.HandleMouseDown(args);
            else userInputManager.HandleMouseUp(args);

            return true;
        }

        private bool OnViewportKeyEvent(Control sender, KeyEvent @event)
        {
            var keys = Input.InputEnums.GetFormsKeys(@event.Key);
            var modifierKeys = Input.InputEnums.GetFormsModifierKeys(@event.ModifierKeys);

            Input.KeyboardEventArgs args = new Input.KeyboardEventArgs(keys | modifierKeys);

            if (@event.IsDown) userInputManager.HandleKeyDown(args);
            else userInputManager.HandleKeyUp(args);

            return true;
        }

        public override void Dispose()
        {
            matchRenderer.Dispose();
            audioPresenter.Dispose();
            commandPipeline.Dispose();
            audio.Dispose();
        }

        private void OnQuitPressed(MatchUI2 sender)
        {
            Manager.PopTo<MainMenuGameState>();
        }

        private void OnFactionMessageReceived(Match match, FactionMessage message)
        {
            ui.AddMessage(message.Text, message.Sender.Color);
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
