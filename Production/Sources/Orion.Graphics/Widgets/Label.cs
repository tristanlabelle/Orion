using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OpenTK;

using Orion.Geometry;

namespace Orion.Graphics.Widgets
{
    /// <summary>
    /// A Label is a visible immutable text field.
    /// </summary>
    public class Label : View
    {
        /// <summary>
        /// Accesses this object's text contents.
        /// </summary>
        public virtual string Value { get; set; }

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
            Value = caption;
        }

        protected override void Draw()
        {
            context.DrawText(Value);
        }
    }
}
