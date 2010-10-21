using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Orion.Engine.Gui2
{
    public abstract class UIElement
    {
        #region Fields
        private UIManager manager;
        private UIElement parent;
        private Borders margin;
        private UIElementVisibility visibility;
        private Size? size;
        private bool hasDirtyChild = false;
        #endregion

        #region Constructors
        #endregion

        #region Properties
        public UIManager Manager
        {
            get { return manager; }
        }

        public UIElement Parent
        {
            get { return parent; }
        }

        public virtual Borders Margin
        {
            get { return margin; }
            set
            {
                this.margin = value;
                MarkSizeAsDirty();
            }
        }

        public UIElementVisibility Visibility
        {
            get { return visibility; }
            set
            {
                visibility = value;
                MarkSizeAsDirty();
            }
        }

        public ICollection<UIElement> Children
        {
            get { return GetChildren(); }
        }
        #endregion

        #region Methods
        protected abstract ICollection<UIElement> GetChildren();

        protected virtual void SetParent(UIElement parent)
        {
            this.parent = parent;
            this.manager = parent == null ? null : parent.manager;
        }

        protected abstract Size MeasureWithoutMargin();

        protected Size Measure()
        {
            Size sizeWithoutMargin = MeasureWithoutMargin();
            return new Size(
                sizeWithoutMargin.Width + Margin.MinX + margin.MaxX,
                sizeWithoutMargin.Height + Margin.MinY + margin.MaxY);
        }

        protected void MarkSizeAsDirty();

        protected abstract void Draw();
        #endregion
    }
}
