using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.Geometry;
using Orion.Graphics;
using OpenTK.Math;

namespace Orion.UserInterface.Widgets
{
    public class ListFrame : ClippedView
    {
        private Rectangle itemFrame;
        private Vector2 padding;

        public ListFrame(Rectangle frame, Rectangle itemFrame, Vector2 padding)
            : base(frame, new Rectangle(0, frame.Height - padding.Y * 2, itemFrame.Width + padding.X * 2, padding.Y * 2), new FilledFrameRenderer())
        {
            this.padding = padding;
            this.itemFrame = itemFrame;
        }

        public Rectangle ItemFrame
        {
            get { return itemFrame; }
        }

        protected internal override void OnAddChild(ViewContainer child)
        {
            if (child != null)
            {
                float yDelta = itemFrame.Height + padding.Y;
                float yPos = Bounds.MaxY - padding.Y - yDelta * Children.Count;
                child.Frame = itemFrame.TranslatedBy(padding.X, yPos);
                FullBounds = FullBounds.TranslatedBy(0, -yDelta).ResizedBy(0, yDelta);
            }
            base.OnAddChild(child);
        }

        protected internal override void OnRemoveChild(ViewContainer child)
        {
            int index = Children.IndexOf(child);
            for (int i = index; i < Children.Count; i++)
            {
                View childAsView = (View)Children[i];
                childAsView.Frame = childAsView.Frame.TranslatedBy(0, -itemFrame.Height);
            }
            base.OnRemoveChild(child);
        }
    }
}
