using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Orion.Geometry;

namespace Orion.Graphics
{
    public interface IRenderer
    {
        void RenderInto(GraphicsContext context);
    }
}
