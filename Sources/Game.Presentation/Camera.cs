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
        private const float defaultTileSizeInPixels = 32;
        private const float defaultScrollSpeed = 40;
        private const int maximumZoomLevel = 4;
        private const float zoomLevelInterpolationSpeed = 25;

        private Size worldSize;
        private Size viewportSize;
        private Vector2 target;
        private Point scrollDirection;
        private int targetZoomLevel;
        private float currentZoomLevel;
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
            get { return Rectangle.FromCenterSize(target.X, target.Y, viewportSize.Width / TileSizeInPixels, viewportSize.Height / TileSizeInPixels); }
        }

        /// <summary>
        /// Accesses the current zoom level. Zero is the default level,
        /// a positive value zooms in while a negative value zooms out.
        /// </summary>
        public int ZoomLevel
        {
            get { return targetZoomLevel; }
            set
            {
                if (value < MinimumZoomLevel) value = MinimumZoomLevel;
                if (value > maximumZoomLevel) value = maximumZoomLevel;
                targetZoomLevel = value;
            }
        }

        private int MinimumZoomLevel
        {
            get { return (int)Math.Floor(Math.Log(defaultTileSizeInPixels / Math.Min(worldSize.Width, worldSize.Height), 2) * 2); }
        }

        private float ZoomScale
        {
            get { return (float)Math.Pow(2, currentZoomLevel / 2.0); }
        }

        private float TileSizeInPixels
        {
            get { return ZoomScale * defaultTileSizeInPixels; }
        }

        private float ScrollSpeed
        {
            get { return defaultScrollSpeed / ZoomScale; }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Zooms in of a single zoom level.
        /// </summary>
        public void ZoomIn()
        {
            if (targetZoomLevel < maximumZoomLevel) targetZoomLevel++;
        }

        /// <summary>
        /// Zooms out of a single zoom level.
        /// </summary>
        public void ZoomOut()
        {
            if (targetZoomLevel > MinimumZoomLevel) targetZoomLevel--;
        }

        /// <summary>
        /// Updates this <see cref="Camera"/> for a frame.
        /// </summary>
        /// <param name="timeDelta">The time elapsed since the last frame.</param>
        public void Update(TimeSpan timeDelta)
        {
            float secondsElapsed = (float)timeDelta.TotalSeconds;

            if (currentZoomLevel != targetZoomLevel)
            {
                int interpolationSign = Math.Sign(targetZoomLevel - currentZoomLevel);
                currentZoomLevel += interpolationSign * zoomLevelInterpolationSpeed * secondsElapsed;
                if (Math.Sign(targetZoomLevel - currentZoomLevel) != interpolationSign)
                    currentZoomLevel = targetZoomLevel;
            }

            target += new Vector2(scrollDirection.X, scrollDirection.Y) * secondsElapsed * ScrollSpeed;
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
