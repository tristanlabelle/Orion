
using OpenTK.Math;
using Orion.Geometry;
using System;

namespace Orion.GameLogic
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
        private int amountRemaining;
        private Unit extractor = null;
        #endregion

        #region Constructors
        internal ResourceNode(World world, Handle handle, ResourceType type, int amount, Point position)
            : base(world, handle)
        {
            Argument.EnsureDefined(type, "type");
            Argument.EnsureStrictlyPositive(amount, "amount");

            this.type = type;
            this.totalAmount = amount;
            this.amountRemaining = amount;
            this.position = position;
        }
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

        public int AmountRemaining
        {
            get { return amountRemaining; }
            set { amountRemaining = value; }
        }

        public override Size Size
        {
            get { return DefaultSize; }
        }

        public override Vector2 Position
        {
            get { return position; }
        }

        public override CollisionLayer CollisionLayer
        {
            get { return type == ResourceType.Alagene ? CollisionLayer.None : CollisionLayer.Ground; }
        }

        public Unit Extractor
        {
            get { return extractor; }
            set 
            {
                Argument.EnsureNotNull(value, "Extractor");
                extractor = value;
                extractor.Died += new GenericEventHandler<Entity>(OnExtractorDied);
            }
        }
        #endregion

        #region Methods
        public void Harvest(int amount)
        {
            if (amount > amountRemaining)
            {
                throw new ArgumentException(
                    "Cannot harvest {0} points when only {1} remain."
                    .FormatInvariant(amount, amountRemaining), "amount");
            }

            amountRemaining -= amount;
            if (amountRemaining <= 0)
            {
                amountRemaining = 0;
                Die();
            }
        }

        public override string ToString()
        {
            return "{0} {1} node".FormatInvariant(Handle, type);
        }

        public bool IsHarvestableByFaction(Faction faction)
        {
            return (type != ResourceType.Alagene)
                || (extractor != null && extractor.Faction == faction);
        }

        private void OnExtractorDied(Entity sender)
        {
            extractor = null;
        }
        #endregion
    }
}
