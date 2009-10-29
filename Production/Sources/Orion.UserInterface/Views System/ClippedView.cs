﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Orion.Geometry;
using Orion.Graphics;

using OpenTK.Math;

namespace Orion.UserInterface
{
    public class ClippedView : RenderedView
    {
        public readonly Rectangle FullBounds;

        public ClippedView(Rectangle frame, Rectangle fullBounds, IRenderer renderer)
            : base(frame, renderer)
        {
            FullBounds = fullBounds;
        }

        public void Zoom(double factor)
        {
            Zoom(factor, Bounds.Center);
        }

        public void Zoom(double factor, Vector2 center)
        {
            Vector2 scale = new Vector2((float)factor, (float)factor);
            Vector2 newSize = Bounds.Size;
            Vector2 newOrigin = Bounds.Origin;
            newSize.Scale(scale);

            if (newSize.X > FullBounds.Size.X)
            {
                float ratio = Bounds.Size.Y / Bounds.Size.X;
                newSize.X = FullBounds.Size.X;
                newSize.Y = newSize.X * ratio;
            }
            if (newSize.Y > FullBounds.Size.Y)
            {
                float ratio = Bounds.Size.X / Bounds.Size.Y;
                newSize.Y = newSize.X * ratio;
                newSize.X = newSize.Y * ratio;
            }

            Rectangle newBounds = new Rectangle(newOrigin, newSize);

            Vector2 originDifference = Bounds.Center - newBounds.Center;
            originDifference.Scale(0.5f, 0.5f);
            newOrigin += originDifference;

            if (newOrigin.X < 0) newOrigin.X = 0;
            if (newOrigin.Y < 0) newOrigin.Y = 0;

            newBounds = new Rectangle(newOrigin, newSize);

            if (newBounds.MaxX > FullBounds.MaxX) newOrigin.X -= newBounds.MaxX - FullBounds.MaxX;
            if (newBounds.MaxY > FullBounds.MaxY) newOrigin.Y -= newBounds.MaxY - FullBounds.MaxY;

            Bounds = new Rectangle(newOrigin, newSize);
        }

        public void ScrollBy(double x, double y)
        {
            ScrollBy(new Vector2((float)x, (float)y));
        }

        public void ScrollBy(Vector2 direction)
        {
            Rectangle newBounds = Bounds.Translate(direction);
            Vector2 newOrigin = newBounds.Origin;
            Vector2 newSize = newBounds.Size;

            if (newOrigin.X < FullBounds.X)
                newOrigin.X = FullBounds.X;

            if (newOrigin.Y < FullBounds.Y)
                newOrigin.Y = FullBounds.Y;

            if (newBounds.MaxX > FullBounds.MaxX)
                newOrigin.X -= newBounds.MaxX - FullBounds.MaxX;

            if (newBounds.MaxY > FullBounds.MaxY)
                newOrigin.Y -= newBounds.MaxY - FullBounds.MaxY;

            Bounds = new Rectangle(newOrigin, newSize);
        }
    }
}