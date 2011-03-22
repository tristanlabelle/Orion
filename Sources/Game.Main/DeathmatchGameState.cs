using System;
using System.Linq;
using OpenTK;
using Orion.Engine;
using Orion.Engine.Collections;
using Orion.Engine.Data;
using Orion.Engine.Geometry;
using Orion.Engine.Gui;
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
using Orion.Game.Simulation.Components;

namespace Orion.Game.Main
{
    /// <summary>
    /// Handles the initialisation, updating and clean up of the state of the game when
    /// a single player deathmatch is being played.
    /// </summary>
    public sealed class DeathmatchGameState : GameState
    {
        #region Fields
        private readonly Match match;
        private readonly CommandPipeline commandPipeline;
        private readonly SlaveCommander localCommander;
        private readonly UserInputManager userInputManager;
        private readonly MatchUI ui;
        private readonly SingleEntitySelectionPanel singleEntitySelectionPanel;
        private readonly MultipleUnitSelectionPanel multipleUnitSelectionPanel;
        private readonly ActionPanel actionPanel;
        private readonly WorkerActivityMonitor workerActivityMonitor;
        private readonly Camera camera;
        private readonly IMatchRenderer matchRenderer;
        private readonly MatchAudioPresenter audioPresenter;
        private SimulationStep lastSimulationStep;
        #endregion

        #region Constructors
        public DeathmatchGameState(GameStateManager manager, Match match,
            CommandPipeline commandPipeline, SlaveCommander localCommander)
            : base(manager)
        {
            Argument.EnsureNotNull(match, "match");
            Argument.EnsureNotNull(commandPipeline, "commandPipeline");
            Argument.EnsureNotNull(localCommander, "localCommander");

            this.match = match;
            this.match.FactionMessageReceived += OnFactionMessageReceived;
            this.match.CheatUsed += OnCheatUsed;
            this.match.World.DiplomaticStanceChanged += OnDiplomaticStanceChanged;
            this.match.World.EntityRemoved += OnEntityRemoved;
            this.match.World.FactionDefeated += OnFactionDefeated;

            this.commandPipeline = commandPipeline;

            this.localCommander = localCommander;
            this.localCommander.Faction.Warning += OnLocalFactionWarning;

            this.userInputManager = new UserInputManager(match, localCommander);
            this.userInputManager.Selection.Changed += OnSelectionChanged;
            this.userInputManager.SelectionManager.FocusedPrototypeChanged += OnFocusedUnitTypeChanged;

            this.ui = new MatchUI(Graphics, userInputManager.LocalFaction, Localizer);
            this.ui.MinimapCameraMoved += OnMinimapCameraMoved;
            this.ui.MinimapRightClicked += OnMinimapRightClicked;
            this.ui.MinimapRendering += OnMinimapRendering;
            this.ui.MouseMoved += OnViewportMouseMoved;
            this.ui.MouseButton += OnViewportMouseButton;
            this.ui.SelectingIdleWorkers += OnSelectingIdleWorkers;
            this.ui.ViewportZoomed += OnViewportZoomed;
            this.ui.KeyEvent += OnViewportKeyEvent;
            this.ui.Chatted += OnChatted;
            this.ui.DiplomaticStanceChanged += (sender, targetFaction, newStance) => userInputManager.LaunchChangeDiplomacy(targetFaction, newStance);
            this.ui.Paused += sender => match.Pause();
            this.ui.Resumed += sender => match.Resume();
            this.ui.Exited += OnUserExited;

            this.singleEntitySelectionPanel = new SingleEntitySelectionPanel(Graphics, Localizer);
            this.singleEntitySelectionPanel.TaskCancelled += (sender, task) => userInputManager.LaunchCancelTask(task);

            this.multipleUnitSelectionPanel = new MultipleUnitSelectionPanel(Graphics);
            this.multipleUnitSelectionPanel.EntityFocused += OnMultipleSelectionPanelUnitSelected;
            this.multipleUnitSelectionPanel.EntityDeselected += OnMultipleSelectionPanelUnitDeselected;

            this.actionPanel = new ActionPanel(ui);
            
            this.workerActivityMonitor = new WorkerActivityMonitor(localCommander.Faction);

            Binding.CreateOneWay(() => localCommander.Faction.AladdiumAmount, () => ui.AladdiumAmount);
            Binding.CreateOneWay(() => localCommander.Faction.AlageneAmount, () => ui.AlageneAmount);
            Binding.CreateOneWay(() => localCommander.Faction.UsedFoodAmount, () => ui.UsedFoodAmount);
            Binding.CreateOneWay(() => localCommander.Faction.MaxFoodAmount, () => ui.FoodLimit);
            Binding.CreateOneWay(() => workerActivityMonitor.InactiveWorkerCount, () => ui.IdleWorkerCount);

            this.camera = new Camera(match.World.Size, Graphics.Window.ClientAreaSize);

            // Center the camera on the player's largest unit, favoring buildings
            Entity cameraTargetEntity = localCommander.Faction.Entities
                .WithMaxOrDefault(entity => (entity.Identity.IsBuilding ? 1000 : 0) + entity.Size.Area);
            if (cameraTargetEntity != null) this.camera.Target = cameraTargetEntity.Spatial.Center;

            this.matchRenderer = new DeathmatchRenderer(userInputManager, Graphics);
            this.audioPresenter = new MatchAudioPresenter(Audio, userInputManager);
            this.lastSimulationStep = new SimulationStep(-1, 0, 0);
        }
        #endregion

        #region Properties
        private World World
        {
            get { return match.World; }
        }

        private Faction LocalFaction
        {
            get { return userInputManager.LocalFaction; }
        }

        private Selection Selection
        {
            get { return userInputManager.Selection; }
        }

        private SelectionManager SelectionManager
        {
            get { return userInputManager.SelectionManager; }
        }
        #endregion

        #region Methods
        protected internal override void OnEntered()
        {
            Graphics.UIManager.Content = ui;
        }

        protected internal override void OnShadowed()
        {
            Graphics.UIManager.Content = null;
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

            Graphics.UpdateGui(timeDeltaInSeconds);

            UpdateCamera(timeDeltaInSeconds);
            audioPresenter.SetViewBounds(camera.ViewBounds);
        }

        private void UpdateCamera(float timeDeltaInSeconds)
       { 
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

        protected internal override void Draw(GameGraphics Graphics)
        {
            Graphics.UIManager.Arrange();

            Size clientSize = Graphics.Window.ClientAreaSize;
            Region viewportRectangle = ui.ViewportRectangle;
            if (viewportRectangle.Area > 0)
            {
                camera.ViewportSize = ui.ViewportRectangle.Size;
                Rectangle worldViewBounds = camera.ViewBounds;

                Graphics.Context.ProjectionBounds = new Rectangle(
                    worldViewBounds.MinX - worldViewBounds.Width * viewportRectangle.MinX / viewportRectangle.Width,
                    worldViewBounds.MinY - worldViewBounds.Height * viewportRectangle.MinY / viewportRectangle.Height,
                    worldViewBounds.Width * clientSize.Width / viewportRectangle.Width,
                    worldViewBounds.Height * clientSize.Height / viewportRectangle.Height);
                matchRenderer.WorldRenderer.DrawHealthBars = ui.IsDisplayingHealthBars;
                matchRenderer.Draw(worldViewBounds);
            }

            Graphics.Context.ProjectionBounds = new Rectangle(clientSize.Width, clientSize.Height);
            Graphics.DrawGui();
        }

        private void UpdateActionPanel()
        {
            actionPanel.Clear();
            ui.ClearActionButtons();

            bool allSelectedUnitsControllable = Selection
                .All(entity => 
                {
                    Faction entityFaction = FactionMembership.GetFaction(entity);
                    return entityFaction != null && entityFaction.GetDiplomaticStance(LocalFaction).HasFlag(DiplomaticStance.SharedControl);
                });
            if (SelectionManager.FocusedPrototype == null || !allSelectedUnitsControllable) return;

            var actionProvider = new UnitActionProvider(actionPanel, userInputManager,
                Graphics, Localizer, SelectionManager.FocusedPrototype);
            actionPanel.Push(actionProvider);
        }

        private void OnSelectionChanged(Selection selection)
        {
            ui.SelectionInfoPanel = null;
            singleEntitySelectionPanel.Clear();
            multipleUnitSelectionPanel.ClearEntities();

            if (selection.Count == 0)
            {
                actionPanel.Clear();
                ui.ClearActionButtons();
            }
            else if (selection.Count == 1)
            {
                Entity entity = selection.Single();

                if (entity.Components.Has<Harvestable>())
                {
                    singleEntitySelectionPanel.ShowResourceNode(entity);
                    ui.SelectionInfoPanel = singleEntitySelectionPanel;
                }
                else
                {
                    Faction entityFaction = FactionMembership.GetFaction(entity);
                    bool isAllied = entityFaction != null && (LocalFaction.GetDiplomaticStance(entityFaction) & DiplomaticStance.SharedVision) != 0;
                    singleEntitySelectionPanel.ShowUnit((Unit)entity, isAllied);
                    ui.SelectionInfoPanel = singleEntitySelectionPanel;
                }
            }
            else
            {
                multipleUnitSelectionPanel.SetEntities(Selection);
                ui.SelectionInfoPanel = multipleUnitSelectionPanel;
            }
        }

        private void OnFocusedUnitTypeChanged(SelectionManager sender)
        {
            UpdateActionPanel();
        }

        private void OnMultipleSelectionPanelUnitSelected(MultipleUnitSelectionPanel sender, Entity entity)
        {
            Selection.Set(entity);
        }

        private void OnMultipleSelectionPanelUnitDeselected(MultipleUnitSelectionPanel sender, Entity entity)
        {
            Selection.Remove(entity);
        }

        private void OnMinimapCameraMoved(MatchUI sender, Vector2 normalizedPosition)
        {
            camera.Target = new Vector2(normalizedPosition.X * World.Width, normalizedPosition.Y * World.Height);
        }

        private void OnMinimapRightClicked(MatchUI sender, Vector2 normalizedPosition)
        {
            Vector2 worldPosition = new Vector2(normalizedPosition.X * World.Width, normalizedPosition.Y * World.Height);
            Input.MouseEventArgs args = new Input.MouseEventArgs(worldPosition, Input.MouseButton.Right, 1, 0);
            userInputManager.HandleMouseDown(args);
            args = new Input.MouseEventArgs(worldPosition, Input.MouseButton.Right, 0, 0);
            userInputManager.HandleMouseUp(args);
        }

        private void OnMinimapRendering(MatchUI sender, Region rectangle)
        {
            if (rectangle.Area == 0) return;

            Rectangle previousProjectionBounds = Graphics.Context.ProjectionBounds;

            Size clientSize = Graphics.Window.ClientAreaSize;

            Graphics.Context.ProjectionBounds = new Rectangle(
                -World.Bounds.Width * rectangle.MinX / rectangle.Width,
                -World.Bounds.Height * rectangle.MinY / rectangle.Height,
                World.Bounds.Width * clientSize.Width / rectangle.Width,
                World.Bounds.Height * clientSize.Height / rectangle.Height);
            matchRenderer.DrawMinimap();

            Graphics.Context.Stroke(camera.ViewBounds, Colors.Red);

            Graphics.Context.ProjectionBounds = previousProjectionBounds;
        }

        private void OnSelectingIdleWorkers(MatchUI sender, bool all)
        {
            if (workerActivityMonitor.InactiveWorkerCount == 0) return;

            var inactiveWorkers = workerActivityMonitor.InactiveWorkers.Cast<Entity>();

            if (all)
            {
                Selection.Set(inactiveWorkers);
                camera.Target = inactiveWorkers.First().Position;
            }
            else
            {
                Entity entityToSelect = Selection.SingleOrDefault();
                if (entityToSelect == null)
                {
                    entityToSelect = inactiveWorkers.First();
                }
                else
                {
                    int index = (inactiveWorkers.IndexOf(entityToSelect) + 1) % workerActivityMonitor.InactiveWorkerCount;
                    entityToSelect = inactiveWorkers.ElementAt(index);
                }

                Selection.Set(entityToSelect);
                camera.Target = entityToSelect.Position;
            }
        }

        private bool OnViewportMouseMoved(Control sender, MouseEvent @event)
        {
            Vector2 worldPosition = camera.ViewportToWorld(@event.Position - ui.ViewportRectangle.Min);
            Input.MouseEventArgs args = new Input.MouseEventArgs(worldPosition, Input.MouseButton.None, 0, 0);
            userInputManager.HandleMouseMove(args);
            return true;
        }

        private void OnViewportZoomed(MatchUI sender, float delta)
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

            Input.MouseEventArgs args = new Input.MouseEventArgs(worldPosition, inputButton, @event.ClickCount, 0f);

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

        private void OnChatted(Control sender, string message)
        {
            message = ProfanityFilter.Filter(message, Localizer.GetNoun("Smurf").ToLowerInvariant());
            userInputManager.LaunchChatMessage(message);
        }

        public override void Dispose()
        {
            matchRenderer.Dispose();
            audioPresenter.Dispose();
            commandPipeline.Dispose();
        }

        private void OnUserExited(MatchUI sender)
        {
            Manager.PopTo<MainMenuGameState>();
        }

        private void OnLocalFactionWarning(Faction sender, string text)
        {
            ui.AddMessage(text, LocalFaction.Color);
        }

        private void OnFactionMessageReceived(Match match, FactionMessage message)
        {
            if (!message.IsRecipient(userInputManager.LocalFaction)) return;

            string text = "{0}: {1}".FormatInvariant(message.Sender.Name, message.Text);
            ui.AddMessage(text, message.Sender.Color);
        }
        
        private void OnCheatUsed(Match match, Faction faction, string cheat)
        {
        	if (faction == LocalFaction)
        	{
        		string message = "Code de triche '{0}' appliqué!".FormatInvariant(cheat);
        		ui.AddMessage(message, faction.Color);
        	}
        	else
        	{
        		string message = "'{0}' a triché!".FormatInvariant(faction.Name);
        		ui.AddMessage(message, faction.Color);
        	}
        }
        
        private void OnDiplomaticStanceChanged(World sender, DiplomaticStanceChange change)
        {
        	if (change.SourceFaction == LocalFaction) return;
        	
            if (change.NewDiplomaticStance.HasFlag(DiplomaticStance.SharedControl))
            {
            	string message = "{0} désire partager le contrôle avec vous.".FormatInvariant(change.TargetFaction.Name);
            	ui.AddMessage(message, change.TargetFaction.Color);
            }
            else
            {
                if (change.NewDiplomaticStance.HasFlag(DiplomaticStance.SharedVision)
            	    && !change.OldDiplomaticStance.HasFlag(DiplomaticStance.SharedVision))
            	{
            		string message = "{0} partage sa vision avec vous.".FormatInvariant(change.TargetFaction.Name);
                    ui.AddMessage(message, change.TargetFaction.Color);
            	}
                else if (!change.NewDiplomaticStance.HasFlag(DiplomaticStance.SharedVision)
            	    && change.OldDiplomaticStance.HasFlag(DiplomaticStance.SharedVision))
                {
            		string message = "{0} ne partage plus sa vision avec vous.".FormatInvariant(change.TargetFaction.Name);
                    ui.AddMessage(message, change.TargetFaction.Color);
                }

                if (change.NewDiplomaticStance.HasFlag(DiplomaticStance.AlliedVictory)
            	    && !change.OldDiplomaticStance.HasFlag(DiplomaticStance.AlliedVictory))
            	{
            		string message = "{0} désire partager la victoire avec vous.".FormatInvariant(change.TargetFaction.Name);
                    ui.AddMessage(message, change.TargetFaction.Color);
            	}
                else if (!change.NewDiplomaticStance.HasFlag(DiplomaticStance.AlliedVictory)
            	         && change.OldDiplomaticStance.HasFlag(DiplomaticStance.AlliedVictory))
                {
            		string message = "{0} ne partagera plus la victoire avec vous.".FormatInvariant(change.TargetFaction.Name);
                    ui.AddMessage(message, change.TargetFaction.Color);
                }
            }
        }

        private void OnEntityRemoved(World sender, Entity entity)
        {
            Faction faction = FactionMembership.GetFaction(entity);
            if (faction == null || faction.Status == FactionStatus.Defeated) return;

            bool hasKeepAliveEntity = faction.Entities.Any(e => 
            {
                FactionMembership factionMembership = e.Components.TryGet<FactionMembership>();
                return e.IsAlive && factionMembership != null && factionMembership.IsKeepAlive;
            });
            if (hasKeepAliveEntity) return;
            
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
            	.Where(f => !Faction.HaveAlliedVictory(localCommander.Faction, f))
                .All(f => f == faction || f.Status == FactionStatus.Defeated);
            if (!allEnemyFactionsDefeated) return;

            audioPresenter.PlayVictorySound();
            //ui.DisplayVictoryMessage(() => Manager.PopTo<MainMenuGameState>());
        }
        #endregion
    }
}
