using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.Engine;
using Orion.Game.Simulation.Components.Serialization;
using Orion.Engine.Collections;

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
        private bool isBuilding;
        private bool isSelectable = true;
        private bool leavesRemains = true;
        private int aladdiumCost;
        private int alageneCost;
        private float spawnTime = 1;
        private TrainType trainType;
        private readonly ICollection<UnitTypeUpgrade> upgrades
            = new ValidatingCollection<UnitTypeUpgrade>(upgrade => upgrade != null);
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

        /// <summary>
        /// Accesses a value indicating if this <see cref="Entity"/> is classified as a building.
        /// </summary>
        [Persistent]
        public bool IsBuilding
        {
            get { return isBuilding; }
            set { isBuilding = value; }
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

        #region Methods
        /// <summary>
        /// Gets a value indicating if a given <see cref="Entity"/> is a building.
        /// </summary>
        /// <param name="entity">The <see cref="Entity"/> to be tested.</param>
        /// <returns>A value indicating if it is a building.</returns>
        /// <remarks>
        /// This would have been called <c>IsBuilding</c>, but the name would clash with the property.
        /// </remarks>
        public static bool IsEntityBuilding(Entity entity)
        {
            Argument.EnsureNotNull(entity, "entity");

            return entity.Identity != null && entity.Identity.IsBuilding;
        }

        /// <summary>
        /// Attempts to retrieve the prototype of a given <see cref="Entity"/>.
        /// </summary>
        /// <param name="entity">The <see cref="Entity"/> for which the prototype is to be found.</param>
        /// <returns>The prototype <see cref="Entity"/>, or <c>null</c> if it has none.</returns>
        public static Entity GetPrototype(Entity entity)
        {
            Argument.EnsureNotNull(entity, "entity");

            return entity.Identity == null ? null : entity.Identity.Prototype;
        }

        /// <summary>
        /// Gets a value indicating if two given <see cref="Entity">entities</see> have the same prototype.
        /// </summary>
        /// <param name="first">The first <see cref="Entity"/> to be tested.</param>
        /// <param name="second">The second <see cref="Entity"/> to be tested.</param>
        /// <returns>A value indicating if they have the same prototype.</returns>
        public static bool HaveSamePrototype(Entity first, Entity second)
        {
            Argument.EnsureNotNull(first, "first");
            Argument.EnsureNotNull(second, "second");

            return first.Identity != null
                && second.Identity != null
                && first.Identity.Prototype == second.Identity.Prototype;
        }
        #endregion
    }
}
