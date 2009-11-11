
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
        private const int Width = 2;
        private const int Height = 2;

        private readonly ResourceType type;
        private readonly int totalAmount;
        private readonly Vector2 position;
        private int amountRemaining;
        private Unit extractor = null;
        #endregion

        #region Constructors
        internal ResourceNode(World world, int id, ResourceType type, int amount, Vector2 position)
            : base(world, id)
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

        public Vector2 Position
        {
            get { return position; }
        }

        public override Rectangle BoundingRectangle
        {
            get { return Rectangle.FromCenterSize(position.X, position.Y, Width, Height); }
        }

        public Unit Extractor
        {
            get { return extractor; }
            set 
            { 
                extractor = value;
                extractor.Died += new GenericEventHandler<Entity>(ExtractorDied);
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
                OnDied();
            }
        }

        public override string ToString()
        {
            return "#{0} {1} node".FormatInvariant(ID, type);
        }

        public bool IsHarvestableByFaction(Faction faction)
        {
            if (type == ResourceType.Alagene)
            {
                if (extractor == null)
                    return false;
                else
                {
                    if (extractor.Faction == faction)
                        return true;
                    else
                        return false;
                }
            }
            else
                return true;
        }

        private void ExtractorDied(Entity sender)
        {
            extractor = null;
        }

        #endregion
    }
}
