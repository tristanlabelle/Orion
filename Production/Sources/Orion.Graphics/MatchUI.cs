using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Keys = System.Windows.Forms.Keys;

using Orion.Commandment;
using Orion.GameLogic;
using Orion.Geometry;

using OpenTK.Math;

namespace Orion.Graphics
{
    public class MatchUI : UIDisplay
    {
        #region Fields
        private readonly WorldRenderer renderer;
        private UserInputCommander userInputCommander;
        private WorldView worldView;

        #region Event Handling Delegates
        private GenericEventHandler<Responder, MouseEventArgs> worldViewMouseDown, worldViewMouseMove, worldViewMouseUp;
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
            worldView = new WorldView(Bounds, renderer, userInputCommander.SelectionManager);
            worldView.Bounds = new Rectangle(40, 30);
            Children.Add(worldView);

            Rectangle northFrame = new Rectangle(0, Bounds.Height, Bounds.Width, -20);
            Rectangle southFrame = new Rectangle(0, 0, Bounds.Width, 20);
            Rectangle eastFrame = new Rectangle(Bounds.Width, 0, -20, Bounds.Height);
            Rectangle westFrame = new Rectangle(0, 0, 20, Bounds.Height);
            Scroller northScroller = new Scroller(northFrame, worldView, new Vector2(0, 1), Keys.Up);
            Scroller southScroller = new Scroller(southFrame, worldView, new Vector2(0, -1), Keys.Down);
            Scroller eastScroller = new Scroller(eastFrame, worldView, new Vector2(1, 0), Keys.Right);
            Scroller westScroller = new Scroller(westFrame, worldView, new Vector2(-1, 0), Keys.Left);
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
        }

        #endregion

        #region Methods
        #region Event Handling
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
