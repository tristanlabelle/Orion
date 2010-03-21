using System;
using System.Diagnostics;
using Orion.Engine;
using Orion.Engine.Graphics;
using Orion.Engine.Geometry;
using Orion.Graphics;
using Font = System.Drawing.Font;

namespace Orion.UserInterface.Widgets
{
    /// <summary>
    /// A Label is a visible readonly text field.
    /// </summary>
    [DebuggerDisplay("{String} label")]
    public class Label : View
    {
        #region Constructors
        public Label(Rectangle frame)
            : base(frame)
        {
            Text = new Text(string.Empty);
        }

        public Label(Rectangle frame, string caption)
            : base(frame)
        {
            Text = new Text(caption);
        }

        public Label(string caption)
            : this(new Text(caption))
        { }

        public Label(Text text)
            : base(text.Frame)
        {
            Text = text;
        }
        #endregion

        #region Properties
        /// <summary>
        /// Accesses this object's text contents.
        /// </summary>
        public Text Text { get; set; }

        /// <summary>
        /// Accesses this object's text color.
        /// </summary>
        public ColorRgba Color { get; set; }

        private string String
        {
            get { return Text.Value; }
        }
        #endregion

        #region Methods
        protected internal override void Draw(GraphicsContext context)
        {
            context.Draw(Text, Bounds, Color);
        }
        #endregion
    }
}
