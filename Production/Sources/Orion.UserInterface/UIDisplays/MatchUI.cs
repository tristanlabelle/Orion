using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Keys = System.Windows.Forms.Keys;

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
        private readonly WorldRenderer renderer;
        private UserInputCommander userInputCommander;
        private ClippedView worldView;

        #region Event Handling Delegates
        private GenericEventHandler<Responder, MouseEventArgs> worldViewMouseDown, worldViewMouseMove, worldViewMouseUp, worldViewZoom;
        private GenericEventHandler<Responder, KeyboardEventArgs> worldViewKeyDown, worldViewKeyUp;
        #endregion
        #endregion

        #region Constructors

        public MatchUI(World world, UserInputCommander commander)
        {
            Argument.EnsureNotNull(world, "world");
            Argument.EnsureNotNull(commander, "commander");

            userInputCommander = commander;
            renderer = new WorldRenderer(world);

            world.Units.UnitDied += userInputCommander.SelectionManager.UnitDied;
            worldView = new ClippedView(Bounds, world.Bounds, new MatchRenderer(world, commander.SelectionManager));
            worldView.Bounds = new Rectangle(40, 30);
            Children.Add(worldView);

            Rectangle northFrame = new Rectangle(0, Bounds.Height, Bounds.Width, -20);
            Rectangle southFrame = new Rectangle(0, 0, Bounds.Width, 20);
            Rectangle eastFrame = new Rectangle(Bounds.Width, 0, -20, Bounds.Height);
            Rectangle westFrame = new Rectangle(0, 0, 20, Bounds.Height);
            Scroller northScroller = new Scroller(worldView, northFrame, new Vector2(0, 1), MouseEventType.MouseEntered, Keys.Up);
            Scroller southScroller = new Scroller(worldView, southFrame, new Vector2(0, -1), MouseEventType.MouseEntered, Keys.Down);
            Scroller eastScroller = new Scroller(worldView, eastFrame, new Vector2(1, 0), MouseEventType.MouseEntered, Keys.Right);
            Scroller westScroller = new Scroller(worldView, westFrame, new Vector2(-1, 0), MouseEventType.MouseEntered, Keys.Left);
            
            Children.Add(northScroller);
            Children.Add(southScroller);
            Children.Add(eastScroller);
            Children.Add(westScroller);
            Rectangle resourceDisplayFrame = new Rectangle(0, Bounds.Height, Bounds.Width, -Bounds.Height / 25);
            ResourceDisplay resourceDisplay = new ResourceDisplay(resourceDisplayFrame, userInputCommander.Faction);
            Children.Add(resourceDisplay);

            worldViewMouseDown = WorldViewMouseDown;
            worldViewMouseUp = WorldViewMouseUp;
            worldViewMouseMove = WorldViewMouseMove;
            worldViewKeyDown = WorldViewKeyDown;
            worldViewKeyUp = WorldViewKeyUp;
            worldViewZoom = WorldViewZoom;
        }

        #endregion

        #region Methods
        #region Event Handling
        private void WorldViewZoom(Responder source, MouseEventArgs args)
        {
            double scale = 1 - (args.WheelDelta / 600.0);
            worldView.Zoom(scale, args.Position);
            base.OnMouseWheel(args);
        }

        private void WorldViewMouseDown(Responder source, MouseEventArgs args)
        {
            userInputCommander.OnMouseButton(new Vector2(args.X, args.Y), args.ButtonPressed, true);
        }

        private void WorldViewMouseUp(Responder source, MouseEventArgs args)
        {
            userInputCommander.OnMouseButton(new Vector2(args.X, args.Y), args.ButtonPressed, false);
        }

        private void WorldViewMouseMove(Responder source, MouseEventArgs args)
        {
            userInputCommander.OnMouseMove(new Vector2(args.X, args.Y));
        }

        private void WorldViewKeyDown(Responder source, KeyboardEventArgs args)
        {
            userInputCommander.OnKeyDown(args.Key);
        }

        private void WorldViewKeyUp(Responder source, KeyboardEventArgs args)
        {
            userInputCommander.OnKeyUp(args.Key);
        }
        #endregion

        #region IUIDisplay Implementation
        internal override void OnEnter(RootView into)
        {
            worldView.MouseWheel += worldViewZoom;
            worldView.MouseDown += worldViewMouseDown;
            worldView.MouseMoved += worldViewMouseMove;
            worldView.MouseUp += worldViewMouseUp;
            worldView.KeyDown += worldViewKeyDown;
            worldView.KeyUp += worldViewKeyUp;
        }

        internal override void OnShadow(RootView shadowedFrom)
        {
            worldView.MouseDown -= worldViewMouseDown;
            worldView.MouseMoved -= worldViewMouseMove;
            worldView.MouseUp -= worldViewMouseUp;
            worldView.KeyDown -= worldViewKeyDown;
            worldView.KeyUp -= worldViewKeyUp;
            shadowedFrom.PopDisplay(this);
        }
        #endregion

        #endregion
    }
}
