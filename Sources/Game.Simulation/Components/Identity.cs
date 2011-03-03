using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.Engine;
using Orion.Game.Simulation.Components.Serialization;

namespace Orion.Game.Simulation.Components
{
    /// <summary>
    /// Marks an <see cref="Entity"/> with general type information.
    /// </summary>
    public sealed class Identity : Component
    {
        #region Fields
        public static readonly Stat AladdiumCostStat = new Stat(typeof(Identity), StatType.Integer, "AladdiumCost");
        public static readonly Stat AlageneCostStat = new Stat(typeof(Identity), StatType.Integer, "AlageneCost");
        public static readonly Stat SpawnTimeStat = new Stat(typeof(Identity), StatType.Real, "SpawnTime");

        private string name;
        private string visualIdentity;
        private string soundIdentity;
        private bool isSelectable = true;
        private bool leavesRemains = true;
        private int aladdiumCost;
        private int alageneCost;
        private float spawnTime = 1;
        private TrainType trainType;
        private List<UnitTypeUpgrade> upgrades = new List<UnitTypeUpgrade>();
        private Entity prototype;
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
        public string VisualIdentity
        {
            get { return visualIdentity ?? name; }
            set { visualIdentity = value; }
        }

        [Persistent]
        public string SoundIdentity
        {
            get { return soundIdentity ?? name; }
            set { soundIdentity = value; }
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
            set
            {
                Argument.EnsurePositive(value, "AladdiumCost");
                aladdiumCost = value;
            }
        }

        [Persistent]
        public int AlageneCost
        {
            get { return alageneCost; }
            set
            {
                Argument.EnsurePositive(value, "AlageneCost");
                alageneCost = value;
            }
        }

        /// <summary>
        /// Accesses the time needed to build or train this <see cref="Entity"/>.
        /// </summary>
        [Persistent]
        public float SpawnTime
        {
            get { return spawnTime; }
            set
            {
                Argument.EnsurePositive(value, "SpawnTime");
                spawnTime = value;
            }
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

        [Persistent]
        public bool LeavesRemains
        {
            get { return leavesRemains; }
            set { leavesRemains = value; }
        }

        public Entity Prototype
        {
            get { return prototype; }
            set { prototype = value; }
        }
        #endregion
    }
}
