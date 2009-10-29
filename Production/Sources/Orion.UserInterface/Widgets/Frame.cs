using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Color = System.Drawing.Color;

using Orion.Geometry;
using Orion.Graphics;

namespace Orion.UserInterface.Widgets
{
    public class Frame : RenderedView
    {
        public Frame(Rectangle frame)
            : base(frame, new FilledFrameRenderer())
        { }
        public Frame(Rectangle frame, Color fillColor)
            : base(frame, new FilledFrameRenderer(fillColor))
        { }
        public Frame(Rectangle frame, FrameRenderer renderer)
            : base(frame, renderer)
        { }
    }
}
