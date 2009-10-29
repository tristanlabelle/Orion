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

        private byte[,] tiles;
        private Faction faction;

        #endregion
        
        #region Constructors
        
        public FogOfWar(int width, int height, Faction faction)
        {
            this.tiles = new byte[width, height];
            
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
            if (Math.Floor(unit.Position.X) == Math.Floor(eventArgs.OldValue.X) &&
                Math.Floor(unit.Position.Y) == Math.Floor(eventArgs.OldValue.Y))
                return;

            ModifyUnitSight(unit, true);

            Unit fakeUnit = unit;
            fakeUnit.Position = eventArgs.OldValue;
            ModifyUnitSight(fakeUnit, false);
        }

        public void UnitCreated(Unit unit)
        {
            ModifyUnitSight(unit, true);
        }

        public void UnitDied(Unit unit)
        {
            ModifyUnitSight(unit, false);
        }

        private void ModifyUnitSight(Unit unit, bool addOrRemove)
        {
            //addOrRemove : true = add  false = remove
            Rectangle tilesRectangle = CreateTilesRectangle(unit.LineOfSight.BoundingRectangle);

            for (int i = (int)tilesRectangle.X; i < tilesRectangle.MaxX; i++)
            {
                for (int j = (int)tilesRectangle.Y; j < tilesRectangle.MaxY; j++)
                {
                    if (unit.LineOfSight.ContainsPoint(new Vector2((float)(i + 0.5), (float)(j + 0.5))))
                    {
                        if (addOrRemove)
                            if (tiles[i, j] == 255)
                                tiles[i, j] = 1;
                            else
                                tiles[i, j]++;
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

        public bool HasSeenTile(int x, int y)
        {
            if (tiles[x, y] == 255)
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

        public bool SeesTileCurrently(int x, int y)
        {
            if (tiles[x, y] == 0 || tiles[x, y] == 255)
                return false;
            return true;
        }
        
        #endregion


    }
}
