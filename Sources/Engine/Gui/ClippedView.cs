using System;
using OpenTK;
using Orion.Engine;
using Orion.Engine.Graphics;
using Orion.Engine.Geometry;

namespace Orion.Engine.Gui
{
    public class ClippedView : RenderedView
    {
        #region Fields
        private Rectangle fullBounds;
        #endregion

        #region Constructors
        public ClippedView(Rectangle frame, Rectangle fullBounds, IViewRenderer renderer)
            : base(frame, renderer)
        {
            this.fullBounds = fullBounds;
            MinimumVisibleBoundsSize = FullBounds.Size * 0.01f;
            MaximumVisibleBounds = FullBounds;
        }
        #endregion

        #region Events
        public event Action<ClippedView, Rectangle> FullBoundsChanged;
        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets the full, unclipped size of the view, of which <see cref="Bounds"/> only represents a portion.
        /// </summary>
        public Rectangle FullBounds
        {
            get { return fullBounds; }
            set
            {
                fullBounds = value;
                if (FullBoundsChanged != null) FullBoundsChanged(this, value);
            }
        }

        /// <summary>
        /// Gets the minimum size of <see cref="Bounds"/>.
        /// </summary>
        public Vector2 MinimumVisibleBoundsSize { get; set; }

        /// <summary>
        /// Gets or sets the maximum area for <see cref="Bounds"/>.
        /// </summary>
        public Rectangle MaximumVisibleBounds { get; set; }
        #endregion

        #region Methods
        public void Zoom(float factor)
        {
            Vector2 scale = new Vector2(factor, factor);
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

            if (newSize.X < MinimumVisibleBoundsSize.X)
            {
                float ratio = Bounds.Size.Y / Bounds.Size.X;
                newSize.X = MinimumVisibleBoundsSize.X;
                newSize.Y = newSize.X * ratio;
            }
            if (newSize.Y < MinimumVisibleBoundsSize.Y)
            {
                float ratio = Bounds.Size.X / Bounds.Size.Y;
                newSize.Y = MinimumVisibleBoundsSize.Y;
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

        public void ScrollBy(float x, float y)
        {
            ScrollBy(new Vector2(x, y));
        }

        public void ScrollBy(Vector2 direction)
        {
            SetTranslatedBounds(Bounds.TranslatedBy(direction));
        }

        public void ScrollTo(float x, float y)
        {
            ScrollTo(new Vector2(x, y));
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
            using (graphicsContext.PushScissorRegion(region)) base.Render(graphicsContext);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) FullBoundsChanged = null;

            base.Dispose(disposing);
        }
        #endregion
    }
}
