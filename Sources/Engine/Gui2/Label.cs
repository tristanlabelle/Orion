using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using OpenTK;
using Orion.Engine.Graphics;

namespace Orion.Engine.Gui2
{
    public sealed class Label : UIElement
    {
        #region Fields
        private string text = string.Empty;
        private ColorRgba? customColor;
        private Font customFont;
        #endregion

        #region Constructors
        #endregion

        #region Properties
        public string Text
        {
            get { return text; }
            set
            {
                Argument.EnsureNotNull(value, "Text");
                this.text = value;
            }
        }

        public ColorRgba? CustomColor
        {
            get { return customColor; }
            set { customColor = value; }
        }
        
        public ColorRgba Color
        {
        	get
        	{
        		if (customColor.HasValue) return customColor.Value;
        		return Manager == null ? Colors.Black : Manager.DefaultTextColor;
        	}
        }
        
        public Font CustomFont
        {
        	get { return customFont; }
        	set { customFont = value; }
        }
        
        public Font Font
        {
        	get
        	{
        		if (customFont != null) return customFont;
        		return Manager == null ? null : Manager.DefaultFont;
        	}
        }
        #endregion

        #region Methods
        protected override Size MeasurePreferredSizeWithoutMargin()
        {
        	return Manager.MeasureString(text);
        }
        
		protected override void DoDraw(GraphicsContext graphicsContext)
		{
			Region region = GetRegion();
			
			graphicsContext.Draw(text, (Vector2)region.Min, Color);
		}
        #endregion
    }
}
