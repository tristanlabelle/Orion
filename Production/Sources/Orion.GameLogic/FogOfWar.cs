﻿using System;
using OpenTK.Math;
using Orion.Geometry;

namespace Orion.GameLogic
{
    public sealed class FogOfWar
    {
        #region Fields

        //private BitArray2D tiles;
        private short[,] tiles;

        #endregion
        
        #region Constructors
        
        public FogOfWar(int width, int height, Faction faction)
        {
            this.tiles = new short[width, height];
            for (int i = 0; i < width; i++)
                for (int j = 0; j < height; j++)
                    this.tiles[i, j] = short.MaxValue; 
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

        public void UnitMoved(Unit unit, ValueChangedEventArgs<OpenTK.Math.Vector2> eventArgs)
        {
            Vector2 newRoundedPosition = new Vector2(
                (float)Math.Floor(unit.Position.X),
                (float)Math.Floor(unit.Position.Y));

            Vector2 oldRoundedPosition = new Vector2(
                (float)Math.Floor(eventArgs.OldValue.X),
                (float)Math.Floor(eventArgs.OldValue.Y));

            if (newRoundedPosition == oldRoundedPosition)
                return;

            Circle newCircle = new Circle(newRoundedPosition, unit.GetStat(UnitStat.SightRange));
            Circle oldCircle = new Circle(oldRoundedPosition, unit.GetStat(UnitStat.SightRange));

            ModifyUnitSight(newCircle, true);
            ModifyUnitSight(oldCircle, false);
        }

        public void UnitCreated(Unit unit)
        {
            Vector2 newRoundedPosition = new Vector2(
                (float)Math.Floor(unit.Position.X),
                (float)Math.Floor(unit.Position.Y));
            Circle newCircle = new Circle(newRoundedPosition, unit.GetStat(UnitStat.SightRange));
            ModifyUnitSight(newCircle, true);
        }

        public void UnitDied(Unit unit)
        {
            Vector2 newRoundedPosition = new Vector2(
                (float)Math.Floor(unit.Position.X),
                (float)Math.Floor(unit.Position.Y));
            Circle newCircle = new Circle(newRoundedPosition, unit.GetStat(UnitStat.SightRange));
            ModifyUnitSight(newCircle, false);
        }

        private void ModifyUnitSight(Circle sight, bool addOrRemove)
        {
            //addOrRemove : true = add  false = remove
            Rectangle tilesRectangle = CreateTilesRectangle(sight.BoundingRectangle);

            for (int i = (int)tilesRectangle.X; i < tilesRectangle.MaxX; i++)
            {
                for (int j = (int)tilesRectangle.Y; j < tilesRectangle.MaxY; j++)
                {
                    if (sight.ContainsPoint(new Vector2((float)(i + 0.5), (float)(j + 0.5))))
                    {
                        if (addOrRemove)
                            if (tiles[i, j] == short.MaxValue)
                                tiles[i, j] = 1;
                            else
                            {
                                System.Diagnostics.Debug.Assert(tiles[i, j] != short.MaxValue - 1);
                                tiles[i, j]++;
                            }
                        else
                            tiles[i, j]--;
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

        /// <summary>
        /// Indicates if the tile was seen at the specified coordinate.
        /// </summary>
        /// <param name="x">The x coordinate in the field.</param>
        /// <param name="y">The y coordinate in the field.</param>
        /// <returns>A boolean value from the fog of war field.</returns>
        public bool HasSeenTile(int x, int y)
        {
            if (tiles[x, y] == short.MaxValue)
                return false;
            return true;
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
            if (tiles[x, y] == 0 || tiles[x, y] == short.MaxValue)
                return false;
            return true;
        }

        public bool SeesTileCurrently(Vector2 position)
        {
            return SeesTileCurrently((int)position.X, (int)position.Y);
        }

        public bool SeesTileCurrently(Point16 point)
        {
            return SeesTileCurrently(point.X, point.Y);
        }
        
        #endregion


    }
}
