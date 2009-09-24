using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

using OpenTK;
using OpenTK.Graphics;

namespace Orion.Graphics.Drawing
{
    interface IDrawable
    {
		internal void Fill();
		internal void Stroke();
    }
}
