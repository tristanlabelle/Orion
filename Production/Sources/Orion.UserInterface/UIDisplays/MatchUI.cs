using System;
using System.Collections.Generic;
using System.Linq;
using OpenTK.Math;
using Orion.Commandment;
using Orion.GameLogic;
using Orion.Geometry;
using Orion.Graphics;
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
        private readonly Dictionary<Label, DateTime> messagesExpiration = new Dictionary<Label,DateTime>();
        private readonly List<Label> messagesToDelete = new List<Label>();
        #endregion

        #region General UI
        private readonly ClippedView worldView;
        private readonly Frame hudFrame;
        private readonly Frame selectionFrame;
        #endregion

        #region Minimap
        private readonly Frame minimapFrame;
        private bool mouseDownOnMinimap;
        #endregion

        private readonly List<ActionEnabler> enablers = new List<ActionEnabler>();
        private readonly Match match;
        private readonly UserInputManager userInputManager;
        private readonly ActionFrame actions;
        private UnitType selectedType;
        private bool isSpaceDown;
        #endregion

        #region Constructors

        public MatchUI(Match match)
        {
            Argument.EnsureNotNull(match, "match");

            this.match = match;
            userInputManager = new UserInputManager(match.UserCommander);
            World world = match.World;

            MatchRenderer matchRenderer = new MatchRenderer(world, userInputManager);
            world.Entities.Died += userInputManager.SelectionManager.EntityDied;
            Rectangle worldFrame = Instant.CreateComponentRectangle(Bounds, new Vector2(0, 0.25f), new Vector2(1, 1));
            worldView = new ClippedView(worldFrame, world.Bounds, matchRenderer);
            worldView.Bounds = new Rectangle(40, 20);
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

            Rectangle chatInputFrame = Instant.CreateComponentRectangle(Bounds, new Vector2(0.025f, 0.3f), new Vector2(0.5f, 0.34f));
            chatInput = new TextField(chatInputFrame);
            Rectangle messagesFrame = Instant.CreateComponentRectangle(Bounds, new Vector2(0.025f, 0.35f), new Vector2(0.5f, 0.8f));
            chatMessages = new TransparentFrame(messagesFrame);
            Children.Add(chatMessages);

            KeyDown += userInputManager.HandleKeyDown;
            KeyUp += userInputManager.HandleKeyUp;
            worldView.BoundsChanged += WorldViewBoundsChanged;

            userInputManager.SelectionManager.SelectionChanged += SelectionChanged;
            userInputManager.SelectionManager.SelectionCleared += SelectionCleared;
            match.UserCommander.CommandGenerated += CommanderGeneratedCommand;
            minimapFrame.MouseDown += MinimapMouseDown;
            minimapFrame.MouseMoved += MinimapMouseMove;

            enablers.Add(new AttackEnabler(userInputManager, actions));
            enablers.Add(new BuildEnabler(userInputManager, actions, world.UnitTypes));
            enablers.Add(new HarvestEnabler(userInputManager, actions));
            enablers.Add(new MoveEnabler(userInputManager, actions));
            enablers.Add(new TrainEnabler(userInputManager, actions, world.UnitTypes));
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
        #endregion

        #region Methods
        #region Initialization
        private void CreateScrollers()
        {
            Rectangle northFrame = Instant.CreateComponentRectangle(Bounds, new Vector2(0, 0.98f), new Vector2(1, 1));
            Rectangle southFrame = Instant.CreateComponentRectangle(Bounds, new Vector2(0, 0), new Vector2(1, 0.02f));
            Rectangle eastFrame = Instant.CreateComponentRectangle(Bounds, new Vector2(0.98f, 0), new Vector2(1, 1));
            Rectangle westFrame = Instant.CreateComponentRectangle(Bounds, new Vector2(0, 0), new Vector2(0.02f, 1));
            Scroller northScroller = new Scroller(worldView, northFrame, new Vector2(0, 1), Keys.Up);
            Scroller southScroller = new Scroller(worldView, southFrame, new Vector2(0, -1), Keys.Down);
            Scroller eastScroller = new Scroller(worldView, eastFrame, new Vector2(1, 0), Keys.Right);
            Scroller westScroller = new Scroller(worldView, westFrame, new Vector2(-1, 0), Keys.Left);

            Children.Add(northScroller);
            Children.Add(southScroller);
            Children.Add(eastScroller);
            Children.Add(westScroller);
        }
        #endregion

        #region Event Handling
        public void DisplayMessage(Faction origin, string message)
        {
            Label messageLabel = new Label("{0}: {1}".FormatInvariant(origin.Name, message));
            messageLabel.Color = origin.Color;
            messagesExpiration[messageLabel] = DateTime.UtcNow + messageTimeToLive;
            float height = messageLabel.Frame.Height;
            foreach (Label writtenMessage in chatMessages.Children)
                writtenMessage.Frame = writtenMessage.Frame.TranslatedBy(0, height);
            chatMessages.Children.Add(messageLabel);
        }

        protected override bool OnMouseWheel(MouseEventArgs args)
        {
            double scale = 1 - (args.WheelDelta / 600.0);
            worldView.Zoom(scale, Rectangle.ConvertPoint(Bounds, worldView.Bounds, args.Position));
            return base.OnMouseWheel(args);
        }

        protected override bool OnMouseDown(MouseEventArgs args)
        {
            Vector2 newPosition = Rectangle.ConvertPoint(worldView.Frame, worldView.Bounds, args.Position);
            userInputManager.HandleMouseDown(this, new MouseEventArgs(newPosition.X, newPosition.Y, args.ButtonPressed, args.Clicks, args.WheelDelta));
            return base.OnMouseDown(args);
        }

        protected override bool OnMouseMove(MouseEventArgs args)
        {
            Vector2 newPosition = Rectangle.ConvertPoint(worldView.Frame, worldView.Bounds, args.Position);
            userInputManager.HandleMouseMove(this, new MouseEventArgs(newPosition.X, newPosition.Y, args.ButtonPressed, args.Clicks, args.WheelDelta));
            return base.OnMouseMove(args);
        }

        protected override bool OnMouseUp(MouseEventArgs args)
        {
            Vector2 newPosition = Rectangle.ConvertPoint(worldView.Frame, worldView.Bounds, args.Position);
            userInputManager.HandleMouseUp(this, new MouseEventArgs(newPosition.X, newPosition.Y, args.ButtonPressed, args.Clicks, args.WheelDelta));
            mouseDownOnMinimap = false;
            return base.OnMouseUp(args);
        }

        protected override bool OnKeyDown(KeyboardEventArgs args)
        {
            (worldView.Renderer as MatchRenderer).DrawAllHealthBars = args.HasAlt;
            isSpaceDown = args.Key == Keys.Space;
            if (args.Key == Keys.Enter)
            {
                if (!Children.Contains(chatInput))
                {
                    chatInput.Clear();
                    Children.Add(chatInput);
                }
                else
                {
                    UserInputCommander commander = userInputManager.Commander;
                    commander.SendMessage(chatInput.Contents);
                    Children.Remove(chatInput);
                }
            }
            if (Children.Contains(chatInput))
            {
                if (args.Key == Keys.Escape)
                {
                    chatInput.Clear();
                    Children.Remove(chatInput);
                }
                return false;
            }
            else
            {
                return base.OnKeyDown(args);
            }
        }

        protected override bool OnDoubleClick(MouseEventArgs args)
        {
            Vector2 newPosition = Rectangle.ConvertPoint(worldView.Frame, worldView.Bounds, args.Position);
            userInputManager.HandleMouseDoubleClick(this, new MouseEventArgs(newPosition.X, newPosition.Y, args.ButtonPressed, args.Clicks, args.WheelDelta));
            return base.OnDoubleClick(args);
        }

        protected override bool OnKeyUp(KeyboardEventArgs args)
        {
            (worldView.Renderer as MatchRenderer).DrawAllHealthBars = args.HasAlt;
            isSpaceDown = (args.Key != Keys.Space && isSpaceDown);
            return base.OnKeyUp(args);
        }

        protected override void OnUpdate(UpdateEventArgs args)
        {
            if (isSpaceDown && SelectedType != null && !Children.Contains(chatInput))
            {
                Unit unitToFollow = userInputManager.SelectionManager.SelectedUnits.First(unit => unit.Type == SelectedType);
                Vector2 halfWorldBoundsSize = worldView.Bounds.Size;
                halfWorldBoundsSize.Scale(0.5f, 0.5f);
                worldView.Bounds = worldView.Bounds.TranslatedTo(unitToFollow.Position - halfWorldBoundsSize);
            }
            DateTime now = DateTime.UtcNow;
            messagesToDelete.AddRange(messagesExpiration.Where(pair => pair.Value <= now).Select(pair => pair.Key));
            foreach (Label toDelete in messagesToDelete)
            {
                messagesExpiration.Remove(toDelete);
                toDelete.Dispose();
            }
            messagesToDelete.Clear();

            match.Update(args);
            base.OnUpdate(args);
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

            if (selectionCount == 1) CreateSingleUnitSelectionPanel();
            else CreateMultipleUnitsSelectionPanel();
        }

        private void CommanderGeneratedCommand(Commander commander, Command command)
        {
            actions.Restore();
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

        private void CreateSingleUnitSelectionPanel()
        {
            UnitsRenderer unitRenderer = (worldView.Renderer as MatchRenderer).WorldRenderer.UnitRenderer;
            Unit unit = userInputManager.SelectionManager.SelectedUnits.First();
            selectionFrame.Renderer = new UnitFrameRenderer(unit);
            UnitButtonRenderer buttonRenderer = new UnitButtonRenderer(unitRenderer, unit);
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
            UnitsRenderer unitRenderer = (worldView.Renderer as MatchRenderer).WorldRenderer.UnitRenderer;
            foreach (Unit unit in userInputManager.SelectionManager.SelectedUnits)
            {
                UnitButtonRenderer renderer = new UnitButtonRenderer(unitRenderer, unit);
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
                Unit unit = (button.Renderer as UnitButtonRenderer).Unit;
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
                        UnitButtonRenderer renderer = unitButton.Renderer as UnitButtonRenderer;
                        renderer.HasFocus = renderer.Unit.Type == SelectedType;
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
        #endregion
        #endregion
    }
}
