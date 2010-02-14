using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Font = System.Drawing.Font;
using OpenTK.Math;
using Orion.Graphics;
using Orion.Geometry;
using Orion.UserInterface.Widgets;

namespace Orion.UserInterface.Actions
{
    internal class TooltipFrame : View
    {
        #region Fields
        private readonly Vector2 origin;
        private readonly float width;
        private readonly Font tooltipFont;
        private Text[] description;
        #endregion

        #region Constructors
        public TooltipFrame(Vector2 origin, float width)
            : base(new Rectangle(origin.X, origin.Y, width, 0))
        {
            tooltipFont = new Font("Trebuchet MS", 12);
            this.origin = origin;
            this.width = width;
        }
        #endregion

        #region Methods
        public void SetDescription(string value)
        {
            Rectangle unionedRectangle = new Rectangle(width, 0);
            string[] parts = value.Split('\n');
            description = new Text[parts.Length];
            int i = 0;
            foreach (string part in parts)
            {
                description[i] = new Text(part, tooltipFont);
                float constrainedHeight = description[i].HeightForConstrainedWidth(width);
                unionedRectangle = unionedRectangle.ResizedBy(0, constrainedHeight);
                i++;
            }

            Bounds = unionedRectangle;
            Frame = Bounds.TranslatedTo(origin);
        }

        protected internal override void Draw(GraphicsContext context)
        {
            context.FillColor = Colors.Gray;
            context.StrokeColor = Colors.White;
            context.Fill(Bounds);
            context.Stroke(Bounds);

            context.FillColor = Colors.White;
            float top = Bounds.MaxY;
            foreach (Text part in description)
            {
                context.Font = part.Font;
                float height = part.HeightForConstrainedWidth(width);
                top -= height;
                Rectangle textBounds = new Rectangle(width, height);
                context.Draw(part, new Vector2(0, top), textBounds);
            }
        }
        #endregion
    }
}
