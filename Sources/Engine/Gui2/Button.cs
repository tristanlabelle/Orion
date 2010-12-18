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
        }

        public Button(string text)
            : this()
        {
            Argument.EnsureNotNull(text, "text");

            Content = new Label(text)
            {
                HorizontalAlignment = Alignment.Center,
                VerticalAlignment = Alignment.Center,
                Margin = new Borders(2)
            };
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
            return (content == null ? Size.Zero : content.Measure());
        }

        protected override void DoDraw(GraphicsContext graphicsContext)
        {
            bool hovered = IsAncestorOf(Manager.HoveredElement);
            graphicsContext.Fill((Rectangle)(GetReservedRectangle() - Margin).Value, hovered ? Colors.LightGray : Colors.Gray);
            DrawChildren(graphicsContext);
        }
        #endregion
    }
}
