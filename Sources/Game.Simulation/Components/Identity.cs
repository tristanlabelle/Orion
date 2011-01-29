using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.Engine;
using Orion.Game.Simulation.Components.Serialization;

namespace Orion.Game.Simulation.Components
{
    public class Identity : Component
    {
        #region Fields
        private string name;
        private int aladdiumCost;
        private int alageneCost;
        private Size size;
        private List<UnitTypeUpgrade> upgrades = new List<UnitTypeUpgrade>();
        #endregion

        #region Constructors
        public Identity(Entity entity) : base(entity) { }
        #endregion

        #region Properties
        [Mandatory]
        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        [Persistent]
        public int AladdiumCost
        {
            get { return aladdiumCost; }
            set { aladdiumCost = value; }
        }

        [Persistent]
        public int AlageneCost
        {
            get { return alageneCost; }
            set { alageneCost = value; }
        }

        [Persistent]
        public ICollection<UnitTypeUpgrade> Upgrades
        {
            get { return upgrades; }
        }

        [Mandatory]
        public Size Size
        {
            get { return size; }
            set { size = value; }
        }

        [Transient]
        public int Width
        {
            get { return size.Width; }
            set { size = new Size(value, size.Height); }
        }

        [Transient]
        public int Height
        {
            get { return size.Height; }
            set { size = new Size(size.Width, value); }
        }
        #endregion
    }
}
