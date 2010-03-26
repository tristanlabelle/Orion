using System;
using System.Collections.Generic;
using System.Linq;
using OpenTK.Math;
using Orion.Engine;
using Orion.Engine.Collections;
using Orion.Engine.Geometry;
using Orion.Engine.Graphics;
using Orion.Engine.Gui;
using Orion.Game.Simulation;
using Orion.Game.Simulation.Utilities;
using Orion.Game.Presentation;
using Orion.Game.Presentation.Audio;
using Orion.Game.Presentation.Actions;
using Orion.Game.Presentation.Actions.Enablers;
using Orion.Game.Presentation.Renderers;
using Orion.Game.Matchmaking;
using Orion.Game.Matchmaking.Commands;
using Control = System.Windows.Forms.Control;
using Keys = System.Windows.Forms.Keys;
using MouseButtons = System.Windows.Forms.MouseButtons;

namespace Orion.Game.Presentation
{
    public sealed class MatchUI : UIDisplay
    {
        #region Fields
        #region Chat
        private readonly TextField chatInput;
        private readonly MatchConsole console;
        #endregion

        #region General UI
        private readonly MatchRenderer matchRenderer;
        private readonly ClippedView worldView;
        private readonly Frame hudFrame;
        private readonly Frame selectionFrame;
        #endregion

        #region Pause
        private readonly Frame pausePanel;
        #endregion

        #region Minimap
        private readonly Frame minimapFrame;
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
        private readonly ActionFrame actions;
        private Frame diplomacyFrame;
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
            this.match.Quitting += Quit;
            this.userInputManager = new UserInputManager(localCommander);

            this.gameAudio = new GameAudio();
            this.matchAudioPresenter = new MatchAudioPresenter(gameAudio, match, this.userInputManager);

            this.gameGraphics = gameGraphics;
            World world = match.World;

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

            Rectangle pauseButtonRectangle = Instant.CreateComponentRectangle(resourceDisplayFrame, new Vector2(0.69f, 0), new Vector2(0.84f, 1));
            Button pauseButton = new Button(pauseButtonRectangle, "Pause");
            pauseButton.Triggered += b => DisplayPausePanel();
            Children.Add(pauseButton);

            Rectangle diplomacyButtonRectangle = Instant.CreateComponentRectangle(resourceDisplayFrame, new Vector2(0.85f, 0), new Vector2(1, 1));
            Button diplomacyButton = new Button(diplomacyButtonRectangle, "Diplomatie");
            diplomacyButton.Triggered += b => DisplayDiplomacy();
            Children.Add(diplomacyButton);

            Rectangle hudRectangle = Instant.CreateComponentRectangle(Bounds, new Vector2(0, 0), new Vector2(1, 0.29f));
            hudFrame = new Frame(hudRectangle);
            Children.Add(hudFrame);

            Rectangle selectionFrameRectangle = Instant.CreateComponentRectangle(hudFrame.Bounds, new Vector2(0.25f, 0), new Vector2(0.75f, 1));
            selectionFrame = new Frame(selectionFrameRectangle, Colors.DarkGray);
            hudFrame.Children.Add(selectionFrame);

            Rectangle actionsRectangle = Instant.CreateComponentRectangle(hudFrame.Bounds, new Vector2(0.75f, 0), new Vector2(1, 1));
            actions = new ActionFrame(actionsRectangle);
            hudFrame.Children.Add(actions);

            Vector2 maxMinimapRectangleSize = new Vector2(0.23f, 0.9f);
            Vector2 minimapRectangleSize = maxMinimapRectangleSize;
            if (match.World.Width > match.World.Height)
                minimapRectangleSize.Y *= match.World.Height / (float)match.World.Width;
            else
                minimapRectangleSize.X *= match.World.Width / (float)match.World.Height;

            Vector2 minimapRectangleOrigin = new Vector2(
                0.01f + (maxMinimapRectangleSize.X - minimapRectangleSize.X) * 0.5f,
                0.05f + (maxMinimapRectangleSize.Y - minimapRectangleSize.Y) * 0.5f);

            Rectangle minimapRectangle = Instant.CreateComponentRectangle(hudFrame.Bounds,
                minimapRectangleOrigin, minimapRectangleOrigin + minimapRectangleSize);
            minimapFrame = new Frame(minimapRectangle, matchRenderer.MinimapRenderer);
            minimapFrame.Bounds = world.Bounds;
            hudFrame.Children.Add(minimapFrame);

            CreateScrollers();

            Rectangle chatInputRectangle = Instant.CreateComponentRectangle(Bounds, new Vector2(0.04f, 0.3f), new Vector2(0.915f, 0.34f));
            chatInput = new TextField(chatInputRectangle);
            chatInput.Triggered += SendMessage;
            chatInput.KeyboardButtonPressed += ChatInputKeyDown;

            Rectangle consoleRectangle = Instant.CreateComponentRectangle(Bounds, new Vector2(0.005f, 0.35f), new Vector2(0.5f, 0.9f));
            console = new MatchConsole(consoleRectangle);
            Children.Add(console);

            Rectangle pausePanelRectangle = Instant.CreateComponentRectangle(Bounds, new Vector2(0.33f, 0.33f), new Vector2(0.66f, 0.66f));
            pausePanel = new Frame(pausePanelRectangle);

            Rectangle quitGameRectangle = Instant.CreateComponentRectangle(pausePanel.Bounds, new Vector2(0.25f, 0.56f), new Vector2(0.75f, 0.86f));
            Button quitGame = new Button(quitGameRectangle, "Quitter");
            quitGame.Triggered += button => match.Quit();

            Rectangle resumeGameRectangle = Instant.CreateComponentRectangle(pausePanel.Bounds, new Vector2(0.25f, 0.14f), new Vector2(0.75f, 0.42f));
            Button resumeGame = new Button(resumeGameRectangle, match.IsPausable ? "Reprendre" : "Retour");
            resumeGame.Triggered += button => HidePausePanel();

            pausePanel.Children.Add(quitGame);
            pausePanel.Children.Add(resumeGame);

            KeyboardButtonPressed += (sender, args) => userInputManager.HandleKeyDown(args);
            KeyboardButtonReleased += (sender, args) => userInputManager.HandleKeyUp(args);

            userInputManager.Selection.Changed += OnSelectionChanged;
            userInputManager.SelectionManager.FocusedUnitTypeChanged += OnFocusedUnitTypeChanged;
            localCommander.CommandIssued += OnCommanderGeneratedCommand;
            minimapFrame.MouseButtonPressed += MinimapMouseDown;
            minimapFrame.MouseMoved += MinimapMouseMove;

            enablers.Add(new MoveEnabler(userInputManager, actions, gameGraphics));
            enablers.Add(new AttackEnabler(userInputManager, actions, gameGraphics));
            enablers.Add(new StandGuardEnabler(userInputManager, actions, gameGraphics));
            enablers.Add(new BuildEnabler(userInputManager, actions, gameGraphics));
            enablers.Add(new HarvestEnabler(userInputManager, actions, gameGraphics));
            enablers.Add(new TrainEnabler(userInputManager, actions, gameGraphics));
            enablers.Add(new HealEnabler(userInputManager, actions, gameGraphics));
            enablers.Add(new ResearchEnabler(userInputManager, actions, gameGraphics));

            workerActivityMonitor = new WorkerActivityMonitor(LocalFaction);
            workerActivityMonitor.WorkerActivityStateChanged += OnWorkerActivityStateChanged;
            Rectangle inactiveWorkerRectangle = Instant.CreateComponentRectangle(Bounds, new Vector2(0.005f, 0.3f), new Vector2(0.035f, 0.34f));
            Texture workerTexture = gameGraphics.GetUnitTexture("Schtroumpf");
            TexturedFrameRenderer workerButtonRenderer = new TexturedFrameRenderer(workerTexture, Colors.White, Colors.Gray, Colors.LightGray);
            this.idleWorkerButton = new Button(inactiveWorkerRectangle, string.Empty, workerButtonRenderer);
            this.idleWorkerButton.CaptionUpColor = Colors.Red;
            this.idleWorkerButton.Triggered += OnIdleWorkerButtonTriggered;
            UpdateWorkerActivityButton();

            LocalFaction.Warning += OnLocalFactionWarning;
        }
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

        #region Messages
        public void DisplayMessage(FactionMessage message)
        {
            Argument.EnsureNotNull(message, "message");

            string text = "{0}: {1}".FormatInvariant(message.Faction.Name, message.Text);
            console.AddMessage(text, message.Faction.Color);
        }

        public void DisplayDefeatMessage(Faction faction)
        {
            Argument.EnsureNotNull(faction, "faction");

            string text = "{0} a été vaincu.".FormatInvariant(faction.Name);
            console.AddMessage(text, faction.Color);

            if (faction != LocalFaction) return;
            
            if (match.IsPausable) match.Pause();
            Instant.DisplayAlert(this, "Vous avez perdu le match.", () => Parent.PopDisplay(this));
        }

        public void DisplayVictoryMessage(IEnumerable<Faction> factions)
        {
            Argument.EnsureNotNull(factions, "factions");
            if (!factions.Contains(LocalFaction)) return;
            
            if (match.IsPausable) match.Pause();
            Instant.DisplayAlert(this, "VICTOIRE !", () => Parent.PopDisplay(this));
        }
        #endregion

        #region Event Handling
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
            string text = chatInput.Contents;
            if (text.Any(character => !char.IsWhiteSpace(character)))
                userInputManager.LaunchChatMessage(chatInput.Contents);

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

            if (args.Key == Keys.F9)
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

        protected override bool OnCharacterPressed(char character)
        {
            if (character == '\r')
            {
                chatInput.Clear();
                Children.Add(chatInput);
            }
            return base.OnCharacterPressed(character);
        }

        protected override bool OnDoubleClick(MouseEventArgs args)
        {
            Vector2 newPosition = Rectangle.ConvertPoint(worldView.Frame, worldView.Bounds, args.Position);
            userInputManager.HandleMouseDoubleClick(args.CloneWithNewPosition(newPosition));
            return base.OnDoubleClick(args);
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

            match.Update(timeDeltaInSeconds);
            base.Update(timeDeltaInSeconds);
        }

        private void Quit(Match sender)
        {
            Parent.PopDisplay(this);
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
            while (selectionFrame.Children.Count > 0) selectionFrame.Children[0].Dispose();
            selectionFrame.Children.Clear();
            selectionFrame.Renderer = null;

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
            actions.Restore();
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

        #region UIDisplay Implementation
        protected override void OnShadowed()
        {
            Root.PopDisplay(this);
            base.OnShadowed();
        }
        #endregion

        #region Methods
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
            selectionFrame.Renderer = new UnitFrameRenderer(userInputManager.LocalFaction, unit, gameGraphics);
            UnitButtonRenderer buttonRenderer = new UnitButtonRenderer(unit, gameGraphics);
            Button unitButton = new Button(new Rectangle(10, 10, 130, 200), string.Empty, buttonRenderer);
            float aspectRatio = Bounds.Width / Bounds.Height;
            unitButton.Bounds = new Rectangle(3f, 3f * aspectRatio);

            unitButton.Triggered += OnUnitButtonPressed;
            selectionFrame.Children.Add(unitButton);
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

            selectionFrame.Renderer = new FilledFrameRenderer(Colors.DarkGray, Colors.Gray);
            const float paddingX = 5;
            const float paddingY = 15;
            Rectangle frame = new Rectangle(selectionFrame.Bounds.Width / 11 - paddingX * 2,
                selectionFrame.Bounds.Height / 2.2f - paddingY * 2);
            float currentX = paddingX + selectionFrame.Bounds.MinX;
            float currentY = selectionFrame.Bounds.Height - paddingY - frame.Height;
            foreach (Unit unit in orderedSelectedUnits)
            {
                UnitButtonRenderer renderer = new UnitButtonRenderer(unit, gameGraphics);
                renderer.HasFocus = (unit.Type == SelectionManager.FocusedUnitType);
                Button unitButton = new Button(frame.TranslatedTo(currentX, currentY), string.Empty, renderer);
                float aspectRatio = Bounds.Width / Bounds.Height;
                unitButton.Bounds = new Rectangle(3f, 3f * aspectRatio);
                unitButton.Triggered += OnUnitButtonPressed;
                currentX += frame.Width + paddingX;
                if (currentX + frame.Width > selectionFrame.Bounds.MaxX)
                {
                    currentY -= frame.Height + paddingY;
                    currentX = paddingX + selectionFrame.Bounds.MinX;
                }
                selectionFrame.Children.Add(unitButton);
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
                foreach (Button unitButton in selectionFrame.Children)
                {
                    UnitButtonRenderer renderer = (UnitButtonRenderer)unitButton.Renderer;
                    renderer.HasFocus = renderer.Unit.Type == SelectionManager.FocusedUnitType;
                }
            }
        }

        private void UpdateSkillsPanel()
        {
            actions.Clear();
            if (Selection.Type != SelectionType.Units) return;
            
            if (Selection.Units.All(u => u.Faction == LocalFaction))
                actions.Push(new UnitActionProvider(enablers, SelectionManager.FocusedUnitType));
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
            Rectangle diplomacyFrameRectangle = Instant.CreateComponentRectangle(Bounds,new Vector2(0.0f,0.0f), new Vector2(1f,1f));
            diplomacyFrame = new Frame(diplomacyFrameRectangle);
            Children.Add(diplomacyFrame);
            Rectangle listFrameRectangle = Instant.CreateComponentRectangle(diplomacyFrame.Bounds,new Vector2(0.0f,0.1f), new Vector2(1f,1f));
            ListFrame listFrame = new ListFrame(listFrameRectangle,new Vector2(0,0));
            Rectangle rectangleFrame = new Rectangle(listFrame.Bounds.Width, listFrame.Bounds.Height/10);

            assocFactionDropList.Clear();
            foreach (Faction faction in World.Factions)
            {
                if (faction == LocalFaction) continue;
                if (faction.Status == FactionStatus.Defeated) continue;

                Frame frameFaction = new Frame(rectangleFrame, faction.Color);
                
                Rectangle rectangleFaction = Instant.CreateComponentRectangle(frameFaction.Bounds,new Vector2(0.7f,0.7f), new Vector2(1f,1f));
                DropdownList<DiplomaticStance> dropdownList = new DropdownList<DiplomaticStance>(rectangleFaction);
                dropdownList.StringConverter = stance => stance == DiplomaticStance.Ally ? "Allié" : "Ennemi";
                assocFactionDropList.Add(faction, dropdownList);

                dropdownList.AddItem(DiplomaticStance.Enemy);
                dropdownList.AddItem(DiplomaticStance.Ally);
                dropdownList.SelectedItem = LocalFaction.GetDiplomaticStance(faction);

                frameFaction.Children.Add(new Label(faction.Name));
                frameFaction.Children.Add(dropdownList);
                

                listFrame.Children.Add(frameFaction);
            }
            diplomacyFrame.Children.Add(listFrame);

            Rectangle rectangleButton = Instant.CreateComponentRectangle(diplomacyFrame.Bounds,new Vector2(0.4f,0.01f), new Vector2(0.6f,0.09f));
            Button acceptButton = new Button(rectangleButton, "Accepter");
            acceptButton.Triggered += AcceptNewDiplomacy;
            diplomacyFrame.Children.Add(acceptButton);
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
