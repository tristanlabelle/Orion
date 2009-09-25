using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OpenTK.Math;

namespace Orion.GameLogic
{
    /// <summary>
    /// Represents an in-game unit, which can be a character, a vehicle or a building,
    /// depending on its <see cref="UnitType"/>.
    /// </summary>
    [Serializable]
    public sealed class Unit
    {
        #region Fields
        private readonly uint id;
        private readonly UnitType type;
        private readonly World world;
        private Faction faction;
        private Vector2 position;
        private float angle;
        private float damage;

        /// <summary>
        /// The current <see cref="Task"/> of this <see cref="Unit"/>.
        /// This should never be <c>null</c>, <see cref="Tasks.Stand"/> should be used as a null object.
        /// </summary>
        private Task task = Tasks.Stand.Instance;
        #endregion

        #region Constructors
        public Unit(uint id, UnitType type, World world)
        {
            Argument.EnsureNotNull(world, "world");
            Argument.EnsureNotNull(type, "type");

            this.id = id;
            this.type = type;
            this.world = world;
        }
        #endregion

        #region Events
        #endregion

        #region Properties
        #region Identification
        /// <summary>
        /// Gets the unique identifier of this <see cref="Unit"/>.
        /// </summary>
        public uint ID
        {
            get { return id; }
        }

        /// <summary>
        /// Gets the <see cref="UnitType"/> of this <see cref="Unit"/>.
        /// </summary>
        public UnitType Type
        {
            get { return type; }
        }
        #endregion

        #region Affiliation
        /// <summary>
        /// Gets the <see cref="World"/> containing this <see cref="Unit"/>.
        /// </summary>
        public World World
        {
            get { return world; }
        }

        /// <summary>
        /// Accesses the <see cref="Faction"/> which this <see cref="Unit"/> is a member of.
        /// </summary>
        public Faction Faction
        {
            get { return faction; }
            set { faction = value; }
        }
        #endregion

        #region State
        /// <summary>
        /// Accesses the position of this <see cref="Unit"/>, in <see cref="World"/> coordinates.
        /// </summary>
        public Vector2 Position
        {
            get { return position; }
            set { position = value; }
        }

        /// <summary>
        /// Gets the angle this <see cref="Unit"/> is facing.
        /// </summary>
        public float Angle
        {
            get { return angle; }
            set { angle = value; }
        }

        /// <summary>
        /// Accesses the damage that has been inflicted to this <see cref="Unit"/>, in health points.
        /// </summary>
        public float Damage
        {
            get { return damage; }
            set
            {
                Argument.EnsurePositive(value, "Damage");
                damage = value;
            }
        }

        /// <summary>
        /// Accesses the <see cref="Task"/> currently executed by this <see cref="Unit"/>.
        /// </summary>
        public Task Task
        {
            get { return task; }
            set
            {
                Argument.EnsureNotNull(value, "Task");
                if (!task.HasEnded) task.OnCancelled(this);
                task = value;
            }
        }
        #endregion
        #endregion

        #region Methods
        /// <summary>
        /// Updates this <see cref="Unit"/> for a frame.
        /// </summary>
        /// <param name="timeDelta">The time elapsed since the last frame.</param>
        public void Update(float timeDelta)
        {
            task.Update(timeDelta);
        }
        #endregion
    }
}
