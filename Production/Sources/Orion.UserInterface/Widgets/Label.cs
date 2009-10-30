using Orion.Geometry;
using Orion.Graphics;
using Color = System.Drawing.Color;

namespace Orion.UserInterface.Widgets
{
    /// <summary>
    /// A Label is a visible immutable text field.
    /// </summary>
    public class Label : View
    {
        /// <summary>
        /// Accesses this object's text contents.
        /// </summary>
        public virtual string Text { get; set; }

        /// <summary>
        /// Accesses this object's text color.
        /// </summary>
        public Color Color { get; set; }

        /// <summary>
        /// Constructs a Label with a given frame.
        /// </summary>
        /// <param name="frame">The frame rectangle</param>
        public Label(Rectangle frame)
            : base(frame)
        { }

        /// <summary>
        /// Constructs a Label with a given frame and text content.
        /// </summary>
        /// <param name="frame"></param>
        /// <param name="caption"></param>
        public Label(Rectangle frame, string caption)
            : base(frame)
        {
            Text = caption;
        }

        protected internal override void Draw(GraphicsContext context)
        {
            context.FillColor = Color;
            context.DrawText(Text, Bounds);
        }
    }
}
