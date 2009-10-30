using System.Collections.Generic;
using System.Linq;
using OpenTK.Math;
using Orion.Commandment;
using Orion.GameLogic;
using Orion.Geometry;
using Orion.Graphics;
using Orion.UserInterface.Widgets;
using Color = System.Drawing.Color;
using Keys = System.Windows.Forms.Keys;

namespace Orion.UserInterface
{
    public class MatchUI : UIDisplay
    {
        #region Fields
        private readonly UserInputCommander userInputCommander;
        private readonly ClippedView worldView;
        private readonly Frame hudFrame;
        private readonly Frame selectionFrame;
        private readonly Frame minimapFrame;
        private UnitType selectedType;

        #region Event Handling
        private GenericEventHandler<SelectionManager> selectionChanged;
        private GenericEventHandler<Responder, MouseEventArgs> minimapMouseDown;
        private GenericEventHandler<Responder, MouseEventArgs> minimapMouseMove;
        private GenericEventHandler<Responder, MouseEventArgs> minimapMouseUp;
        private bool minimapLeftButtonDown;
        #endregion
        #endregion

        #region Constructors

        public MatchUI(World world, UserInputCommander commander)
        {
            Argument.EnsureNotNull(world, "world");
            Argument.EnsureNotNull(commander, "commander");

            userInputCommander = commander;

            MatchRenderer matchRenderer = new MatchRenderer(world, commander);
            world.Units.UnitDied += userInputCommander.SelectionManager.UnitDied;
            Rectangle worldFrame = Bounds.Resize(0, -Bounds.Height / 25).Resize(0, -Bounds.Height / 4).Translate(0, Bounds.Height / 4);
            worldView = new ClippedView(worldFrame, world.Bounds, matchRenderer);
            worldView.Bounds = new Rectangle(40, 20);
            Children.Add(worldView);

            Rectangle resourceDisplayFrame = new Rectangle(0, Bounds.Height, Bounds.Width, -Bounds.Height / 25);
            ResourceDisplay resourceDisplay = new ResourceDisplay(resourceDisplayFrame, userInputCommander.Faction);
            Children.Add(resourceDisplay);

            hudFrame = new Frame(new Rectangle(Bounds.Width, Bounds.Height / 4), Color.DarkGray);
            Children.Add(hudFrame);

            selectionFrame = new Frame(new Rectangle(hudFrame.Bounds.Width / 4, 0, hudFrame.Bounds.Width / 2, hudFrame.Frame.Height), Color.DarkGray);
            hudFrame.Children.Add(selectionFrame);

            minimapFrame = new Frame(new Rectangle(hudFrame.Bounds.Width / 4, hudFrame.Bounds.Height), matchRenderer.MinimapRenderer);
            minimapFrame.Bounds = world.Bounds;
            hudFrame.Children.Add(minimapFrame);

            Rectangle northFrame = new Rectangle(0, Bounds.Height, Bounds.Width, -20);
            Rectangle southFrame = new Rectangle(0, 0, Bounds.Width, 20);
            Rectangle eastFrame = new Rectangle(Bounds.Width, 0, -20, Bounds.Height);
            Rectangle westFrame = new Rectangle(0, 0, 20, Bounds.Height);
            Scroller northScroller = new Scroller(worldView, northFrame, new Vector2(0, 1), Keys.Up);
            Scroller southScroller = new Scroller(worldView, southFrame, new Vector2(0, -1), Keys.Down);
            Scroller eastScroller = new Scroller(worldView, eastFrame, new Vector2(1, 0), Keys.Right);
            Scroller westScroller = new Scroller(worldView, westFrame, new Vector2(-1, 0), Keys.Left);

            Children.Add(northScroller);
            Children.Add(southScroller);
            Children.Add(eastScroller);
            Children.Add(westScroller);

            selectionChanged = SelectionChanged;
            minimapMouseDown = MinimapMouseDown;
            minimapMouseMove = MinimapMouseMove;
            minimapMouseUp = MinimapMouseUp;
        }

        #endregion

        #region Methods
        #region Event Handling
        protected override bool OnMouseWheel(MouseEventArgs args)
        {
            double scale = 1 - (args.WheelDelta / 600.0);
            worldView.Zoom(scale, Rectangle.ConvertPoint(Bounds, worldView.Bounds, args.Position));
            return base.OnMouseWheel(args);
        }

        protected override bool OnMouseDown(MouseEventArgs args)
        {
            userInputCommander.OnMouseButton(Rectangle.ConvertPoint(worldView.Frame, worldView.Bounds, args.Position), args.ButtonPressed, true);
            return base.OnMouseDown(args);
        }

        protected override bool OnMouseUp(MouseEventArgs args)
        {
            userInputCommander.OnMouseButton(Rectangle.ConvertPoint(worldView.Frame, worldView.Bounds, args.Position), args.ButtonPressed, false);
            return base.OnMouseUp(args);
        }

        protected override bool OnMouseMove(MouseEventArgs args)
        {
            userInputCommander.OnMouseMove(Rectangle.ConvertPoint(worldView.Frame, worldView.Bounds, args.Position));
            return base.OnMouseMove(args);
        }

        protected override bool OnKeyDown(KeyboardEventArgs args)
        {
            userInputCommander.OnKeyDown(args.Key);
            return base.OnKeyDown(args);
        }

        protected override bool OnKeyUp(KeyboardEventArgs args)
        {
            userInputCommander.OnKeyUp(args.Key);
            return base.OnKeyUp(args);
        }

        private void MinimapMouseDown(Responder source, MouseEventArgs args)
        {
            if (args.ButtonPressed == MouseButton.Left)
            {
                MoveWorldView(args.Position);
                minimapLeftButtonDown = true;
            }
            else if (args.ButtonPressed == MouseButton.Right)
            {
                userInputCommander.OnMouseButton(args.Position, MouseButton.Right, true);
            }
        }

        private void MinimapMouseMove(Responder source, MouseEventArgs args)
        {
            if (minimapLeftButtonDown) MoveWorldView(args.Position);
        }

        private void MinimapMouseUp(Responder source, MouseEventArgs args)
        {
            minimapLeftButtonDown = false;
        }

        private void SelectionChanged(SelectionManager selectionManager)
        {
            foreach (Button button in selectionFrame.Children)
            {
                button.Dispose();
            }
            selectionFrame.Children.Clear();

            const float padding = 10;
            Rectangle frame = new Rectangle(selectionFrame.Bounds.Width / 7 - padding * 2, selectionFrame.Bounds.Height / 2 - padding * 2);
            float currentX = padding + selectionFrame.Bounds.X;
            float currentY = selectionFrame.Bounds.Height - padding - frame.Height;
            UnitRenderer unitRenderer = (worldView.Renderer as MatchRenderer).WorldRenderer.UnitRenderer;
            foreach (Unit unit in selectionManager.SelectedUnits)
            {
                UnitButtonRenderer renderer = new UnitButtonRenderer(unitRenderer.GetTypeShape(unit.Type), unit);
                Button unitButton = new Button(frame.TranslateTo(currentX, currentY), "", renderer);
                float aspectRatio = Bounds.Width / Bounds.Height;
                unitButton.Bounds = new Rectangle(3f, 3f * aspectRatio);
                unitButton.Pressed += ButtonPress;
                currentX += frame.Width + padding;
                if (currentX + frame.Width > selectionFrame.Bounds.MaxX)
                {
                    currentY -= frame.Height + padding;
                    currentX = padding + selectionFrame.Bounds.X;
                }
                selectionFrame.Children.Add(unitButton);
            }

            if (selectionFrame.Children.Count == 0)
                selectedType = null;
            else if (selectedType == null &&
                selectionManager.SelectedUnits.Select(u => u.Type).Distinct().Count() == 1)
                selectedType = selectionManager.SelectedUnits.First().Type;
        }

        private void ButtonPress(Button button)
        {
            if (button.Renderer is UnitButtonRenderer)
            {
                Unit unit = (button.Renderer as UnitButtonRenderer).Unit;
                IEnumerable<Unit> selectedUnits = userInputCommander.SelectionManager.SelectedUnits;
                if (unit.Type == selectedType || selectedUnits.Count() == 1)
                {
                    userInputCommander.SelectionManager.SelectUnit(unit);
                    MoveWorldView(unit.Position);
                    selectedType = null;
                }
                else
                {
                    selectedType = unit.Type;
                    foreach (Button unitButton in selectionFrame.Children)
                    {
                        UnitButtonRenderer renderer = unitButton.Renderer as UnitButtonRenderer;
                        renderer.HasFocus = renderer.Unit.Type == selectedType;
                    }
                }
            }
        }

        private void MoveWorldView(Vector2 center)
        {
            Vector2 difference = worldView.Bounds.Origin - worldView.Bounds.Center;
            Rectangle newBounds = worldView.Bounds.TranslateTo(center + difference);
            float xDiff = worldView.FullBounds.MaxX - newBounds.MaxX;
            float yDiff = worldView.FullBounds.MaxY - newBounds.MaxY;
            if (xDiff < 0) newBounds = newBounds.TranslateX(xDiff);
            if (yDiff < 0) newBounds = newBounds.TranslateY(yDiff);
            if (newBounds.X < 0) newBounds = newBounds.TranslateTo(0, newBounds.Origin.Y);
            if (newBounds.Y < 0) newBounds = newBounds.TranslateTo(newBounds.Origin.X, 0);
            worldView.Bounds = newBounds;
        }
        #endregion

        #region IUIDisplay Implementation
        internal override void OnEnter(RootView into)
        {
            userInputCommander.SelectionManager.SelectionChanged += selectionChanged;
            minimapFrame.MouseDown += minimapMouseDown;
            minimapFrame.MouseMoved += minimapMouseMove;
            minimapFrame.MouseUp += minimapMouseUp;
        }

        internal override void OnShadow(RootView shadowedFrom)
        {
            userInputCommander.SelectionManager.SelectionChanged -= selectionChanged;
            minimapFrame.MouseDown -= minimapMouseDown;
            minimapFrame.MouseMoved -= minimapMouseMove;
            minimapFrame.MouseUp -= minimapMouseUp;
            shadowedFrom.PopDisplay(this);
        }
        #endregion
        #endregion
    }
}
