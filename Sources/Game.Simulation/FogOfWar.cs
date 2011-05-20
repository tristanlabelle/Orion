using System;
using System.Collections.Generic;
using System.Diagnostics;
using OpenTK;
using Orion.Engine;
using Orion.Engine.Collections;
using Orion.Engine.Geometry;

namespace Orion.Game.Simulation
{
    /// <summary>
    /// Manages a grid that indicates if each world tile is undiscovered, discovered or visible.
    /// </summary>
    /// <remarks>
    /// This class is performance-critical, so there are some dirty tricks here and there,
    /// some willingly duplicated code and not much runtime validation.
    /// </remarks>
    [Serializable]
    public sealed class FogOfWar
    {
        #region Nested Types
        public struct IntegerCircle : IEquatable<IntegerCircle>
        {
            #region Instance
            #region Fields
            private readonly int centerX;
            private readonly int centerY;
            private readonly int radius;
            #endregion

            #region Constructors
            public IntegerCircle(Circle circle)
            {
                this.centerX = (int)Math.Round(circle.Center.X);
                this.centerY = (int)Math.Round(circle.Center.Y);
                this.radius = (int)Math.Round(circle.Radius);
            }
            #endregion

            #region Properties
            public int CenterX
            {
                get { return centerX; }
            }

            public int CenterY
            {
                get { return centerY; }
            }

            public int Radius
            {
                get { return radius; }
            }
            #endregion

            #region Methods
            public bool Equals(IntegerCircle other)
            {
                return centerX == other.centerX
                    && centerY == other.centerY
                    && radius == other.radius;
            }

            public override bool Equals(object obj)
            {
                return base.Equals(obj);
            }

            public override int GetHashCode()
            {
                return centerX ^ centerY ^ radius;
            }
            #endregion
            #endregion

            #region Static
            #region Methods
            public static bool Equals(IntegerCircle a, IntegerCircle b)
            {
                return a.Equals(b);
            }
            #endregion

            #region Operators
            public static bool operator ==(IntegerCircle lhs, IntegerCircle rhs)
            {
                return Equals(lhs, rhs);
            }

            public static bool operator !=(IntegerCircle lhs, IntegerCircle rhs)
            {
                return !Equals(lhs, rhs);
            }
            #endregion
            #endregion
        }

        private struct CircleLookup
        {
            #region Fields
            private readonly int minX;
            private readonly int minY;
            private readonly BitArray2D bitmap;
            #endregion

            #region Constructors
            public CircleLookup(int minX, int minY, BitArray2D bitmap)
            {
                this.minX = minX;
                this.minY = minY;
                this.bitmap = bitmap;
            }
            #endregion

            #region Properties
            public int MinX
            {
                get { return minX; }
            }

            public int MinY
            {
                get { return minY; }
            }

            public int Width
            {
                get { return bitmap.Width; }
            }

            public int Height
            {
                get { return bitmap.Height; }
            }

            public int ExclusiveMaxX
            {
                get { return minX + Width; }
            }

            public int ExclusiveMaxY
            {
                get { return minY + Height; }
            }
            #endregion

            #region Methods
            public bool IsSet(int x, int y)
            {
                if (x < minX || y < minY || x >= ExclusiveMaxX || y >= ExclusiveMaxY)
                    return false;

                return bitmap[y - minY, x - minX];
            }
            #endregion
        }
        #endregion

        #region Instance
        #region Fields
        /// <summary>
        /// The value of undiscovered tiles in the <see cref="tiles"/> array.
        /// </summary>
        /// <remarks>
        /// The topmost bit indicates undiscovery. When incremented,
        /// that bit gets discarded so the tile is considered discovered.
        /// </remarks>
        private const ushort undiscoveredTileValue = 1 << 15;

        /// <summary>
        /// The mask to the reference count in a tile value.
        /// </summary>
        /// <remarks>
        /// This includes all bits but the topmost, which indicates undiscovery.
        /// </remarks>
        private const ushort tileReferenceCountMask = 0x7FFF;

        private readonly int width;
        private readonly int height;
        /// <summary>
        /// Holds the reference count of each tile in the fog of war.
        /// A value of <see cref="undiscoveredTileValue"/>
        /// indicates that the tile has never been seen.
        /// </summary>
        private readonly ushort[] tiles;
        private readonly Dictionary<int, BitArray2D> cachedCircleBitmaps = new Dictionary<int, BitArray2D>();
        private bool isEnabled = true;
        #endregion
        
        #region Constructors
        public FogOfWar(Size size)
        {
            Argument.EnsureStrictlyPositive(size.Area, "size.Area");

            this.width = size.Width;
            this.height = size.Height;
            this.tiles = new ushort[size.Area];
            for (int i = 0; i < size.Area; i++)
                this.tiles[i] = undiscoveredTileValue; 
        }
        #endregion

        #region Events
        /// <summary>
        /// Raised when this fog of war changes.
        /// </summary>
        public event Action<FogOfWar, Region> Changed;

        private void OnChanged(Region dirtyRectangle)
        {
            if (Changed != null) Changed(this, dirtyRectangle);
        }

        private void OnChanged()
        {
            OnChanged((Region)Size);
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets the width of the fog of war grid, in tiles.
        /// </summary>
        public int Width
        {
            get { return width; }
        }

        /// <summary>
        /// Gets the height of the fog of war grid, in tiles.
        /// </summary>
        public int Height
        {
            get { return height; }
        }

        /// <summary>
        /// Gets the size of the fog of war grid, in tiles.
        /// </summary>
        public Size Size
        {
            get { return new Size(width, height); }
        }

        /// <summary>
        /// Gets a value indicating if this fog of war is enabled.
        /// </summary>
        public bool IsEnabled
        {
            get { return isEnabled; }
        }
        #endregion

        #region Methods
        #region Points
        /// <summary>
        /// Adds a vision reference to a fog of war point without raising changed events.
        /// </summary>
        /// <param name="x">The x coordinate of the point.</param>
        /// <param name="y">The y coordinate of the point.</param>
        public void AddSilently(int x, int y)
        {
            Debug.Assert(x >= 0 && x < width && y >= 0 && y < height,
                "Fog of war point out of bounds.");

            int tileIndex = x + y * width;

            Debug.Assert(tiles[tileIndex] != tileReferenceCountMask,
                "Fog of war tile reference count overflow.");

            // Clear the top bit, which indicates undiscovery, and increment
            // the reference count.
            tiles[tileIndex] = (ushort)((tiles[tileIndex] & tileReferenceCountMask) + 1);
        }

        /// <summary>
        /// Removes a vision reference to a fog of war point without raising changed events.
        /// </summary>
        /// <param name="x">The x coordinate of the point.</param>
        /// <param name="y">The y coordinate of the point.</param>
        public void RemoveSilently(int x, int y)
        {
            Debug.Assert(x >= 0 && x < width && y >= 0 && y < height,
                "Fog of war point out of bounds.");

            int tileIndex = x + y * width;

            Debug.Assert((tiles[tileIndex] & tileReferenceCountMask) > 0,
                "Fog of war tile reference count underflow.");

            tiles[tileIndex]--;
        }

        /// <summary>
        /// Reveals a fog of war point without raising changed events.
        /// </summary>
        /// <param name="x">The x coordinate of the point.</param>
        /// <param name="y">The y coordinate of the point.</param>
        public void RevealSilently(int x, int y)
        {
            Debug.Assert(x >= 0 && x < width && y >= 0 && y < height,
                "Fog of war point out of bounds.");

            int tileIndex = x + y * width;
            tiles[tileIndex] &= tileReferenceCountMask;
        }

        public TileVisibility GetTileVisibility(int x, int y)
        {
            Debug.Assert(x >= 0 && x < width && y >= 0 && y < height,
                "Fog of war point out of bounds.");

            int tileIndex = x + y * width;
            ushort value = tiles[tileIndex];
            if (value == undiscoveredTileValue) return TileVisibility.Undiscovered;
            return value == 0 ? TileVisibility.Discovered : TileVisibility.Visible;
        }

        public bool IsDiscovered(int x, int y)
        {
            Debug.Assert(x >= 0 && x < width && y >= 0 && y < height,
                "Fog of war point out of bounds.");

            int tileIndex = x + y * width;
            return tiles[tileIndex] != undiscoveredTileValue;
        }

        public bool IsVisible(int x, int y)
        {
            Debug.Assert(x >= 0 && x < width && y >= 0 && y < height,
                "Fog of war point out of bounds.");

            int tileIndex = x + y * width;
            return (tiles[tileIndex] & tileReferenceCountMask) > 0;
        }
        #endregion

        #region Circles
        /// <summary>
        /// Adds or remove visibility to a given fog of war circle.
        /// </summary>
        /// <param name="circle">The circle to be modified.</param>
        /// <param name="add">
        /// True to add a new vision reference, false to remove one.
        /// </param>
        public void ModifyCircle(Circle circle, bool add)
        {
            if (!isEnabled) return;

            IntegerCircle roundedCircle = new IntegerCircle(circle);
            ModifyCircle(roundedCircle, add);
        }

        public void UpdateCircle(Circle oldCircle, Circle newCircle)
        {
            if (!isEnabled) return;

            IntegerCircle oldRoundedCircle = new IntegerCircle(oldCircle);
            IntegerCircle newRoundedCircle = new IntegerCircle(newCircle);

            if (newRoundedCircle == oldRoundedCircle) return;

            ModifyCircle(oldRoundedCircle, false);
            ModifyCircle(newRoundedCircle, true);
        }

        private BitArray2D GetCircleBitmap(int radius)
        {
            BitArray2D bitmap;
            if (cachedCircleBitmaps.TryGetValue(radius, out bitmap))
                return bitmap;

            Circle circle = new Circle(radius, radius, radius);
            bitmap = new BitArray2D(radius * 2, radius * 2);
            for (int x = 0; x < radius * 2; ++x)
            {
                for (int y = 0; y < radius * 2; ++y)
                {
                    Vector2 point = new Vector2(x + 0.5f, y + 0.5f);
                    bitmap[x, y] = circle.ContainsPoint(point);
                }
            }

            cachedCircleBitmaps.Add(radius, bitmap);

            return bitmap;
        }

        private CircleLookup GetCircleLookup(IntegerCircle circle)
        {
            BitArray2D bitmap = GetCircleBitmap(circle.Radius);

            int minX = circle.CenterX - bitmap.Width / 2;
            int minY = circle.CenterY - bitmap.Height / 2;

            return new CircleLookup(minX, minY, bitmap);
        }

        private void ModifyCircle(IntegerCircle circle, bool add)
        {
            if (!isEnabled) return;

            CircleLookup lookup = GetCircleLookup(circle);

            int minX = Math.Max(lookup.MinX, 0);
            int minY = Math.Max(lookup.MinY, 0);
            int exclusiveMaxX = Math.Min(lookup.ExclusiveMaxX, width);
            int exclusiveMaxY = Math.Min(lookup.ExclusiveMaxY, height);

            if (add)
            {
                for (int y = minY; y < exclusiveMaxY; ++y)
                {
                    for (int x = minY; x < exclusiveMaxX; ++x)
                    {
                        if (!lookup.IsSet(x, y)) continue;
                        AddSilently(x, y);
                    }
                }
            }
            else
            {
                for (int y = minY; y < exclusiveMaxY; ++y)
                {
                    for (int x = minY; x < exclusiveMaxX; ++x)
                    {
                        if (!lookup.IsSet(x, y)) continue;
                        RemoveSilently(x, y);
                    }
                }
            }

            OnChanged(new Region(minX, minY, exclusiveMaxX, exclusiveMaxY));
        }
        #endregion

        #region Regions
        /// <summary>
        /// Adds or remove visibility to a given fog of war region.
        /// </summary>
        /// <param name="region">The region to be modified.</param>
        /// <param name="add">
        /// True to add a new vision reference, false to remove one.
        /// </param>
        public void ModifyRegion(Region region, bool add)
        {
            if (!isEnabled) return;

            int exclusiveMaxX = region.ExclusiveMaxX;
            int exclusiveMaxY = region.ExclusiveMaxY;

            if (add)
            {
                for (int y = region.MinY; y < exclusiveMaxY; ++y)
                    for (int x = region.MinX; x < exclusiveMaxX; ++x)
                        AddSilently(x, y);
            }
            else
            {
                for (int y = region.MinY; y < exclusiveMaxY; ++y)
                    for (int x = region.MinX; x < exclusiveMaxX; ++x)
                        RemoveSilently(x, y);
            }

            OnChanged(region);
        }
        #endregion

        #region Full Grid
        /// <summary>
        /// Reveals the map, as if the player had seen every tile at least once.
        /// </summary>
        public void Reveal()
        {
            if (!isEnabled) return;

            for (int i = 0; i < tiles.Length; ++i)
                tiles[i] &= tileReferenceCountMask;

            OnChanged();
        }

        /// <summary>
        /// Disables the fog of war, as if there were always entities seeing every tile.
        /// </summary>
        public void Disable()
        {
            if (!isEnabled) return;

            isEnabled = false;

            for (int i = 0; i < tiles.Length; ++i)
                tiles[i] = 1;

            OnChanged();
        }
        #endregion
        #endregion
        #endregion
    }
}
