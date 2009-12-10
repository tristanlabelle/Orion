using System;
using System.Collections.Generic;
using Control = System.Windows.Forms.Control;
using MouseButtons = System.Windows.Forms.MouseButtons;
using System.Linq;
using OpenTK.Math;
using Orion.Commandment;
using Orion.Commandment.Commands;
using Orion.GameLogic;
using Orion.GameLogic.Tasks;
using Orion.Geometry;
using Orion.Graphics;
using Orion.Graphics.Renderers;
using Orion.UserInterface.Widgets;
using Orion.UserInterface.Actions;
using Orion.UserInterface.Actions.Enablers;
using Color = System.Drawing.Color;
using Font = System.Drawing.Font;
using Keys = System.Windows.Forms.Keys;

namespace Orion.UserInterface
{
    public class MatchUI : UIDisplay
    {
        #region Fields
        #region Chat
        private static readonly TimeSpan messageTimeToLive = new TimeSpan(0, 0, 10);
        private readonly TextField chatInput;
        private readonly TransparentFrame chatMessages;
        private readonly Dictionary<Label, DateTime> messagesExpiration = new Dictionary<Label, DateTime>();
        private readonly List<Label> messagesToDelete = new List<Label>();
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
        private readonly UnitType workerType;
        private readonly Dictionary<Unit, bool> workersActivityState = new Dictionary<Unit, bool>();
        private readonly Button idleWorkerButton;
        #endregion

        private readonly Match match;
        private readonly List<ActionEnabler> enablers = new List<ActionEnabler>();
        private readonly UserInputManager userInputManager;
        private readonly TextureManager textureManager;
        private readonly ActionFrame actions;
        private UnitType selectedType;
        private Frame diplomacyFrame;
        private bool isSpaceDown;
        private bool isShiftDown;
        private Dictionary<Faction, DropdownList<DiplomaticStance>> assocFactionDropList = new Dictionary<Faction, DropdownList<DiplomaticStance>>();
        #endregion

        #region Constructors
        public MatchUI(Match match, SlaveCommander localCommander)
        {
            Argument.EnsureNotNull(match, "match");
            Argument.EnsureNotNull(localCommander, "localCommander");

            this.match = match;
            this.match.Quitting += Quit;
            this.userInputManager = new UserInputManager(localCommander);
            this.textureManager = new TextureManager();
            World world = match.World;

            matchRenderer = new MatchRenderer(userInputManager, textureManager);
            world.Entities.Removed += userInputManager.SelectionManager.EntityDied;
            Rectangle worldFrame = Instant.CreateComponentRectangle(Bounds, new Vector2(0, 0.29f), new Vector2(1, 1));
            worldView = new ClippedView(worldFrame, world.Bounds, matchRenderer);
            worldView.Bounds = new Rectangle(40, 20);
            worldView.MinimumVisibleBounds = new Rectangle(8, 4);
            Children.Add(worldView);

            Rectangle resourceDisplayFrame = Instant.CreateComponentRectangle(Bounds, new Vector2(0, 0.96f), new Vector2(1, 1));
            ResourceDisplay resourceDisplay = new ResourceDisplay(resourceDisplayFrame, userInputManager.Commander.Faction);
            Children.Add(resourceDisplay);

            Rectangle hudRectangle = Instant.CreateComponentRectangle(Bounds, new Vector2(0, 0), new Vector2(1, 0.29f));
            hudFrame = new Frame(hudRectangle);
            Children.Add(hudFrame);

            Rectangle selectionFrameRectangle = Instant.CreateComponentRectangle(hudFrame.Bounds, new Vector2(0.25f, 0), new Vector2(0.75f, 1));
            selectionFrame = new Frame(selectionFrameRectangle, Color.DarkGray);
            hudFrame.Children.Add(selectionFrame);

            Rectangle actionsRectangle = Instant.CreateComponentRectangle(hudFrame.Bounds, new Vector2(0.75f, 0), new Vector2(1, 1));
            actions = new ActionFrame(actionsRectangle);
            hudFrame.Children.Add(actions);

            Rectangle minimapRectangle = Instant.CreateComponentRectangle(hudFrame.Bounds, new Vector2(0.02f, 0.08f), new Vector2(0.23f, 0.92f));
            minimapFrame = new Frame(minimapRectangle, matchRenderer.MinimapRenderer);
            minimapFrame.Bounds = world.Bounds;
            hudFrame.Children.Add(minimapFrame);

            CreateScrollers();

            Rectangle chatInputFrame = Instant.CreateComponentRectangle(Bounds, new Vector2(0.04f, 0.3f), new Vector2(0.915f, 0.34f));
            chatInput = new TextField(chatInputFrame);
            chatInput.Triggered += SendMessage;
            chatInput.KeyDown += ChatInputKeyDown;
            Rectangle messagesFrame = Instant.CreateComponentRectangle(Bounds, new Vector2(0.005f, 0.35f), new Vector2(0.5f, 0.9f));
            chatMessages = new TransparentFrame(messagesFrame);
            Children.Add(chatMessages);

            Rectangle pausePanelRectangle = Instant.CreateComponentRectangle(Bounds, new Vector2(0.33f, 0.33f), new Vector2(0.66f, 0.66f));
            pausePanel = new Frame(pausePanelRectangle);

            Rectangle quitGameRectangle = Instant.CreateComponentRectangle(pausePanel.Bounds, new Vector2(0.25f, 0.56f), new Vector2(0.75f, 0.86f));
            Button quitGame = new Button(quitGameRectangle, "Quit");
            quitGame.Triggered += button => match.Quit();

            Rectangle resumeGameRectangle = Instant.CreateComponentRectangle(pausePanel.Bounds, new Vector2(0.25f, 0.14f), new Vector2(0.75f, 0.42f));
            Button resumeGame = new Button(resumeGameRectangle, match.IsPausable ? "Resume" : "Return");
            resumeGame.Triggered += button => HidePausePanel();

            pausePanel.Children.Add(quitGame);
            pausePanel.Children.Add(resumeGame);

            KeyDown += userInputManager.HandleKeyDown;
            KeyUp += userInputManager.HandleKeyUp;
            worldView.BoundsChanged += WorldViewBoundsChanged;

            userInputManager.SelectionManager.SelectionChanged += SelectionChanged;
            userInputManager.SelectionManager.SelectionCleared += SelectionCleared;
            localCommander.CommandGenerated += CommanderGeneratedCommand;
            minimapFrame.MouseDown += MinimapMouseDown;
            minimapFrame.MouseMoved += MinimapMouseMove;

            enablers.Add(new AttackEnabler(userInputManager, actions, textureManager));
            enablers.Add(new BuildEnabler(userInputManager, actions, world.UnitTypes, textureManager));
            enablers.Add(new HarvestEnabler(userInputManager, actions, textureManager));
            enablers.Add(new MoveEnabler(userInputManager, actions, textureManager));
            enablers.Add(new TrainEnabler(userInputManager, actions, world.UnitTypes, textureManager));
            enablers.Add(new HealEnabler(userInputManager, actions, world.UnitTypes, textureManager));

            this.workerType = World.UnitTypes.FromName("Schtroumpf");
            Rectangle inactiveWorkerRectangle = Instant.CreateComponentRectangle(Bounds, new Vector2(0.005f, 0.3f), new Vector2(0.035f, 0.34f));
            Texture workerTexture = textureManager.GetUnit(workerType.Name);
            TexturedFrameRenderer workerButtonRenderer = new TexturedFrameRenderer(workerTexture, Color.White, Color.Gray, Color.LightGray);
            this.idleWorkerButton = new Button(inactiveWorkerRectangle, "", workerButtonRenderer);
            this.idleWorkerButton.Triggered += OnIdleSmurfsButtonTriggered;

            world.Entities.Added += EntityAdded;
        }
        #endregion

        #region Properties
        private UnitType SelectedType
        {
            get { return selectedType; }
            set
            {
                selectedType = value;
                UpdateSkillsPanel();
            }
        }

        private MatchRenderer MatchRenderer
        {
            get { return (MatchRenderer)worldView.Renderer; }
        }

        private WorldRenderer WorldRenderer
        {
            get { return MatchRenderer.WorldRenderer; }
        }

        private SlaveCommander LocalCommander
        {
            get { return userInputManager.Commander; }
        }

        private Faction LocalFaction
        {
            get { return LocalCommander.Faction; }
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
            Rectangle northFrame = Instant.CreateComponentRectangle(Bounds, new Vector2(0, 0.98f), new Vector2(1, 1));
            Rectangle southFrame = Instant.CreateComponentRectangle(Bounds, new Vector2(0, 0), new Vector2(1, 0.02f));
            Rectangle eastFrame = Instant.CreateComponentRectangle(Bounds, new Vector2(0.98f, 0), new Vector2(1, 1));
            Rectangle westFrame = Instant.CreateComponentRectangle(Bounds, new Vector2(0, 0), new Vector2(0.02f, 1));
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
            DisplayMessage(text, message.Faction.Color);
        }

        public void DisplayMessage(string text, Color color)
        {
            Argument.EnsureNotNull(text, "text");

            Label messageLabel = new Label(text);
            messageLabel.Color = color;
            messagesExpiration[messageLabel] = DateTime.UtcNow + messageTimeToLive;
            float height = messageLabel.Frame.Height;
            foreach (Label writtenMessage in chatMessages.Children)
                writtenMessage.Frame = writtenMessage.Frame.TranslatedBy(0, height);
            chatMessages.Children.Add(messageLabel);
        }

        public void DisplayDefeatMessage(Faction faction)
        {
            Argument.EnsureNotNull(faction, "faction");
            DisplayMessage("{0} was defeated.".FormatInvariant(faction.Name), faction.Color);

            if (faction == LocalFaction)
            {
                if(match.IsPausable)
                    match.Pause();
                Instant.DisplayAlert(this, "You have lost the match.", () => Parent.PopDisplay(this));
            }
        }

        public void DisplayVictoryMessage(Faction faction)
        {
            Argument.EnsureNotNull(faction, "faction");
            if (faction == LocalFaction)
            {
                if(match.IsPausable)
                    match.Pause();
                Instant.DisplayAlert(this, "VICTORY!", () => Parent.PopDisplay(this));
            }
        }
        #endregion

        #region Event Handling
        private void EntityAdded(EntityManager manager, Entity entity)
        {
            Unit unit = entity as Unit;
            if (unit == null || unit.Faction != LocalFaction) return;

            if (unit.Type == workerType)
            {
                workersActivityState[unit] = true;
                unit.TaskQueue.Changed += OnSmurfTaskChanged;
                unit.Died += OnSmurfDied;
            }
        }

        private void OnSmurfTaskChanged(TaskQueue queue)
        {
            workersActivityState[queue.Unit] = queue.IsEmpty;
        }

        private void OnSmurfDied(Entity smurfAsEntity)
        {
            Unit smurf = (Unit)smurfAsEntity;
            workersActivityState.Remove(smurf);
        }

        private void OnIdleSmurfsButtonTriggered(Button sender)
        {
            IEnumerable<Unit> smurfs = workersActivityState.Where(kp => kp.Value).Select(kp => kp.Key);
            if (isShiftDown)
            {
                userInputManager.SelectionManager.SelectUnits(smurfs);
            }
            else
            {
                IEnumerable<Unit> selectedUnits = userInputManager.SelectionManager.SelectedUnits;
                if (selectedUnits.Count() == 1)
                {
                    Unit selectedUnit = selectedUnits.First();
                    if (smurfs.Contains(selectedUnit))
                    {
                        int nextIndex = (smurfs.IndexOf(selectedUnit) + 1) % smurfs.Count();
                        userInputManager.SelectionManager.SelectUnit(smurfs.Skip(nextIndex).First());
                        CenterOnSelection();
                        return;
                    }
                }
                userInputManager.SelectionManager.SelectUnit(smurfs.First());
                CenterOnSelection();
            }
        }

        protected override bool OnMouseWheel(MouseEventArgs args)
        {
            double scale = 1 - (args.WheelDelta / 600.0);
            worldView.Zoom(scale, Rectangle.ConvertPoint(Bounds, worldView.Bounds, args.Position));
            return base.OnMouseWheel(args);
        }

        protected override bool OnMouseDown(MouseEventArgs args)
        {
            if (worldView.Frame.ContainsPoint(args.Position))
            {
                Vector2 newPosition = Rectangle.ConvertPoint(worldView.Frame, worldView.Bounds, args.Position);
                userInputManager.HandleMouseDown(this, new MouseEventArgs(newPosition.X, newPosition.Y, args.ButtonPressed, args.Clicks, args.WheelDelta));
            }
            return base.OnMouseDown(args);
        }

        protected override bool OnMouseMove(MouseEventArgs args)
        {
            if (worldView.Frame.ContainsPoint(args.Position) || (Control.MouseButtons & MouseButtons.Left) != 0)
            {
                Vector2 newPosition = Rectangle.ConvertPoint(worldView.Frame, worldView.Bounds, args.Position);
                userInputManager.HandleMouseMove(this, new MouseEventArgs(newPosition.X, newPosition.Y, args.ButtonPressed, args.Clicks, args.WheelDelta));
            }
            else
                userInputManager.SelectionManager.HoveredUnit = null;
            return base.OnMouseMove(args);
        }

        protected override bool OnMouseUp(MouseEventArgs args)
        {
            Vector2 newPosition = Rectangle.ConvertPoint(worldView.Frame, worldView.Bounds, args.Position);
            userInputManager.HandleMouseUp(this, new MouseEventArgs(newPosition.X, newPosition.Y, args.ButtonPressed, args.Clicks, args.WheelDelta));
            mouseDownOnMinimap = false;
            return base.OnMouseUp(args);
        }

        private void SendMessage(TextField chatInput)
        {
            string text = chatInput.Contents;
            if (text.Length > 0)
            {
                SlaveCommander commander = userInputManager.Commander;
                commander.SendMessage(chatInput.Contents);
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

        protected override bool OnKeyDown(KeyboardEventArgs args)
        {
            isShiftDown = args.HasShift;
            MatchRenderer.DrawAllHealthBars = args.HasAlt;
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
            return base.OnKeyDown(args);
        }

        protected override bool OnKeyPress(char character)
        {
            if (character == '\r')
            {
                chatInput.Clear();
                Children.Add(chatInput);
            }
            return base.OnKeyPress(character);
        }

        protected override bool OnDoubleClick(MouseEventArgs args)
        {
            Vector2 newPosition = Rectangle.ConvertPoint(worldView.Frame, worldView.Bounds, args.Position);
            userInputManager.HandleMouseDoubleClick(this, new MouseEventArgs(newPosition.X, newPosition.Y, args.ButtonPressed, args.Clicks, args.WheelDelta));
            return base.OnDoubleClick(args);
        }

        protected override bool OnKeyUp(KeyboardEventArgs args)
        {
            isShiftDown = args.HasShift;
            (worldView.Renderer as MatchRenderer).DrawAllHealthBars = args.HasAlt;
            isSpaceDown = (args.Key != Keys.Space && isSpaceDown);
            return base.OnKeyUp(args);
        }

        protected override void OnUpdate(UpdateEventArgs args)
        {
            if (isSpaceDown && SelectedType != null)
            {
                CenterOnSelection();
            }
            DateTime now = DateTime.UtcNow;
            messagesToDelete.AddRange(messagesExpiration.Where(pair => pair.Value <= now).Select(pair => pair.Key));
            foreach (Label toDelete in messagesToDelete)
            {
                messagesExpiration.Remove(toDelete);
                toDelete.Dispose();
            }
            messagesToDelete.Clear();

            bool shouldDisplaySmurfButton = workersActivityState.Values.Any(value => value == true);
            if (Children.Contains(idleWorkerButton) && !shouldDisplaySmurfButton)
                Children.Remove(idleWorkerButton);

            if(!Children.Contains(idleWorkerButton) && shouldDisplaySmurfButton)
                Children.Add(idleWorkerButton);

            match.Update(args.TimeDeltaInSeconds);
            base.OnUpdate(args);
        }

        private void Quit(Match sender)
        {
            Parent.PopDisplay(this);
        }

        private void WorldViewBoundsChanged(View sender, Rectangle newBounds)
        {
            Vector2 boundsHalfsize = new Vector2(newBounds.Width / 2, newBounds.Height / 2);
            worldView.FullBounds = userInputManager.Commander.Faction.World.Bounds
                .TranslatedBy(-boundsHalfsize.X, -boundsHalfsize.Y).ResizedBy(newBounds.Width, newBounds.Height);

            if (worldView.IsMouseOver)
            {
                Vector2 newPosition = Rectangle.ConvertPoint(worldView.Frame, worldView.Bounds, worldView.MousePosition.Value);
                userInputManager.HandleMouseMove(this, new MouseEventArgs(newPosition.X, newPosition.Y, MouseButton.None, 0, 0));
            }
        }

        private void MinimapMouseDown(Responder source, MouseEventArgs args)
        {
            if (args.ButtonPressed == MouseButton.Left)
            {
                if (userInputManager.SelectedCommand != null) userInputManager.LaunchMouseCommand(args.Position);
                else
                {
                    MoveWorldView(args.Position);
                    mouseDownOnMinimap = true;
                }
            }
            else if (args.ButtonPressed == MouseButton.Right)
            {
                userInputManager.LaunchDefaultCommand(args.Position);
            }
        }

        private void MinimapMouseMove(Responder source, MouseEventArgs args)
        {
            if (mouseDownOnMinimap) MoveWorldView(args.Position);
        }

        private void SelectionCleared(SelectionManager selectionManager)
        {
            SelectedType = null;
        }

        private void SelectionChanged(SelectionManager selectionManager)
        {
            while (selectionFrame.Children.Count > 0) selectionFrame.Children[0].Dispose();
            selectionFrame.Children.Clear();

            IEnumerable<Unit> selection = selectionManager.SelectedUnits;
            int selectionCount = selection.Count();
            if (SelectedType == null && selectionCount > 0) SelectedType = selection.First().Type;

            if (selectionCount == 0) SelectedType = null;
            if (selectionCount == 1) CreateSingleUnitSelectionPanel();
            else CreateMultipleUnitsSelectionPanel();
        }

        private void CommanderGeneratedCommand(Commander commander, Command command)
        {
            actions.Restore();
        }

        private void AcceptNewDiplomacy(Button bouton)
        {
            foreach (var pair in assocFactionDropList)
            {
                if (LocalFaction.GetDiplomaticStance(pair.Key) != pair.Value.SelectedItem)
                {
                    LocalCommander.LaunchChangeDiplomacy(pair.Key);
                }
            }

            // remove diplomacy pannel from view.
            assocFactionDropList.Clear();
            bouton.Parent.RemoveFromParent();
        }
        #endregion

        #region IUIDisplay Implementation
        internal override void OnShadow(RootView shadowedFrom)
        {
            shadowedFrom.PopDisplay(this);
            base.OnShadow(shadowedFrom);
        }
        #endregion

        #region Methods
        public void CenterOn(Vector2 position)
        {
            Vector2 halfWorldBoundsSize = worldView.Bounds.Size;
            halfWorldBoundsSize.Scale(0.5f, 0.5f);
            worldView.Bounds = worldView.Bounds.TranslatedTo(position - halfWorldBoundsSize);
        }

        private void CenterOnSelection()
        {
            Unit unitToFollow = userInputManager.SelectionManager.SelectedUnits.First(unit => unit.Type == SelectedType);
            CenterOn(unitToFollow.Position);
        }

        private void CreateSingleUnitSelectionPanel()
        {
            Unit unit = userInputManager.SelectionManager.SelectedUnits.First();
            selectionFrame.Renderer = new UnitFrameRenderer(unit, textureManager);
            UnitButtonRenderer buttonRenderer = new UnitButtonRenderer(unit, textureManager);
            Button unitButton = new Button(new Rectangle(10, 10, 130, 175), "", buttonRenderer);
            float aspectRatio = Bounds.Width / Bounds.Height;
            unitButton.Bounds = new Rectangle(3f, 3f * aspectRatio);

            unitButton.Triggered += ButtonPress;
            selectionFrame.Children.Add(unitButton);
        }

        private void CreateMultipleUnitsSelectionPanel()
        {
            selectionFrame.Renderer = new FilledFrameRenderer(Color.DarkGray, Color.Gray);
            const float padding = 10;
            Rectangle frame = new Rectangle(selectionFrame.Bounds.Width / 7 - padding * 2, selectionFrame.Bounds.Height / 2 - padding * 2);
            float currentX = padding + selectionFrame.Bounds.MinX;
            float currentY = selectionFrame.Bounds.Height - padding - frame.Height;
            foreach (Unit unit in userInputManager.SelectionManager.SelectedUnits)
            {
                UnitButtonRenderer renderer = new UnitButtonRenderer(unit, textureManager);
                renderer.HasFocus = unit.Type == SelectedType;
                Button unitButton = new Button(frame.TranslatedTo(currentX, currentY), "", renderer);
                float aspectRatio = Bounds.Width / Bounds.Height;
                unitButton.Bounds = new Rectangle(3f, 3f * aspectRatio);
                unitButton.Triggered += ButtonPress;
                currentX += frame.Width + padding;
                if (currentX + frame.Width > selectionFrame.Bounds.MaxX)
                {
                    currentY -= frame.Height + padding;
                    currentX = padding + selectionFrame.Bounds.MinX;
                }
                selectionFrame.Children.Add(unitButton);
            }
        }

        private void ButtonPress(Button button)
        {
            if (button.Renderer is UnitButtonRenderer)
            {
                Unit unit = (button.Renderer as UnitButtonRenderer).unit;
                IEnumerable<Unit> selectedUnits = userInputManager.SelectionManager.SelectedUnits;
                if (unit.Type == SelectedType || selectedUnits.Count() == 1)
                {
                    userInputManager.SelectionManager.SelectUnit(unit);
                    MoveWorldView(unit.Position);
                }
                else
                {
                    SelectedType = unit.Type;
                    foreach (Button unitButton in selectionFrame.Children)
                    {
                        UnitButtonRenderer renderer = (UnitButtonRenderer)unitButton.Renderer;
                        renderer.HasFocus = renderer.unit.Type == SelectedType;
                    }
                }
            } 
        }

        private void UpdateSkillsPanel()
        {
            actions.Clear();
            if (SelectedType != null)
            {
                IEnumerable<Unit> selectedUnits = userInputManager.SelectionManager.SelectedUnits;
                if (selectedUnits.Count(u => u.Faction != userInputManager.Commander.Faction) == 0)
                    actions.Push(new UnitActionProvider(enablers, SelectedType));
            }
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
            if (!Children.Contains(pausePanel))
            {
                match.Pause();
                if (!match.IsRunning)
                    foreach (Scroller scroller in Children.OfType<Scroller>()) scroller.Enabled = false;
                Children.Add(pausePanel);
            }
        }

        private void HidePausePanel()
        {
            foreach (Scroller scroller in Children.OfType<Scroller>())
            {
                scroller.Enabled = true;
            }
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
            foreach (Faction faction in World.Factions)
            {
                if (faction == LocalFaction) continue;

                Frame frameFaction = new Frame(rectangleFrame, faction.Color);
                
                Rectangle rectangleFaction = Instant.CreateComponentRectangle(frameFaction.Bounds,new Vector2(0.7f,0.7f), new Vector2(1f,1f));
                DropdownList<DiplomaticStance> dropdownList = new DropdownList<DiplomaticStance>(rectangleFaction);
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
            Button boutonAccepter = new Button(rectangleButton, "Accepter");
            boutonAccepter.Triggered += AcceptNewDiplomacy;
            diplomacyFrame.Children.Add(boutonAccepter);
        }

        public override void Dispose()
        {
            textureManager.Dispose();
            matchRenderer.Dispose();
            base.Dispose();
        }
        #endregion
        #endregion
    }
}
