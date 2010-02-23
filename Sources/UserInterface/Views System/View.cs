﻿using System;
using OpenTK.Math;
using Orion.Engine.Graphics;
using Orion.Geometry;
using Orion.Graphics;

namespace Orion.UserInterface
{
    public abstract class View : Responder
    {
        #region Fields
        private GraphicsContext context;
        #endregion

        #region Constructors
        public View(Rectangle rectangle)
        {
            context = new GraphicsContext(new Rectangle(rectangle.Size));
            base.Frame = rectangle;
        }
        #endregion

        #region Events
        public event Action<View, Rectangle> BoundsChanged;
        public event Action<View, Rectangle> FrameChanged;
        #endregion

        #region Properties

        public new ViewChildrenCollection Children
        {
            get { return base.Children as ViewChildrenCollection; }
        }

        public override Rectangle Bounds
        {
            get { return context.CoordinateSystem; }
            set
            {
                Rectangle previousBounds = context.CoordinateSystem;
                context.CoordinateSystem = value;
                if (IsMouseOver)
                {
                    Vector2 position = MousePosition.Value;
                    PropagateMouseEvent(MouseEventType.MouseMoved, new MouseEventArgs(position.X, position.Y, MouseButton.None, 0, 0));
                }
                Action<View, Rectangle> boundsEvent = BoundsChanged;
                if (boundsEvent != null) boundsEvent(this, previousBounds);
            }
        }

        public override Rectangle Frame
        {
            get { return base.Frame; }
            set
            {
                Rectangle previousFrame = base.Frame;
                base.Frame = value;
                Action<View, Rectangle> frameEvent = FrameChanged;
                if (frameEvent != null) frameEvent(this, previousFrame);
            }
        }
        #endregion

        #region Methods
        protected internal override bool PropagateMouseEvent(MouseEventType eventType, MouseEventArgs args)
        {
            Vector2 coords = args.Position;
            coords -= Frame.Min;
            coords.Scale(Bounds.Width / Frame.Width, Bounds.Height / Frame.Height);
            coords += Bounds.Min;

            return base.PropagateMouseEvent(eventType, new MouseEventArgs(coords.X, coords.Y, args.ButtonPressed, args.Clicks, args.WheelDelta));
        }

        protected internal override void Render()
        {
            context.SetUpGLContext(Frame);

            Draw(context);
            base.Render();
            context.RestoreGLContext();
        }

        protected internal abstract void Draw(GraphicsContext context);
        #endregion
    }
}
