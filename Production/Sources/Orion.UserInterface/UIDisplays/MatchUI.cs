﻿using System;
using System.Collections.Generic;
using System.Linq;
using OpenTK.Math;
using Orion.Commandment;
using Orion.GameLogic;
using Orion.Geometry;
using Orion.Graphics;
using Orion.UserInterface.Widgets;
using Color = System.Drawing.Color;
using Font = System.Drawing.Font;
using Keys = System.Windows.Forms.Keys;

namespace Orion.UserInterface
{
    public class MatchUI : UIDisplay
    {
        #region Fields
        private readonly UserInputManager userInputManager;
        private readonly ClippedView worldView;
        private readonly Frame hudFrame;
        private readonly Frame selectionFrame;
        private UnitType selectedType;

        #region Minimap
        private readonly Frame minimapFrame;
        private bool mouseDownOnMinimap;
        #endregion
        #endregion

        #region Constructors

        public MatchUI(World world, UserInputCommander commander)
        {
            Argument.EnsureNotNull(world, "world");
            Argument.EnsureNotNull(commander, "commander");

            userInputManager = new UserInputManager(commander);

            MatchRenderer matchRenderer = new MatchRenderer(world, userInputManager);
            world.Units.UnitDied += userInputManager.SelectionManager.UnitDied;
            Rectangle worldFrame = Bounds.Resize(0, -Bounds.Height / 25).Resize(0, -Bounds.Height / 4).Translate(0, Bounds.Height / 4);
            worldView = new ClippedView(worldFrame, world.Bounds, matchRenderer);
            worldView.Bounds = new Rectangle(40, 20);
            Children.Add(worldView);

            Rectangle resourceDisplayFrame = new Rectangle(0, Bounds.Height, Bounds.Width, -Bounds.Height / 25);
            ResourceDisplay resourceDisplay = new ResourceDisplay(resourceDisplayFrame, userInputManager.Commander.Faction);
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

            worldView.MouseDown += userInputManager.HandleMouseDown;
            worldView.KeyDown += userInputManager.HandleKeyDown;
            worldView.KeyUp += userInputManager.HandleKeyUp;
            worldView.BoundsChanged += WorldViewBoundsChanged;

            userInputManager.SelectionManager.SelectionChanged += SelectionChanged;
            userInputManager.SelectionManager.SelectionCleared += SelectionCleared;
            minimapFrame.MouseDown += MinimapMouseDown;
            minimapFrame.MouseMoved += MinimapMouseMove;
            minimapFrame.MouseUp += MinimapMouseUp;
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
        #region Event Handling
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
            return base.OnMouseUp(args);
        }

        private void WorldViewBoundsChanged(View sender, Rectangle newBounds)
        {
            Vector2 boundsHalfsize = new Vector2(newBounds.Width / 2, newBounds.Height / 2);
            worldView.FullBounds = userInputManager.Commander.Faction.World.Bounds
                .Translate(-boundsHalfsize.X, -boundsHalfsize.Y).Resize(newBounds.Width, newBounds.Height);
        }

        private void MinimapMouseDown(Responder source, MouseEventArgs args)
        {
            if (args.ButtonPressed == MouseButton.Left)
            {
                if (userInputManager.SelectedCommand.HasValue) userInputManager.LaunchMouseCommand(args.Position);
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

        private void MinimapMouseUp(Responder source, MouseEventArgs args)
        {
            mouseDownOnMinimap = false;
        }

        private void SelectionCleared(SelectionManager selectionManager)
        {
            selectedType = null;
        }

        private void SelectionChanged(SelectionManager selectionManager)
        {
            foreach (Button button in selectionFrame.Children.OfType<Button>()) button.Dispose();
            selectionFrame.Children.Clear();

            IEnumerable<Unit> selection = selectionManager.SelectedUnits;
            int selectionCount = selection.Count();
            if (SelectedType == null && selectionCount > 0) SelectedType = selection.First().Type;

            if (selectionCount == 1) CreateSingleUnitSelectionPanel();
            else CreateMultipleUnitsSelectionPanel();
        }

        private void CreateSingleUnitSelectionPanel()
        {
            UnitRenderer unitRenderer = (worldView.Renderer as MatchRenderer).WorldRenderer.UnitRenderer;
            Unit unit = userInputManager.SelectionManager.SelectedUnits.First();
            UnitButtonRenderer buttonRenderer = new UnitButtonRenderer(unitRenderer.GetTypeShape(unit.Type), unit);
            Button unitButton = new Button(new Rectangle(10, 10, 130, 175), "", buttonRenderer);
            float aspectRatio = Bounds.Width / Bounds.Height;
            unitButton.Bounds = new Rectangle(3f, 3f * aspectRatio);

            Label unitName = new Label(new Rectangle(150, 175, 200, 25), unit.Type.Name);
            unitName.Color = Color.White;

            Label health = new Label(new Rectangle(150, 150, 130, 25), "HP: {0}/{1}".FormatInvariant(unit.Health, unit.MaxHealth));
            health.Color = Color.White;

            unitButton.Pressed += ButtonPress;
            selectionFrame.Children.Add(unitButton);
            selectionFrame.Children.Add(health);
            selectionFrame.Children.Add(unitName);
        }

        private void CreateMultipleUnitsSelectionPanel()
        {
            const float padding = 10;
            Rectangle frame = new Rectangle(selectionFrame.Bounds.Width / 7 - padding * 2, selectionFrame.Bounds.Height / 2 - padding * 2);
            float currentX = padding + selectionFrame.Bounds.X;
            float currentY = selectionFrame.Bounds.Height - padding - frame.Height;
            UnitRenderer unitRenderer = (worldView.Renderer as MatchRenderer).WorldRenderer.UnitRenderer;
            foreach (Unit unit in userInputManager.SelectionManager.SelectedUnits)
            {
                UnitButtonRenderer renderer = new UnitButtonRenderer(unitRenderer.GetTypeShape(unit.Type), unit);
                renderer.HasFocus = unit.Type == selectedType;
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
        }

        private void ButtonPress(Button button)
        {
            if (button.Renderer is UnitButtonRenderer)
            {
                Unit unit = (button.Renderer as UnitButtonRenderer).Unit;
                IEnumerable<Unit> selectedUnits = userInputManager.SelectionManager.SelectedUnits;
                if (unit.Type == selectedType || selectedUnits.Count() == 1)
                {
                    userInputManager.SelectionManager.SelectUnit(unit);
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

        private void UpdateSkillsPanel()
        {

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
        internal override void OnEnter(RootView enterOn)
        { }

        internal override void OnShadow(RootView shadowedFrom)
        {
            shadowedFrom.PopDisplay(this);
        }
        #endregion
        #endregion
    }
}
