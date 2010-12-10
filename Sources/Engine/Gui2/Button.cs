using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.Engine.Graphics;
using Orion.Engine.Geometry;

namespace Orion.Engine.Gui2
{
    public sealed class Button : UIElement
    {
        #region Fields
        private readonly SingleChildCollection children;
        private UIElement content;
        #endregion

        #region Constructors
        public Button()
        {
            children = new SingleChildCollection(() => content, value => content = value);
            Padding = new Borders(2);
        }

        public Button(string text)
            : this()
        {
            Argument.EnsureNotNull(text, "text");

            Content = new Label { Text = text };
        }
        #endregion

        #region Properties
        public UIElement Content
        {
            get { return content; }
            set
            {
                if (value == content) return;

                if (content != null)
                {
                    AbandonChild(content);
                    content = null;
                }

                if (value != null)
                {
                    AdoptChild(value);
                    content = value;
                }
            }
        }
        #endregion

        #region Methods
        protected override ICollection<UIElement> GetChildren()
        {
            return children;
        }

        protected override Size MeasureWithoutMargin()
        {
            return (content == null ? Size.Zero : content.Measure()) + Padding;
        }

        protected override void DoDraw(GraphicsContext graphicsContext)
        {
            graphicsContext.Fill((Rectangle)(Arrange() - Margin).Value, Colors.Gray);
            DrawChildren(graphicsContext);
        }
        #endregion
    }
}
