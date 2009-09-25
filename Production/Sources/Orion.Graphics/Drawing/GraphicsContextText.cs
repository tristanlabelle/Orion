using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Drawing;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Audio;
using OpenTK.Math;
using OpenTK.Input;
using OpenTK.Platform;

namespace Orion.Graphics.Drawing
{
    partial class GraphicsContext
    {
        TextPrinter printer = new TextPrinter();
        Font sans_serif = new Font(FontFamily.GenericSansSerif, 18.0f);

        public void DrawTextInView(string text, View view)
        {
            printer.Begin();
            GL.Translate(view.Frame.Position.X, view.Frame.Position.Y, 0);
            printer.Print(text, sans_serif, Color.Black);
            printer.End();
        }
    }
}