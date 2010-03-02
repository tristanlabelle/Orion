﻿using System;
using OpenTK.Math;
using Orion.Engine.Graphics;
using Orion.Geometry;
using Orion.Graphics;

namespace Orion.UserInterface
{
    public class ClippedView : RenderedView
    {
        #region Fields
        private Rectangle fullBounds;
        #endregion

        #region Constructors
        public ClippedView(Rectangle frame, Rectangle fullBounds, IRenderer renderer)
            : base(frame, renderer)
        {
            this.fullBounds = fullBounds;
            MinimumVisibleBounds = FullBounds.ScaledBy(0.01f);
            MaximumVisibleBounds = FullBounds;
        }
        #endregion

        #region Events
        public event Action<ClippedView, Rectangle> FullBoundsChanged;
        #endregion

        #region Properties
        public Rectangle FullBounds
        {
            get { return fullBounds; }
            set
            {
                fullBounds = value;
                if (FullBoundsChanged != null) FullBoundsChanged(this, value);
            }
        }

        public Rectangle MinimumVisibleBounds { get; set; }

        public Rectangle MaximumVisibleBounds { get; set; }
        #endregion

        #region Methods
        public void Zoom(double factor)
        {
            Zoom(factor, Bounds.Center);
        }

        public void Zoom(double factor, Vector2 center)
        {
            Vector2 scale = new Vector2((float)factor, (float)factor);
            Vector2 newSize = Bounds.Size;
            Vector2 newOrigin = Bounds.Min;
            newSize.Scale(scale);

            if (newSize.X > MaximumVisibleBounds.Width)
            {
                float ratio = Bounds.Size.Y / Bounds.Size.X;
                newSize.X = MaximumVisibleBounds.Width;
                newSize.Y = newSize.X * ratio;
            }
            if (newSize.Y > MaximumVisibleBounds.Height)
            {
                float ratio = Bounds.Size.X / Bounds.Size.Y;
                newSize.Y = MaximumVisibleBounds.Height;
                newSize.X = newSize.Y * ratio;
            }

            if (newSize.X < MinimumVisibleBounds.Width)
            {
                float ratio = Bounds.Size.Y / Bounds.Size.X;
                newSize.X = MinimumVisibleBounds.Width;
                newSize.Y = newSize.X * ratio;
            }
            if (newSize.Y < MinimumVisibleBounds.Height)
            {
                float ratio = Bounds.Size.X / Bounds.Size.Y;
                newSize.Y = MinimumVisibleBounds.Height;
                newSize.X = newSize.Y * ratio;
            }

            Rectangle newBounds = new Rectangle(newOrigin, newSize);

            Vector2 originDifference = Bounds.Center - newBounds.Center;
            originDifference.Scale(0.5f, 0.5f);
            newOrigin += originDifference;

            if (newOrigin.X < FullBounds.MinX) newOrigin.X = FullBounds.MinX;
            if (newOrigin.Y < FullBounds.MinY) newOrigin.Y = FullBounds.MinY;

            newBounds = new Rectangle(newOrigin, newSize);

            if (newBounds.MaxX > FullBounds.MaxX) newOrigin.X -= newBounds.MaxX - FullBounds.MaxX;
            if (newBounds.MaxY > FullBounds.MaxY) newOrigin.Y -= newBounds.MaxY - FullBounds.MaxY;

            Bounds = new Rectangle(newOrigin, newSize);
        }

        public void ScrollBy(double x, double y)
        {
            ScrollBy(new Vector2((float)x, (float)y));
        }

        public void ScrollBy(Vector2 direction)
        {
            SetTranslatedBounds(Bounds.TranslatedBy(direction));
        }

        public void ScrollTo(double x, double y)
        {
            ScrollTo(new Vector2((float)x, (float)y));
        }

        public void ScrollTo(Vector2 position)
        {
            SetTranslatedBounds(Bounds.TranslatedTo(position));
        }

        private void SetTranslatedBounds(Rectangle newBounds)
        {
            Vector2 min = newBounds.Min;
            Vector2 max = newBounds.Max;

            if (min.X < FullBounds.MinX)
                min.X = FullBounds.MinX;

            if (min.Y < FullBounds.MinY)
                min.Y = FullBounds.MinY;

            if (max.X > FullBounds.MaxX)
                min.X -= max.X - FullBounds.MaxX;

            if (max.Y > FullBounds.MaxY)
                min.Y -= max.Y - FullBounds.MaxY;

            Bounds = newBounds.TranslatedTo(min);
        }

        protected internal override void Render(GraphicsContext graphicsContext)
        {
            Vector2 origin = Frame.Min;
            Vector2 end = Frame.Max;
            ViewContainer parent = Parent;
            while (parent != null)
            {
                origin = Rectangle.ConvertPoint(parent.Bounds, parent.Frame, origin);
                end = Rectangle.ConvertPoint(parent.Bounds, parent.Frame, end);
                parent = parent.Parent;
            }
            Vector2 size = end - origin;

            Region region = new Region((int)origin.X, (int)origin.Y, (int)size.X, (int)size.Y);
            using (graphicsContext.Scissor(region)) base.Render(graphicsContext);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) FullBoundsChanged = null;

            base.Dispose(disposing);
        }
        #endregion
    }
}