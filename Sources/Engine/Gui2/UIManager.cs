using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using Orion.Engine.Graphics;

namespace Orion.Engine.Gui2
{
    public sealed class UIManager : UIElement
    {
        #region Fields
        private readonly GraphicsContext graphicsContext;
        private Size size;
        private Font defaultFont = new Font("Trebuchet MS", 10);
        private ColorRgba defaultTextColor = Colors.Black;
        #endregion

        #region Constructors
        public UIManager(GraphicsContext graphicsContext)
        {
        	Argument.EnsureNotNull(graphicsContext, "graphicsContext");
        	
        	this.graphicsContext = graphicsContext;
        	this.size = graphicsContext.ViewportSize;
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets the size of the client area where this <see cref="UIManager"/> can draw.
        /// </summary>
        public Size Size
        {
        	get { return size; }
        	set
        	{
        		size = value;
        		SetPreferredSizeDirty();
        	}
        }
        
        public Font DefaultFont
        {
        	get { return defaultFont; }
        	set
        	{
        		Argument.EnsureNotNull(value, "DefaultFont");
        		defaultFont = value;
        	}
        }
        
        public ColorRgba DefaultTextColor
        {
        	get { return defaultTextColor; }
        	set { defaultTextColor = value; }
        }
        #endregion

        #region Methods
        public Size MeasureString(IEnumerable<char> text)
        {
        	Argument.EnsureNotNull(text, "text");
        	throw new NotImplementedException();
        }
        
        /// <summary>
        /// Draws the UI hierarchy beneath this <see cref="UIManager"/>.
        /// </summary>
        public void Draw()
        {
        	Draw(graphicsContext);
        }
        
		protected override Size MeasurePreferredSize()
		{
			return size;
		}
        #endregion
    }
}
