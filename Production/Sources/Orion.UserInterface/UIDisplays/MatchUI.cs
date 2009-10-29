using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Keys = System.Windows.Forms.Keys;
using Color = System.Drawing.Color;

using Orion.Commandment;
using Orion.GameLogic;
using Orion.Geometry;
using Orion.Graphics;
using Orion.UserInterface.Widgets;

using OpenTK.Math;

namespace Orion.UserInterface
{
    public class MatchUI : UIDisplay
    {
        #region Fields
        private readonly UserInputCommander userInputCommander;
        private readonly ClippedView worldView;
        private readonly Frame hudFrame;
        private readonly Frame selectionFrame;
        private UnitType selectedType;

        #region Event Handling Delegates
        private GenericEventHandler<SelectionManager> selectionChanged;
        #endregion
        #endregion

        #region Constructors

        public MatchUI(World world, UserInputCommander commander)
        {
            Argument.EnsureNotNull(world, "world");
            Argument.EnsureNotNull(commander, "commander");

            userInputCommander = commander;

            world.Units.UnitDied += userInputCommander.SelectionManager.UnitDied;
            Rectangle worldFrame = Bounds.Resize(0, -Bounds.Height / 25).Resize(0, -Bounds.Height / 4).Translate(0, Bounds.Height / 4);
            worldView = new ClippedView(worldFrame, world.Bounds, new MatchRenderer(world, commander));
            worldView.Bounds = new Rectangle(40, 20);
            Children.Add(worldView);

            Rectangle resourceDisplayFrame = new Rectangle(0, Bounds.Height, Bounds.Width, -Bounds.Height / 25);
            ResourceDisplay resourceDisplay = new ResourceDisplay(resourceDisplayFrame, userInputCommander.Faction);
            Children.Add(resourceDisplay);

            hudFrame = new Frame(new Rectangle(Bounds.Width, Bounds.Height / 4), Color.DarkGray);
            Children.Add(hudFrame);

            selectionFrame = new Frame(new Rectangle(Bounds.Width / 4, 0, Bounds.Width / 2, hudFrame.Frame.Height), Color.DarkGray);
            hudFrame.Children.Add(selectionFrame);

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
            else if (selectedType == null)
                selectedType = selectionManager.SelectedUnits.First().Type;
        }

        private void ButtonPress(Button button)
        {
            if (button.Renderer is UnitButtonRenderer)
            {
                Unit unit = (button.Renderer as UnitButtonRenderer).Unit;
                if (unit.Type == selectedType)
                    userInputCommander.SelectionManager.SelectUnit(unit);
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
        #endregion

        #region IUIDisplay Implementation
        internal override void OnEnter(RootView into)
        {
            userInputCommander.SelectionManager.SelectionChanged += selectionChanged;
        }

        internal override void OnShadow(RootView shadowedFrom)
        {
            userInputCommander.SelectionManager.SelectionChanged -= selectionChanged;
            shadowedFrom.PopDisplay(this);
        }
        #endregion
        #endregion
    }
}
