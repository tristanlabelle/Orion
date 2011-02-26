using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.Engine;

namespace Orion.Game.Simulation.IO
{
    public class UnitTemplate
    {
        #region Fields
        private readonly Point location;
        private readonly string unitName;
        #endregion

        #region Constructors
        public UnitTemplate(short x, short y, string name)
            : this(new Point(x, y), name)
        { }

        public UnitTemplate(Point location, string unitName)
        {
            this.location = location;
            this.unitName = unitName;
        }
        #endregion

        #region Properties
        public Point Location
        {
            get { return location; }
        }

        public string UnitTypeName
        {
            get { return unitName; }
        }
        #endregion
    }
}
