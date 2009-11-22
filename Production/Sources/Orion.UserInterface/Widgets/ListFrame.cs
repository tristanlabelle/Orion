using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.Geometry;
using OpenTK.Math;

namespace Orion.UserInterface.Widgets
{
    public class ListFrame : Frame
    {
        private Rectangle itemFrame;
        private Vector2 padding;

        public ListFrame(Rectangle frame, Rectangle itemFrame, Vector2 padding)
            : base(frame)
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
                float yPos = Bounds.MaxY - (itemFrame.Height + padding.Y) * Children.Count - padding.Y;
                child.Frame = itemFrame.TranslatedBy(padding.X, yPos);
                if (yPos < 0) Bounds = Bounds.TranslatedBy(0, -yPos);
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
