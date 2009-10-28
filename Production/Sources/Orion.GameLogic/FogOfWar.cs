using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Orion.Geometry;
using OpenTK.Math;

namespace Orion.GameLogic
{
    class FogOfWar
    {
        #region Fields

        private bool[,] tiles;
        private Faction faction;

        #endregion
        
        #region Constructors
        
        public FogOfWar(int width, int height, Faction faction)
        {
            this.tiles = new bool[width, height];
            
        }

        #endregion

        #region Properties
        /// <summary>
        /// Gets the width of this terrain, in tiles.
        /// </summary>
        public int Width
        {
            get { return tiles.GetLength(0); }
        }

        /// <summary>
        /// Gets the height of this terrain, in tiles.
        /// </summary>
        public int Height
        {
            get { return tiles.GetLength(1); }
        }

        #endregion

        #region Methods

        public void UpdateUnitSight(Unit unit, Vector2 oldPosition)
        {
            if (Math.Floor(unit.Position.X) == Math.Floor(oldPosition.X) &&
                Math.Floor(unit.Position.Y) == Math.Floor(oldPosition.Y))
                return;

            Rectangle tilesRectangle = CreateTilesRectangle(unit.LineOfSight.BoundingRectangle);

            for (int i = (int)tilesRectangle.X; i < tilesRectangle.MaxX; i++)
            {
                for (int j = (int)tilesRectangle.Y; j < tilesRectangle.MaxY; j++)
                {
                    if (unit.LineOfSight.ContainsPoint(new Vector2((float)(i + 0.5), (float)(j + 0.5))))
                    {
                        tiles[i, j] = true;
                    }
                }
            }
        }

        private Rectangle CreateTilesRectangle(Rectangle boundingRectangle)
        {
            float X = (float)Math.Floor(boundingRectangle.X);
            float Y = (float)Math.Floor(boundingRectangle.Y);
            float MaxX = (float)Math.Ceiling(boundingRectangle.MaxX);
            float MaxY = (float)Math.Ceiling(boundingRectangle.MaxY);

            if (Math.Ceiling(boundingRectangle.MaxX) > Width)
            {
                MaxX = Width;
            }
            if (Math.Floor(boundingRectangle.X) < 0)
            {
                X = 0;
            }
            if (Math.Ceiling(boundingRectangle.MaxY) > Height)
            {
                MaxY = Height;
            }
            if (Math.Floor(boundingRectangle.Y) < 0)
            {
                Y = 0;
            }

            return new Rectangle(X, Y, MaxX - X, MaxY - Y);
        }

        public bool HasSeenTile(int x, int y)
        {
            return tiles[x, y];
        }

        public bool HasSeenTile(Vector2 position)
        {
            return HasSeenTile((int)position.X, (int)position.Y);
        }

        public bool HasSeenTile(Point16 point)
        {
            return HasSeenTile(point.X, point.Y);
        }
        
        #endregion


    }
}
