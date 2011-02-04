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
        private bool isSelectable = true;
        private int aladdiumCost;
        private int alageneCost;
        private TrainType trainType;
        private List<UnitTypeUpgrade> upgrades = new List<UnitTypeUpgrade>();
        private Entity templateEntity;
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
        public TrainType TrainType
        {
            get { return trainType; }
            set { trainType = value; }
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

        [Persistent]
        public bool IsSelectable
        {
            get { return isSelectable; }
            set { isSelectable = false; }
        }

        public Entity TemplateEntity
        {
            get { return templateEntity; }
            set { templateEntity = value; }
        }
        #endregion
    }
}
