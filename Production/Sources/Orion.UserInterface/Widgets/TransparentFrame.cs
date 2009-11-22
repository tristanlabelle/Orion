﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.Geometry;
using Orion.Graphics;

namespace Orion.UserInterface.Widgets
{
    public class TransparentFrame : View
    {
        public TransparentFrame(Rectangle rect)
            : base(rect)
        { }

        protected internal override void Draw(GraphicsContext context)
        { }
    }
}
