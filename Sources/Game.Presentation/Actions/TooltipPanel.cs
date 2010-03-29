using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK.Math;
using Orion.Engine;
using Orion.Engine.Geometry;
using Orion.Engine.Graphics;
using Orion.Engine.Gui;
using Orion.Game.Presentation;
using Font = System.Drawing.Font;

namespace Orion.Game.Presentation.Actions
{
    internal class TooltipPanel : View
    {
        #region Fields
        private readonly Vector2 origin;
        private readonly float width;
        private readonly Font tooltipFont;
        private Text[] description;
        #endregion

        #region Constructors
        public TooltipPanel(Vector2 origin, float width)
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

        protected override void Draw(GraphicsContext context)
        {
            context.Fill(Bounds, Colors.Gray);
            context.Stroke(Bounds, Colors.White);

            float top = Bounds.MaxY;
            foreach (Text part in description)
            {
                context.Font = part.Font;
                float height = part.HeightForConstrainedWidth(width);
                top -= height;
                Rectangle textBounds = new Rectangle(width, height);
                context.Draw(part, new Vector2(0, top), textBounds, Colors.Black);
            }
        }
        #endregion
    }
}
