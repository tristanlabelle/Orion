using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Orion.Engine.Gui2
{
    /// <summary>
    /// A <see cref="Control"/> which displays an image.
    /// </summary>
    public sealed class ImageBox : Control
    {
        #region Fields
        private object source;
        #endregion

        #region Constructors
        public ImageBox() { }

        public ImageBox(object source)
        {
            this.source = source;
        }
        #endregion

        #region Properties
        /// <summary>
        /// A <see cref="IGuiRenderer"/>-specific object providing the source of the image.
        /// </summary>
        public object Source
        {
            get { return source; }
            set
            {
                if (value == source) return;
                source = value;
            }
        }
        #endregion

        #region Methods
        protected override Size MeasureWithoutMargin()
        {
            return Manager.Renderer.GetImageSize(this, source);
        }
        #endregion
    }
}
