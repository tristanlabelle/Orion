using System;
using System.Collections.Generic;
using OpenTK.Math;
using Orion.Geometry;
using System.Diagnostics;
using OpenTK.Graphics;

namespace Orion.GameLogic
{
    /// <summary>
    /// Manages the visibility of world regions with regard to the viewpoint of a faction.
    /// </summary>
    [Serializable]
    public sealed class FogOfWar
    {
        #region Nested Types
        private struct RoundedCircle : IEquatable<RoundedCircle>
        {
            #region Instance
            #region Fields
            private readonly int centerX;
            private readonly int centerY;
            private readonly int radius;
            #endregion

            #region Constructors
            public RoundedCircle(Circle circle)
            {
                this.centerX = (int)Math.Floor(circle.Center.X);
                this.centerY = (int)Math.Floor(circle.Center.Y);
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
            public bool Equals(RoundedCircle other)
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
            public static bool Equals(RoundedCircle a, RoundedCircle b)
            {
                return a.Equals(b);
            }
            #endregion

            #region Operators
            public static bool operator ==(RoundedCircle lhs, RoundedCircle rhs)
            {
                return Equals(lhs, rhs);
            }

            public static bool operator !=(RoundedCircle lhs, RoundedCircle rhs)
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
                get { return bitmap.ColumnCount; }
            }

            public int Height
            {
                get { return bitmap.RowCount; }
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
        /// Holds the reference count of each tile in the fog of war.
        /// Indexed by [x, y]. A value of <see cref="ushort.MaxValue"/>
        /// indicates that the tile has never been seen.
        /// </summary>
        private readonly ushort[,] tiles;
        private readonly Dictionary<int, BitArray2D> cachedCircleBitmaps = new Dictionary<int, BitArray2D>();
        private bool isEnabled = true;
        #endregion
        
        #region Constructors
        public FogOfWar(Size size)
        {
            Argument.EnsureStrictlyPositive(size.Area, "size.Area");

            this.tiles = new ushort[size.Width, size.Height];
            for (int i = 0; i < size.Width; i++)
                for (int j = 0; j < size.Height; j++)
                    this.tiles[i, j] = ushort.MaxValue; 
        }
        #endregion

        #region Events
        /// <summary>
        /// Raised when this fog of war changes.
        /// </summary>
        public event GenericEventHandler<FogOfWar, Region> Changed;

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
        /// Gets the size of the terrain, in tiles.
        /// </summary>
        public Size Size
        {
            get { return new Size(tiles.GetLength(0), tiles.GetLength(1)); }
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
        #region Updating
        #region Public Interface
        public void UpdateLineOfSight(Circle oldLineOfSight, Circle newLineOfSight)
        {
            if (!isEnabled) return;

            RoundedCircle roundedOldLineOfSight = new RoundedCircle(oldLineOfSight);
            RoundedCircle roundedNewLineOfSight = new RoundedCircle(newLineOfSight);

            if (roundedNewLineOfSight == roundedOldLineOfSight) return;

            ModifyLineOfSight(roundedOldLineOfSight, false);
            ModifyLineOfSight(roundedNewLineOfSight, true);
        }

        public void AddLineOfSight(Circle lineOfSight)
        {
            if (!isEnabled) return;

            RoundedCircle roundedLineOfSight = new RoundedCircle(lineOfSight);
            ModifyLineOfSight(roundedLineOfSight, true);
        }

        public void RemoveLineOfSight(Circle lineOfSight)
        {
            if (!isEnabled) return;

            RoundedCircle roundedLineOfSight = new RoundedCircle(lineOfSight);
            ModifyLineOfSight(roundedLineOfSight, false);
        }
        #endregion

        #region Private Implementation
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

        private CircleLookup GetCircleLookup(RoundedCircle circle)
        {
            BitArray2D bitmap = GetCircleBitmap(circle.Radius);

            int minX = circle.CenterX - bitmap.ColumnCount / 2;
            int minY = circle.CenterY - bitmap.RowCount / 2;

            return new CircleLookup(minX, minY, bitmap);
        }

        private void ModifyLineOfSight(RoundedCircle lineOfSight, bool addOrRemove)
        {
            if (!isEnabled) return;

            CircleLookup lookup = GetCircleLookup(lineOfSight);
            Point min = new Point(Math.Max(lookup.MinX, 0), Math.Max(lookup.MinY, 0));
            Point exclusiveMax = new Point(
                Math.Min(lookup.ExclusiveMaxX, Size.Width),
                Math.Min(lookup.ExclusiveMaxY, Size.Height));

            for (int x = min.X; x < exclusiveMax.X; x++)
            {
                for (int y = min.Y; y < exclusiveMax.Y; y++)
                {
                    if (!lookup.IsSet(x, y)) continue;

                    if (addOrRemove)
                    {
                        if (tiles[x, y] == ushort.MaxValue)
                        {
                            tiles[x, y] = 1;
                        }
                        else
                        {
                            Debug.Assert(tiles[x, y] != ushort.MaxValue - 1);
                            tiles[x, y]++;
                        }
                    }
                    else
                    {
                        Debug.Assert(tiles[x, y] != ushort.MaxValue);
                        tiles[x, y]--;
                    }
                }
            }

            Region dirtyRectangle = Region.FromMinExclusiveMax(min, exclusiveMax);
            OnChanged(dirtyRectangle);
        }
        #endregion
        #endregion

        #region Testing
        /// <summary>
        /// Gets the visibility status of a tile at the specified coordinates.
        /// </summary>
        /// <param name="point">The point where to check.</param>
        /// <returns>A flag indicating the visibility state of that tile.</returns>
        public TileVisibility GetTileVisibility(Point point)
        {
            ushort value = tiles[point.X, point.Y];
            if (value == ushort.MaxValue) return TileVisibility.Undiscovered;
            return value == 0 ? TileVisibility.Discovered : TileVisibility.Visible;
        }
        #endregion

        #region Cheats
        /// <summary>
        /// Reveals the map, as if the player had seen every tile at least once.
        /// </summary>
        public void Reveal()
        {
            if (!isEnabled) return;

            for (int x = 0; x < Size.Width; x++)
                for (int y = 0; y < Size.Height; y++)
                    if (tiles[x, y] == ushort.MaxValue)
                        tiles[x, y] = 0;

            OnChanged();
        }

        /// <summary>
        /// Disables the fog of war, as if there were always units seeing every tile.
        /// </summary>
        public void Disable()
        {
            if (!isEnabled) return;

            isEnabled = false;

            for (int x = 0; x < Size.Width; x++)
                for (int y = 0; y < Size.Height; y++)
                    tiles[x, y] = 1;

            OnChanged();
        }
        #endregion
        #endregion
        #endregion
    }
}
