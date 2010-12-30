using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.Engine;
using Orion.Engine.Geometry;
using OpenTK;

namespace Orion.Game.Presentation
{
    /// <summary>
    /// Manages the portion of the world that is visible.
    /// </summary>
    public sealed class Camera
    {
        #region Fields
        private const float scrollSpeed = 40;

        private Size worldSize;
        private Size viewportSize;
        private Vector2 target;
        private Point scrollDirection;
        #endregion

        #region Constructors
        public Camera(Size worldSize, Size viewportSize)
        {
            this.worldSize = worldSize;
            this.viewportSize = viewportSize;
        }
        #endregion

        #region Properties
        /// <summary>
        /// The size of the client area in which the game can be seen.
        /// </summary>
        public Size ViewportSize
        {
            get { return viewportSize; }
            set { viewportSize = value; }
        }

        /// <summary>
        /// The world-space point at the center of the view.
        /// </summary>
        public Vector2 Target
        {
            get { return target; }
            set { target = value; }
        }

        /// <summary>
        /// A point indicating the X and Y direction the camera is scrolling.
        /// </summary>
        public Point ScrollDirection
        {
            get { return scrollDirection; }
            set { scrollDirection = new Point(Math.Sign(value.X), Math.Sign(value.Y)); }
        }

        /// <summary>
        /// The world-space portion of world that is visible.
        /// </summary>
        public Rectangle ViewBounds
        {
            get { return Rectangle.FromCenterSize(target.X, target.Y, viewportSize.Width / 32f, viewportSize.Height / 32f); }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Updates this <see cref="Camera"/> for a frame.
        /// </summary>
        /// <param name="timeDeltaInSeconds">The time elapsed since the last frame, in seconds.</param>
        public void Update(float timeDeltaInSeconds)
        {
            target += new Vector2(scrollDirection.X, scrollDirection.Y) * timeDeltaInSeconds * scrollSpeed;
            target = new Rectangle(worldSize.Width, worldSize.Height).Clamp(target);
        }

        /// <summary>
        /// Transforms a point in viewport coordinates to world coordinates.
        /// </summary>
        /// <param name="point">The viewport coordinates point to be transformed.</param>
        /// <returns>The resulting world coordinates point.</returns>
        public Vector2 ViewportToWorld(Point point)
        {
            Rectangle viewBounds = ViewBounds;
            return new Vector2(
                viewBounds.MinX + point.X / (float)viewportSize.Width * viewBounds.Width,
                viewBounds.MinY + point.Y / (float)viewportSize.Height * viewBounds.Height);
        }
        #endregion
    }
}
