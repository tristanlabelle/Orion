using System;
using System.Collections.Generic;
using System.Linq;
using OpenTK.Math;
using Orion.Engine;
using Orion.Engine.Collections;
using Orion.Engine.Geometry;
using Orion.Engine.Graphics;
using Orion.Engine.Gui;
using Orion.Engine.Input;
using Orion.Game.Matchmaking;
using Orion.Game.Matchmaking.Commands;
using Orion.Game.Presentation;
using Orion.Game.Presentation.Actions;
using Orion.Game.Presentation.Actions.Enablers;
using Orion.Game.Presentation.Audio;
using Orion.Game.Presentation.Renderers;
using Orion.Game.Simulation;
using Orion.Game.Simulation.Skills;
using Orion.Game.Simulation.Utilities;
using Control = System.Windows.Forms.Control;
using Keys = System.Windows.Forms.Keys;
using MouseButtons = System.Windows.Forms.MouseButtons;

namespace Orion.Game.Presentation
{
    public sealed class MatchUI : MaximizedPanel
    {
        #region Fields
        #region Chat
        private readonly TextField chatInput;
        private readonly MatchConsole console;
        #endregion

        #region General UI
        private readonly MatchRenderer matchRenderer;
        private readonly ClippedView worldView;
        private readonly Panel hudPanel;
        private readonly Panel selectionPanel;
        #endregion

        #region Pause & Diplomacy
        private readonly Panel pausePanel;
        private Panel diplomacyPanel;
        #endregion

        #region Minimap
        private readonly Panel minimapPanel;
        private bool mouseDownOnMinimap;
        #endregion

        #region Idle Workers
        private readonly WorkerActivityMonitor workerActivityMonitor;
        private readonly Button idleWorkerButton;
        #endregion

        private readonly GameGraphics gameGraphics;
        private readonly Match match;
        private readonly List<ActionEnabler> enablers = new List<ActionEnabler>();
        private readonly UserInputManager userInputManager;
        private readonly GameAudio gameAudio;
        private readonly MatchAudioPresenter matchAudioPresenter;
        private readonly ActionPanel actionPanel;
        private bool isSpaceDown;
        private bool isShiftDown;
        private Dictionary<Faction, DropdownList<DiplomaticStance>> assocFactionDropList = new Dictionary<Faction, DropdownList<DiplomaticStance>>();
        #endregion

        #region Constructors
        public MatchUI(GameGraphics gameGraphics, Match match, SlaveCommander localCommander)
        {
            Argument.EnsureNotNull(gameGraphics, "gameGraphics");
            Argument.EnsureNotNull(match, "match");
            Argument.EnsureNotNull(localCommander, "localCommander");

            this.match = match;
            this.match.FactionMessageReceived += OnFactionMessageReceived;
            this.userInputManager = new UserInputManager(localCommander);

            this.gameAudio = new GameAudio();
            this.matchAudioPresenter = new MatchAudioPresenter(gameAudio, match, this.userInputManager);

            this.gameGraphics = gameGraphics;
            World world = match.World;
            world.FactionDefeated += OnFactionDefeated;

            matchRenderer = new MatchRenderer(userInputManager, gameGraphics);

            Rectangle worldFrame = Instant.CreateComponentRectangle(Bounds, new Vector2(0, 0.29f), new Vector2(1, 1));
            worldView = new ClippedView(worldFrame, world.Bounds, matchRenderer);
            worldView.Bounds = new Rectangle(40, 20);
            worldView.MinimumVisibleBoundsSize = new Vector2(8, 4);
            worldView.BoundsChanged += OnWorldViewBoundsChanged;
            Children.Add(worldView);
            matchAudioPresenter.SetViewBounds(worldView.Bounds);

            Rectangle resourceDisplayFrame = Instant.CreateComponentRectangle(Bounds, new Vector2(0, 0.96f), new Vector2(1, 1));
            ResourceDisplay resourceDisplay = new ResourceDisplay(resourceDisplayFrame, userInputManager.LocalFaction);
            Children.Add(resourceDisplay);

            Rectangle pauseButtonFrame = Instant.CreateComponentRectangle(resourceDisplayFrame, new Vector2(0.69f, 0), new Vector2(0.84f, 1));
            Button pauseButton = new Button(pauseButtonFrame, "Pause");
            pauseButton.Triggered += b => DisplayPausePanel();
            Children.Add(pauseButton);

            Rectangle diplomacyButtonFrame = Instant.CreateComponentRectangle(resourceDisplayFrame, new Vector2(0.85f, 0), new Vector2(1, 1));
            Button diplomacyButton = new Button(diplomacyButtonFrame, "Diplomatie");
            diplomacyButton.Triggered += b => DisplayDiplomacy();
            Children.Add(diplomacyButton);

            Rectangle hudPanelFrame = Instant.CreateComponentRectangle(Bounds, new Vector2(0, 0), new Vector2(1, 0.29f));
            hudPanel = new Panel(hudPanelFrame);
            Children.Add(hudPanel);

            Rectangle selectionPanelFrame = Instant.CreateComponentRectangle(hudPanel.Bounds, new Vector2(0.25f, 0), new Vector2(0.75f, 1));
            selectionPanel = new Panel(selectionPanelFrame, Colors.DarkGray);
            hudPanel.Children.Add(selectionPanel);

            Rectangle actionPanelFrame = Instant.CreateComponentRectangle(hudPanel.Bounds, new Vector2(0.75f, 0), new Vector2(1, 1));
            actionPanel = new ActionPanel(actionPanelFrame);
            hudPanel.Children.Add(actionPanel);

            Vector2 maxMinimapRectangleSize = new Vector2(0.23f, 0.9f);
            Vector2 minimapRectangleSize = maxMinimapRectangleSize;
            if (match.World.Width > match.World.Height)
                minimapRectangleSize.Y *= match.World.Height / (float)match.World.Width;
            else
                minimapRectangleSize.X *= match.World.Width / (float)match.World.Height;

            Vector2 minimapRectangleOrigin = new Vector2(
                0.01f + (maxMinimapRectangleSize.X - minimapRectangleSize.X) * 0.5f,
                0.05f + (maxMinimapRectangleSize.Y - minimapRectangleSize.Y) * 0.5f);

            Rectangle minimapFrame = Instant.CreateComponentRectangle(hudPanel.Bounds,
                minimapRectangleOrigin, minimapRectangleOrigin + minimapRectangleSize);
            minimapPanel = new Panel(minimapFrame, matchRenderer.MinimapRenderer);
            minimapPanel.Bounds = world.Bounds;
            hudPanel.Children.Add(minimapPanel);

            CreateScrollers();

            Rectangle chatInputFrame = Instant.CreateComponentRectangle(Bounds, new Vector2(0.04f, 0.3f), new Vector2(0.915f, 0.34f));
            chatInput = new TextField(chatInputFrame);
            chatInput.Triggered += SendMessage;
            chatInput.KeyboardButtonPressed += ChatInputKeyDown;

            Rectangle consoleFrame = Instant.CreateComponentRectangle(Bounds, new Vector2(0.005f, 0.35f), new Vector2(0.5f, 0.9f));
            console = new MatchConsole(consoleFrame);
            Children.Add(console);

            Rectangle pausePanelFrame = Instant.CreateComponentRectangle(Bounds, new Vector2(0.33f, 0.33f), new Vector2(0.66f, 0.66f));
            pausePanel = new Panel(pausePanelFrame);

            Rectangle quitGameButtonFrame = Instant.CreateComponentRectangle(pausePanel.Bounds, new Vector2(0.25f, 0.56f), new Vector2(0.75f, 0.86f));
            Button quitGameButton = new Button(quitGameButtonFrame, "Quitter");
            quitGameButton.Triggered += button => QuitPressed.Raise(this);

            Rectangle resumeGameButtonFrame = Instant.CreateComponentRectangle(pausePanel.Bounds, new Vector2(0.25f, 0.14f), new Vector2(0.75f, 0.42f));
            Button resumeGameButton = new Button(resumeGameButtonFrame, match.IsPausable ? "Reprendre" : "Retour");
            resumeGameButton.Triggered += button => HidePausePanel();

            pausePanel.Children.Add(quitGameButton);
            pausePanel.Children.Add(resumeGameButton);

            KeyboardButtonPressed += (sender, args) => userInputManager.HandleKeyDown(args);
            KeyboardButtonReleased += (sender, args) => userInputManager.HandleKeyUp(args);

            userInputManager.Selection.Changed += OnSelectionChanged;
            userInputManager.SelectionManager.FocusedUnitTypeChanged += OnFocusedUnitTypeChanged;
            localCommander.CommandIssued += OnCommanderGeneratedCommand;
            minimapPanel.MouseButtonPressed += MinimapMouseDown;
            minimapPanel.MouseMoved += MinimapMouseMove;

            enablers.Add(new MoveEnabler(userInputManager, actionPanel, gameGraphics));
            enablers.Add(new AttackEnabler(userInputManager, actionPanel, gameGraphics));
            enablers.Add(new StandGuardEnabler(userInputManager, actionPanel, gameGraphics));
            enablers.Add(new BuildEnabler(userInputManager, actionPanel, gameGraphics));
            enablers.Add(new HarvestEnabler(userInputManager, actionPanel, gameGraphics));
            enablers.Add(new TrainEnabler(userInputManager, actionPanel, gameGraphics));
            enablers.Add(new HealEnabler(userInputManager, actionPanel, gameGraphics));
            enablers.Add(new ResearchEnabler(userInputManager, actionPanel, gameGraphics));

            workerActivityMonitor = new WorkerActivityMonitor(LocalFaction);
            workerActivityMonitor.WorkerActivityStateChanged += OnWorkerActivityStateChanged;
            Rectangle inactiveWorkerRectangle = Instant.CreateComponentRectangle(Bounds, new Vector2(0.005f, 0.3f), new Vector2(0.035f, 0.34f));
            Texture workerTexture = gameGraphics.GetUnitTexture("Schtroumpf");
            TexturedRenderer workerButtonRenderer = new TexturedRenderer(workerTexture, Colors.White, Colors.Gray, Colors.LightGray);
            this.idleWorkerButton = new Button(inactiveWorkerRectangle, string.Empty, workerButtonRenderer);
            this.idleWorkerButton.CaptionUpColor = Colors.Red;
            this.idleWorkerButton.Triggered += OnIdleWorkerButtonTriggered;
            UpdateWorkerActivityButton();

            LocalFaction.Warning += OnLocalFactionWarning;

            Unit viewTarget = localCommander.Faction.Units.FirstOrDefault(unit => unit.HasSkill<TrainSkill>())
                ?? localCommander.Faction.Units.FirstOrDefault();
            if (viewTarget != null) CenterOn(viewTarget.Center);
        }
        #endregion

        #region Events
        /// <summary>
        /// Raised when the user has quitted the match through the UI.
        /// </summary>
        public event Action<MatchUI> QuitPressed;
        #endregion

        #region Properties
        private MatchRenderer MatchRenderer
        {
            get { return (MatchRenderer)worldView.Renderer; }
        }

        private WorldRenderer WorldRenderer
        {
            get { return MatchRenderer.WorldRenderer; }
        }

        private SelectionManager SelectionManager
        {
            get { return userInputManager.SelectionManager; }
        }

        private Selection Selection
        {
            get { return userInputManager.Selection; }
        }

        private Faction LocalFaction
        {
            get { return userInputManager.LocalFaction; }
        }

        private World World
        {
            get { return LocalFaction.World; }
        }
        #endregion

        #region Methods
        #region Initialization
        private void CreateScrollers()
        {
            const float sliderSize = 0.005f;

            Rectangle northFrame = Instant.CreateComponentRectangle(Bounds, new Vector2(0, 1 - sliderSize), new Vector2(1, 1));
            Rectangle southFrame = Instant.CreateComponentRectangle(Bounds, new Vector2(0, 0), new Vector2(1, sliderSize));
            Rectangle eastFrame = Instant.CreateComponentRectangle(Bounds, new Vector2(1 - sliderSize, 0), new Vector2(1, 1));
            Rectangle westFrame = Instant.CreateComponentRectangle(Bounds, new Vector2(0, 0), new Vector2(sliderSize, 1));
            Scroller northScroller = new Scroller(worldView, northFrame, new Vector2(0, 0.05f), Keys.Up);
            Scroller southScroller = new Scroller(worldView, southFrame, new Vector2(0, -0.05f), Keys.Down);
            Scroller eastScroller = new Scroller(worldView, eastFrame, new Vector2(0.025f, 0), Keys.Right);
            Scroller westScroller = new Scroller(worldView, westFrame, new Vector2(-0.025f, 0), Keys.Left);

            Children.Add(northScroller);
            Children.Add(southScroller);
            Children.Add(eastScroller);
            Children.Add(westScroller);
        }
        #endregion

        #region Event Handling
        private void OnFactionDefeated(World sender, Faction faction)
        {
            string text = "{0} a été vaincu.".FormatInvariant(faction.Name);
            console.AddMessage(text, faction.Color);
        }

        private void OnFactionMessageReceived(Match sender, FactionMessage message)
        {
            Argument.EnsureNotNull(message, "message");

            string text = "{0}: {1}".FormatInvariant(message.Faction.Name, message.Text);
            console.AddMessage(text, message.Faction.Color);
        }

        private void OnWorkerActivityStateChanged(WorkerActivityMonitor sender, Unit worker)
        {
            UpdateWorkerActivityButton();
        }

        private void UpdateWorkerActivityButton()
        {
            if (workerActivityMonitor.InactiveWorkerCount == 0)
                Children.Remove(idleWorkerButton);
            else
            {
                idleWorkerButton.Caption = workerActivityMonitor.InactiveWorkerCount.ToStringInvariant();
                if (idleWorkerButton.Parent == null)
                    Children.Add(idleWorkerButton);
            }
        }

        private void OnIdleWorkerButtonTriggered(Button sender)
        {
            var inactiveWorkers = workerActivityMonitor.InactiveWorkers;
            if (isShiftDown)
            {
                Selection.Set(inactiveWorkers);
            }
            else
            {
                if (Selection.Type == SelectionType.Units && Selection.Count == 1)
                {
                    Unit selectedUnit = Selection.Units.First();
                    if (inactiveWorkers.Contains(selectedUnit))
                    {
                        int nextIndex = (inactiveWorkers.IndexOf(selectedUnit) + 1) % inactiveWorkers.Count();
                        Selection.Set(inactiveWorkers.ElementAt(nextIndex));
                        CenterOnSelection();
                        return;
                    }
                }

                Selection.Set(inactiveWorkers.First());
                CenterOnSelection();
            }
        }

        protected override bool OnMouseWheelScrolled(MouseEventArgs args)
        {
            float scale = 1 - args.WheelDelta;
            worldView.Zoom(scale);
            return base.OnMouseWheelScrolled(args);
        }

        protected override bool OnMouseButtonPressed(MouseEventArgs args)
        {
            if (worldView.Frame.ContainsPoint(args.Position))
            {
                Vector2 newPosition = Rectangle.ConvertPoint(worldView.Frame, worldView.Bounds, args.Position);
                userInputManager.HandleMouseDown(args.CloneWithNewPosition(newPosition));
            }

            return base.OnMouseButtonPressed(args);
        }

        protected override bool OnMouseMoved(MouseEventArgs args)
        {
            if (worldView.Frame.ContainsPoint(args.Position) || (Control.MouseButtons & MouseButtons.Left) != 0)
            {
                Vector2 newPosition = Rectangle.ConvertPoint(worldView.Frame, worldView.Bounds, args.Position);
                userInputManager.HandleMouseMove(args.CloneWithNewPosition(newPosition));
            }
            else
            {
                userInputManager.HoveredUnit = null;
            }

            return base.OnMouseMoved(args);
        }

        protected override bool OnMouseButtonReleased(MouseEventArgs args)
        {
            Vector2 newPosition = Rectangle.ConvertPoint(worldView.Frame, worldView.Bounds, args.Position);
            userInputManager.HandleMouseUp(args.CloneWithNewPosition(newPosition));
            mouseDownOnMinimap = false;
            return base.OnMouseButtonReleased(args);
        }

        private void SendMessage(TextField chatInput)
        {
            string text = chatInput.Contents.Trim();
            if (text.Any(character => !char.IsWhiteSpace(character)))
            {
                if (text[0] == '#')
                    userInputManager.LaunchAllyChatMessage(text.Substring(1));
                else
                    userInputManager.LaunchChatMessage(chatInput.Contents);
            }

            Children.Remove(chatInput);
        }

        private void ChatInputKeyDown(Responder sender, KeyboardEventArgs args)
        {
            if (args.Key == Keys.Escape)
            {
                chatInput.Clear();
                Children.Remove(chatInput);
            }
        }

        protected override bool OnKeyboardButtonPressed(KeyboardEventArgs args)
        {
            isShiftDown = args.IsShiftModifierDown;
            MatchRenderer.DrawAllHealthBars = args.IsAltModifierDown;
            isSpaceDown = args.Key == Keys.Space;

            if (args.Key == Keys.Enter)
            {
                chatInput.Clear();
                Children.Add(chatInput);
                return false;
            }
            else if (args.Key == Keys.F9)
            {
                DisplayPausePanel();
                return false;
            }
            else if (args.Key == Keys.F10)
            {
                DisplayDiplomacy();
                return false;
            }

            return base.OnKeyboardButtonPressed(args);
        }

        protected override bool OnKeyboardButtonReleased(KeyboardEventArgs args)
        {
            isShiftDown = args.IsShiftModifierDown;
            ((MatchRenderer)worldView.Renderer).DrawAllHealthBars = args.IsAltModifierDown;
            isSpaceDown = (args.Key != Keys.Space && isSpaceDown);
            return base.OnKeyboardButtonReleased(args);
        }

        protected override void Update(float timeDeltaInSeconds)
        {
            if (isSpaceDown && !Selection.IsEmpty)
                CenterOnSelection();

            base.Update(timeDeltaInSeconds);
        }

        private void OnWorldViewBoundsChanged(View sender, Rectangle oldBounds)
        {
            Rectangle newBounds = sender.Bounds;
            matchAudioPresenter.SetViewBounds(newBounds);
            worldView.FullBounds = World.Bounds
                .TranslatedBy(-newBounds.Extent)
                .ResizedBy(newBounds.Width, newBounds.Height);

            if (worldView.IsMouseOver)
            {
                Vector2 newPosition = Rectangle.ConvertPoint(worldView.Frame, worldView.Bounds, worldView.MousePosition.Value);
                userInputManager.HandleMouseMove(new MouseEventArgs(newPosition, MouseButton.None, 0, 0));
            }
        }

        private void MinimapMouseDown(Responder source, MouseEventArgs args)
        {
            if (args.Button == MouseButton.Left)
            {
                if (userInputManager.SelectedCommand != null)
                {
                    userInputManager.LaunchMouseCommand(args.Position);
                }
                else
                {
                    MoveWorldView(args.Position);
                    mouseDownOnMinimap = true;
                }
            }
            else if (args.Button == MouseButton.Right)
            {
                userInputManager.LaunchDefaultCommand(args.Position);
            }
        }

        private void MinimapMouseMove(Responder source, MouseEventArgs args)
        {
            if (mouseDownOnMinimap) MoveWorldView(args.Position);
        }

        private void OnSelectionChanged(Selection selection)
        {
            while (selectionPanel.Children.Count > 0) selectionPanel.Children[0].Dispose();
            selectionPanel.Children.Clear();
            selectionPanel.Renderer = null;

            if (selection.Type == SelectionType.Units)
            {
                if (selection.Count == 1)
                    CreateSingleUnitSelectionPanel();
                else
                    CreateMultipleUnitsSelectionPanel();
            }
        }

        private void OnFocusedUnitTypeChanged(SelectionManager selectionManager)
        {
            UpdateSkillsPanel();
        }

        private void OnCommanderGeneratedCommand(Commander commander, Command command)
        {
            actionPanel.Restore();
        }

        private void AcceptNewDiplomacy(Button bouton)
        {
            foreach (var pair in assocFactionDropList)
                if (LocalFaction.GetDiplomaticStance(pair.Key) != pair.Value.SelectedItem)
                    userInputManager.LaunchChangeDiplomacy(pair.Key);

            // Remove diplomacy panel from view.
            assocFactionDropList.Clear();
            bouton.Parent.RemoveFromParent();
        }

        private void OnLocalFactionWarning(Faction sender, string args)
        {
            console.AddMessage(args, sender.Color);
        }
        #endregion

        #region Methods
        public void DisplayDefeatMessage(Action callback)
        {
            Argument.EnsureNotNull(callback, "callback");
            Instant.DisplayAlert(this, "Vous avez perdu le match.", callback);
        }

        public void DisplayVictoryMessage(Action callback)
        {
            Argument.EnsureNotNull(callback, "callback");
            Instant.DisplayAlert(this, "VICTOIRE !", callback);
        }

        public void CenterOn(Vector2 position)
        {
            Vector2 worldBoundsExtent = worldView.Bounds.Extent;
            worldView.Bounds = worldView.Bounds.TranslatedTo(position - worldBoundsExtent);
        }

        private void CenterOnSelection()
        {
            CenterOn(SelectionManager.FocusedUnit.Center);
        }

        private void CreateSingleUnitSelectionPanel()
        {
            Unit unit = Selection.Units.First();
            selectionPanel.Renderer = new UnitPanelRenderer(userInputManager.LocalFaction, unit, gameGraphics);
            UnitButtonRenderer buttonRenderer = new UnitButtonRenderer(unit, gameGraphics);
            Button unitButton = new Button(new Rectangle(10, 10, 130, 200), string.Empty, buttonRenderer);
            float aspectRatio = Bounds.Width / Bounds.Height;
            unitButton.Bounds = new Rectangle(3f, 3f * aspectRatio);

            unitButton.Triggered += OnUnitButtonPressed;
            selectionPanel.Children.Add(unitButton);
        }

        private void CreateMultipleUnitsSelectionPanel()
        {
            List<Unit> orderedSelectedUnits = userInputManager.Selection.Units.ToList();

            var selectedUnitTypes = orderedSelectedUnits
                .GroupBy(unit => unit.Type)
                .OrderByDescending(group => group.Count())
                .Select(group => group.Key)
                .ToList();

            orderedSelectedUnits.Sort((a, b) =>
            {
                int comparison = selectedUnitTypes.IndexOf(a.Type)
                    .CompareTo(selectedUnitTypes.IndexOf(b.Type));
                if (comparison == 0) comparison = a.Handle.Value.CompareTo(b.Handle.Value);
                return comparison;
            });

            selectionPanel.Renderer = new FilledRenderer(Colors.DarkGray, Colors.Gray);
            const float paddingX = 5;
            const float paddingY = 15;
            Rectangle frame = new Rectangle(selectionPanel.Bounds.Width / 11 - paddingX * 2,
                selectionPanel.Bounds.Height / 2.2f - paddingY * 2);
            float currentX = paddingX + selectionPanel.Bounds.MinX;
            float currentY = selectionPanel.Bounds.Height - paddingY - frame.Height;
            foreach (Unit unit in orderedSelectedUnits)
            {
                UnitButtonRenderer renderer = new UnitButtonRenderer(unit, gameGraphics);
                renderer.HasFocus = (unit.Type == SelectionManager.FocusedUnitType);
                Button unitButton = new Button(frame.TranslatedTo(currentX, currentY), string.Empty, renderer);
                float aspectRatio = Bounds.Width / Bounds.Height;
                unitButton.Bounds = new Rectangle(3f, 3f * aspectRatio);
                unitButton.Triggered += OnUnitButtonPressed;
                currentX += frame.Width + paddingX;
                if (currentX + frame.Width > selectionPanel.Bounds.MaxX)
                {
                    currentY -= frame.Height + paddingY;
                    currentX = paddingX + selectionPanel.Bounds.MinX;
                }
                selectionPanel.Children.Add(unitButton);
            }
        }

        private void OnUnitButtonPressed(Button button)
        {
            Unit unit = ((UnitButtonRenderer)button.Renderer).Unit;
            if (unit.Type == SelectionManager.FocusedUnitType || SelectionManager.Selection.Count == 1)
            {
                SelectionManager.Selection.Set(unit);
                MoveWorldView(unit.Center);
            }
            else
            {
                SelectionManager.FocusedUnitType = unit.Type;
                foreach (Button unitButton in selectionPanel.Children)
                {
                    UnitButtonRenderer renderer = (UnitButtonRenderer)unitButton.Renderer;
                    renderer.HasFocus = renderer.Unit.Type == SelectionManager.FocusedUnitType;
                }
            }
        }

        private void UpdateSkillsPanel()
        {
            actionPanel.Clear();
            if (Selection.Type != SelectionType.Units) return;
            
            if (Selection.Units.All(u => u.Faction == LocalFaction))
                actionPanel.Push(new UnitActionProvider(enablers, SelectionManager.FocusedUnitType));
        }

        private void MoveWorldView(Vector2 center)
        {
            Vector2 difference = worldView.Bounds.Min - worldView.Bounds.Center;
            Rectangle newBounds = worldView.Bounds.TranslatedTo(center + difference);
            float xDiff = worldView.FullBounds.MaxX - newBounds.MaxX;
            float yDiff = worldView.FullBounds.MaxY - newBounds.MaxY;
            if (xDiff < 0) newBounds = newBounds.TranslatedXBy(xDiff);
            if (yDiff < 0) newBounds = newBounds.TranslatedYBy(yDiff);
            if (newBounds.MinX < -newBounds.Width / 2) newBounds = newBounds.TranslatedTo(-newBounds.Width / 2, newBounds.Min.Y);
            if (newBounds.MinY < -newBounds.Height / 2) newBounds = newBounds.TranslatedTo(newBounds.Min.X, -newBounds.Height / 2);
            worldView.Bounds = newBounds;
        }

        private void DisplayPausePanel()
        {
            if (Children.Contains(pausePanel)) return;

            match.Pause();
            if (!match.IsRunning)
            {
                foreach (Scroller scroller in Children.OfType<Scroller>())
                    scroller.IsEnabled = false;
            }

            Children.Add(pausePanel);
        }

        private void HidePausePanel()
        {
            foreach (Scroller scroller in Children.OfType<Scroller>())
                scroller.IsEnabled = true;

            Children.Remove(pausePanel);
            match.Resume();
        }

        private void DisplayDiplomacy()
        {
            Rectangle diplomacyPanelFrame = Instant.CreateComponentRectangle(Bounds,new Vector2(0.0f,0.0f), new Vector2(1f,1f));
            diplomacyPanel = new Panel(diplomacyPanelFrame);
            Children.Add(diplomacyPanel);

            Rectangle listPanelFrame = Instant.CreateComponentRectangle(diplomacyPanel.Bounds,new Vector2(0.0f,0.1f), new Vector2(1f,1f));
            ListPanel listPanel = new ListPanel(listPanelFrame,new Vector2(0,0));

            Rectangle factionPanelFrame = new Rectangle(listPanel.Bounds.Width, listPanel.Bounds.Height/10);

            assocFactionDropList.Clear();
            foreach (Faction faction in World.Factions)
            {
                if (faction == LocalFaction) continue;
                if (faction.Status == FactionStatus.Defeated) continue;

                Panel factionPanel = new Panel(factionPanelFrame, faction.Color);
                
                Rectangle dropdownListFrame = Instant.CreateComponentRectangle(factionPanel.Bounds,new Vector2(0.7f,0.7f), new Vector2(1f,1f));
                DropdownList<DiplomaticStance> dropdownList = new DropdownList<DiplomaticStance>(dropdownListFrame);
                dropdownList.StringConverter = stance => stance == DiplomaticStance.Ally ? "Allié" : "Ennemi";
                assocFactionDropList.Add(faction, dropdownList);

                dropdownList.AddItem(DiplomaticStance.Enemy);
                dropdownList.AddItem(DiplomaticStance.Ally);
                dropdownList.SelectedItem = LocalFaction.GetDiplomaticStance(faction);

                factionPanel.Children.Add(new Label(faction.Name));
                factionPanel.Children.Add(dropdownList);

                listPanel.Children.Add(factionPanel);
            }
            diplomacyPanel.Children.Add(listPanel);

            Rectangle buttonFrame = Instant.CreateComponentRectangle(diplomacyPanel.Bounds,new Vector2(0.4f,0.01f), new Vector2(0.6f,0.09f));
            Button acceptButton = new Button(buttonFrame, "Accepter");
            acceptButton.Triggered += AcceptNewDiplomacy;
            diplomacyPanel.Children.Add(acceptButton);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                matchRenderer.Dispose();
            }

            base.Dispose(disposing);
        }
        #endregion
        #endregion
    }
}
