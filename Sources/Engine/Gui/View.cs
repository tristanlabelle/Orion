﻿using System;
using OpenTK.Math;
using Orion.Engine;
using Orion.Engine.Graphics;
using Orion.Engine.Geometry;
using Orion.Engine.Gui;

namespace Orion.Engine.Gui
{
    public abstract class View : Responder
    {
        #region Fields
        private Rectangle bounds;
        #endregion

        #region Constructors
        public View(Rectangle rectangle)
        {
            base.Frame = rectangle;
            this.bounds = new Rectangle(rectangle.Size);
        }
        #endregion

        #region Events
        public event Action<View, Rectangle> BoundsChanged;
        public event Action<View, Rectangle> FrameChanged;
        #endregion

        #region Properties
        public new ViewChildrenCollection Children
        {
            get { return (ViewChildrenCollection)base.Children; }
        }

        public override Rectangle Bounds
        {
            get { return bounds; }
            set
            {
                Rectangle previousBounds = bounds;
                bounds = value;
                if (IsMouseOver)
                {
                    Vector2 position = MousePosition.Value;
                    PropagateMouseEvent(MouseEventType.MouseMoved, new MouseEventArgs(position, MouseButton.None, 0, 0));
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

            return base.PropagateMouseEvent(eventType, args.CloneWithNewPosition(coords));
        }

        protected internal override void Render(GraphicsContext graphicsContext)
        {
            Vector2 scaling = new Vector2(Frame.Width / bounds.Width, Frame.Height / bounds.Height);
            Vector2 translation = new Vector2(Frame.MinX - bounds.MinX * scaling.X, Frame.MinY - bounds.MinY * scaling.Y);

            using (graphicsContext.PushTransform(translation, 0, scaling))
            {
                Draw(graphicsContext);
                base.Render(graphicsContext);
            }
        }

        protected internal abstract void Draw(GraphicsContext graphicsContext);
        #endregion
    }
}