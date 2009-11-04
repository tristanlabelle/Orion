using System;
using System.Collections.Generic;
using OpenTK.Math;
using Orion.Geometry;
using System.Diagnostics;

namespace Orion.GameLogic
{
    [Serializable]
    public sealed class FogOfWar
    {
        #region Fields
        private readonly ushort[,] tiles;
        private readonly Dictionary<int, BitArray2D> cachedCircleBitmaps = new Dictionary<int, BitArray2D>();
        private bool blackSheepWall = false;
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
        public event GenericEventHandler<FogOfWar> Changed;

        private void OnChanged()
        {
            if (Changed != null) Changed(this);
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
        #endregion

        #region Methods
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
            if (blackSheepWall) return;

            int roundedRadius = (int)Math.Round(lineOfSight.Radius);
            BitArray2D bitmap = GetCircleBitmap(roundedRadius);

            int minX = (int)Math.Floor(lineOfSight.Center.X - bitmap.ColumnCount * 0.5f);
            int minY = (int)Math.Floor(lineOfSight.Center.Y - bitmap.RowCount * 0.5f);
            int maxX = minX + bitmap.ColumnCount;
            int maxY = minY + bitmap.RowCount;

            for (int x = minX; x < maxX; x++)
            {
                if (x < 0 || x >= Width) continue;
                for (int y = minY; y < maxY; y++)
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
            OnChanged();
        }

        private Rectangle CreateTilesRectangle(Rectangle boundingRectangle)
        {
            float X = (float)Math.Floor(boundingRectangle.MinX);
            float Y = (float)Math.Floor(boundingRectangle.MinY);
            float MaxX = (float)Math.Ceiling(boundingRectangle.MaxX);
            float MaxY = (float)Math.Ceiling(boundingRectangle.MaxY);

            if (Math.Ceiling(boundingRectangle.MaxX) > Width)
            {
                MaxX = Width;
            }
            if (Math.Floor(boundingRectangle.MinX) < 0)
            {
                X = 0;
            }
            if (Math.Ceiling(boundingRectangle.MaxY) > Height)
            {
                MaxY = Height;
            }
            if (Math.Floor(boundingRectangle.MinY) < 0)
            {
                Y = 0;
            }

            return new Rectangle(X, Y, MaxX - X, MaxY - Y);
        }

        /// <summary>
        /// Indicates if the tile was seen at the specified coordinate.
        /// </summary>
        /// <param name="x">The x coordinate in the field.</param>
        /// <param name="y">The y coordinate in the field.</param>
        /// <returns>A boolean value from the fog of war field.</returns>
        public bool HasSeenTile(int x, int y)
        {
            return tiles[x, y] != ushort.MaxValue;
        }

        public bool HasSeenTile(Vector2 position)
        {
            return HasSeenTile((int)position.X, (int)position.Y);
        }

        public bool HasSeenTile(Point16 point)
        {
            return HasSeenTile(point.X, point.Y);
        }

        /// <summary>
        /// Indicates if the tile at the specified coordinate in currently in the sight of a unit.
        /// </summary>
        /// <param name="x">The x coordinate in the field.</param>
        /// <param name="y">The y coordinate in the field.</param>
        /// <returns>A boolean value from the fog of war field.</returns>
        public bool SeesTileCurrently(int x, int y)
        {
            return tiles[x, y] != 0 && tiles[x, y] != ushort.MaxValue;
        }

        public bool SeesTileCurrently(Vector2 position)
        {
            return SeesTileCurrently((int)position.X, (int)position.Y);
        }

        public bool SeesTileCurrently(Point16 point)
        {
            return SeesTileCurrently(point.X, point.Y);
        }

        public void BlackSheepWall()
        {
            blackSheepWall = true;
            for (int i = 0; i < Width; i++)
                for (int j = 0; j < Height; j++)
                    this.tiles[i, j] = 1;
            OnChanged();
        }

        #endregion
    }
}
