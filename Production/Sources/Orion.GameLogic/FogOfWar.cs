using System;
using System.Collections.Generic;
using OpenTK.Math;
using Orion.Geometry;
using System.Diagnostics;
using OpenTK.Graphics;

namespace Orion.GameLogic
{
    [Serializable]
    public sealed class FogOfWar
    {
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
        public FogOfWar(int width, int height)
        {
            Argument.EnsureStrictlyPositive(width, "width");
            Argument.EnsureStrictlyPositive(height, "height");

            this.tiles = new ushort[width, height];
            for (int i = 0; i < width; i++)
                for (int j = 0; j < height; j++)
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
            OnChanged(new Region(0, 0, Width, Height));
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets the width of the terrain, in tiles.
        /// </summary>
        public int Width
        {
            get { return tiles.GetLength(0); }
        }

        /// <summary>
        /// Gets the height of the terrain, in tiles.
        /// </summary>
        public int Height
        {
            get { return tiles.GetLength(1); }
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
            Circle roundedOldLineOfSight = RoundLineOfSight(oldLineOfSight);
            Circle roundedNewLineOfSight = RoundLineOfSight(newLineOfSight);

            if (roundedNewLineOfSight == roundedOldLineOfSight)
                return;

            ModifyLineOfSight(roundedNewLineOfSight, true);
            ModifyLineOfSight(roundedOldLineOfSight, false);
        }

        public void AddLineOfSight(Circle lineOfSight)
        {
            Circle roundedCircle = RoundLineOfSight(lineOfSight);
            ModifyLineOfSight(roundedCircle, true);
        }

        public void RemoveLineOfSight(Circle lineOfSight)
        {
            Circle roundedCircle = RoundLineOfSight(lineOfSight);
            ModifyLineOfSight(roundedCircle, false);
        }
        #endregion

        #region Private Implementation
        private Circle RoundLineOfSight(Circle lineOfSight)
        {
            return new Circle(
                (float)Math.Floor(lineOfSight.Center.X),
                (float)Math.Floor(lineOfSight.Center.Y),
                (float)Math.Round(lineOfSight.Radius));
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

        private void ModifyLineOfSight(Circle lineOfSight, bool addOrRemove)
        {
            if (!isEnabled) return;

            int roundedRadius = (int)Math.Round(lineOfSight.Radius);
            BitArray2D bitmap = GetCircleBitmap(roundedRadius);

            int minX = (int)Math.Floor(lineOfSight.Center.X - bitmap.ColumnCount * 0.5f);
            int minY = (int)Math.Floor(lineOfSight.Center.Y - bitmap.RowCount * 0.5f);
            int exclusiveMaxX = minX + bitmap.ColumnCount;
            int exclusiveMaxY = minY + bitmap.RowCount;

            for (int x = minX; x < exclusiveMaxX; x++)
            {
                if (x < 0 || x >= Width) continue;
                for (int y = minY; y < exclusiveMaxY; y++)
                {
                    if (y < 0 || y >= Height) continue;

                    if (!bitmap[x - minX, y - minY]) continue;

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

            Region dirtyRectangle = Region.FromMinExclusiveMax(
                Math.Max(minX, 0), Math.Max(minY, 0),
                Math.Min(exclusiveMaxX, Width), Math.Min(exclusiveMaxY, Height));
            OnChanged(dirtyRectangle);
        }
        #endregion
        #endregion

        #region Testing
        /// <summary>
        /// Gets the visibility status of a tile at the specified coordinates.
        /// </summary>
        /// <param name="x">The x coordinate of the tile in the field.</param>
        /// <param name="y">The y coordinate of the tile in the field.</param>
        /// <returns>A flag indicating the visibility state of that tile..</returns>
        public TileVisibility GetTileVisibility(int x, int y)
        {
            ushort value = tiles[x, y];
            if (value == ushort.MaxValue) return TileVisibility.Undiscovered;
            return value == 0 ? TileVisibility.Discovered : TileVisibility.Visible;
        }

        public TileVisibility GetTileVisibility(Vector2 position)
        {
            return GetTileVisibility((int)position.X, (int)position.Y);
        }

        public TileVisibility GetTileVisibility(Point16 point)
        {
            return GetTileVisibility(point.X, point.Y);
        }
        #endregion

        #region Cheats
        /// <summary>
        /// Reveals the map, as if the player had seen every tile at least once.
        /// </summary>
        public void Reveal()
        {
            if (!isEnabled) return;

            for (int x = 0; x < Width; x++)
                for (int y = 0; y < Height; y++)
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

            for (int x = 0; x < Width; x++)
                for (int y = 0; y < Height; y++)
                    tiles[x, y] = 1;

            OnChanged();
        }
        #endregion
        #endregion
    }
}
