using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.Engine.Geometry;
using OpenTK;

namespace Orion.Engine.Gui
{
    public class ListPanel : ClippedView
    {
        #region Fields
        private readonly Action<View, Rectangle> frameChangedHandler;
        private readonly Vector2 padding;
        #endregion

        #region Constructors
        public ListPanel(Rectangle frame)
            : this(frame, new Vector2(0, 0))
        { }

        public ListPanel(Rectangle frame, Vector2 padding)
            : base(frame, new Rectangle(0, frame.Height - padding.Y, frame.Width, padding.Y), new FilledRenderer())
        {
            this.padding = padding;
            frameChangedHandler = OnChildFrameChanged;
        }
        #endregion

        #region Methods
        private void OnChildFrameChanged(View container, Rectangle newBounds)
        {
            int childIndex = Children.IndexOf(container);
            float translateBy = (newBounds.MaxY + padding.Y) -
                ((childIndex == 0) ? FullBounds.MaxY : Children[childIndex - 1].Frame.MaxY);
            
            foreach (ViewContainer child in Children.Skip(childIndex))
                child.Frame = child.Frame.TranslatedBy(0, translateBy);
        }

        protected internal override void OnChildAdded(ViewContainer child)
        {
            if (child != null)
            {
                View view = child as View;
                if (view != null) view.BoundsChanged += frameChangedHandler;

                float yTransform = child.Frame.Height + padding.Y;
                FullBounds = FullBounds.TranslatedBy(0, -yTransform).ResizedBy(0, yTransform);
                child.Frame = new Rectangle(padding.X, FullBounds.MinY + padding.Y, child.Frame.Width, child.Frame.Height);
            }
            base.OnChildAdded(child);
        }

        protected internal override void OnChildRemoved(ViewContainer child)
        {
            if (child != null)
            {
                View view = child as View;
                if (view != null) view.BoundsChanged -= frameChangedHandler;

                float yTranslation = child.Frame.Height + padding.Y;
                for (int index = Children.IndexOf(child) + 1; index < Children.Count; index++)
                {
                    ViewContainer container = Children[index];
                    container.Frame = container.Frame.TranslatedBy(0, yTranslation);
                }
                FullBounds = FullBounds.TranslatedBy(0, yTranslation).ResizedBy(0, -yTranslation);
            }
            base.OnChildRemoved(child);
        }
        #endregion
    }
}
