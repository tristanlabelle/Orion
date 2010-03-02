﻿using System;
using Orion.Engine.Graphics;
using Orion.Geometry;
using Orion.Graphics;
using Font = System.Drawing.Font;

namespace Orion.UserInterface.Widgets
{
    /// <summary>
    /// A Label is a visible readonly text field.
    /// </summary>
    public class Label : View
    {
        #region Constructors
        public Label(Rectangle frame)
            : base(frame)
        {
            Text = new Text("");
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
        #endregion

        #region Methods
        protected internal override void Draw(GraphicsContext context)
        {
            context.FillColor = Color;
            context.Draw(Text, Bounds);
        }
        #endregion
    }
}