using System;
using OpenTK;
using Orion.Engine;
using Orion.Engine.Geometry;

namespace Orion.Game.Simulation
{
    /// <summary>
    /// Represents a location from which resources can be harvested.
    /// </summary>
    [Serializable]
    public sealed class ResourceNode : Entity
    {
        #region Fields
        public const int DefaultTotalAmount = 4000;
        public static readonly Size DefaultSize = new Size(2, 2);

        private readonly ResourceType type;
        private readonly int totalAmount;
        private readonly Point position;
        private int remainingAmount;
        #endregion

        #region Constructors
        internal ResourceNode(World world, Handle handle, ResourceType type, int amount, Point position)
            : base(world, handle)
        {
            Argument.EnsureDefined(type, "type");
            Argument.EnsureStrictlyPositive(amount, "amount");

            this.type = type;
            this.totalAmount = amount;
            this.remainingAmount = amount;
            this.position = position;
        }
        #endregion

        #region Events
        /// <summary>
        /// Raised when the amount remaining in the node changes.
        /// </summary>
        public event Action<ResourceNode> RemainingAmountChanged;
        #endregion

        #region Properties
        public ResourceType Type
        {
            get { return type; }
        }

        public int TotalAmount
        {
            get { return totalAmount; }
        }

        /// <summary>
        /// Accesses the amount of resource which remains in this node.
        /// </summary>
        public int RemainingAmount
        {
            get { return remainingAmount; }
            set
            {
                if (value == remainingAmount) return;

                Argument.EnsurePositive(value, "RemainingAmount");

                remainingAmount = value;
                RemainingAmountChanged.Raise(this);

                if (remainingAmount == 0) Die();
            }
        }

        public override Size Size
        {
            get { return DefaultSize; }
        }

        public new Point Position
        {
            get { return position; }
        }

        public override CollisionLayer CollisionLayer
        {
            get { return type == ResourceType.Alagene ? CollisionLayer.None : CollisionLayer.Ground; }
        }
        #endregion

        #region Methods
        public void Harvest(int amount)
        {
            RemainingAmount -= amount;
        }

        public override string ToString()
        {
            return "{0} {1} node".FormatInvariant(Handle, type);
        }

        protected override Vector2 GetPosition()
        {
            return position;
        }
        #endregion
    }
}
