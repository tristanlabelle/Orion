using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Orion.Game.Simulation.Components
{
    public struct Stat
    {
        #region Static
        #region Methods
        public static Stat operator +(Stat a, Stat b)
        {
            if (a.Type == b.Type && a.Type == StatType.Integer)
                return new Stat(a.IntegerValue + b.IntegerValue);
            else
                return new Stat(a.RealValue + b.RealValue);
        }

        public static Stat operator -(Stat a, Stat b)
        {
            if (a.Type == b.Type && a.Type == StatType.Integer)
                return new Stat(a.IntegerValue - b.IntegerValue);
            else
                return new Stat(a.RealValue - b.RealValue);
        }

        public static explicit operator int(Stat that)
        {
            return that.IntegerValue;
        }

        public static explicit operator float(Stat that)
        {
            return that.RealValue;
        }
        #endregion
        #endregion

        #region Instance
        #region Fields
        private readonly StatType type;
        private readonly int integer;
        private readonly float real;
        #endregion

        #region Constructors
        public Stat(int integer)
        {
            type = StatType.Integer;
            this.integer = integer;
            this.real = 0;
        }

        public Stat(float real)
        {
            type = StatType.Real;
            this.real = real;
            this.integer = 0;
        }
        #endregion

        #region Properties
        public StatType Type
        {
            get { return type; }
        }

        public int IntegerValue
        {
            get { return type == StatType.Integer ? integer : (int)real; }
        }

        public float RealValue
        {
            get { return type == StatType.Real ? real : (float)integer; }
        }
        #endregion
        #endregion
    }
}
