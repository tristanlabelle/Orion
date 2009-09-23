using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Audio;
using OpenTK.Math;
using OpenTK.Input;
using OpenTK.Platform;

namespace Orion.Graphics.Graphics
{
    struct Rect
    {
        public readonly Vector2 Position;
        public readonly Vector2 Size;

        public Rect(Vector2 position, Vector2 size)
        {
            Position = position;
            Size = size;
        }
    }
}
