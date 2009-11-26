using System;
using System.Linq;
using System.Diagnostics;
using OpenTK.Math;

namespace Orion.GameLogic.Tasks
{
    /// <summary>
    /// A <see cref="Task"/> which makes a <see cref="Unit"/> build a building of a given type.
    /// </summary>
    [Serializable]
    public sealed class Build : Task
    {
        #region Fields
        private readonly Unit builder;
        private readonly UnitType buildingType;
        private readonly Point location;
        private Move move;
        private float heathPointsBuilt = 0;
        private bool hasBegunBuilding = false;
        private bool hasEnded = false;
        Unit unit;
        #endregion

        #region Constructors
        public Build(Unit builder, UnitType buildingType, Point location)
        {
            Argument.EnsureNotNull(builder, "builder");
            if (!builder.HasSkill<Skills.Build>())
                throw new ArgumentException("Cannot build without the build skill.", "builder");
            Argument.EnsureNotNull(buildingType, "buildingType");

            this.builder = builder;
            this.buildingType = buildingType;
            this.location = location;
            this.move = new Move(builder, location);

            if (buildingType.HasSkill<Skills.ExtractAlagene>())
            {
                ResourceNode alageneNode = builder.World.Entities
                            .OfType<ResourceNode>()
                            .FirstOrDefault(node => node.BoundingRectangle.ContainsPoint(location)
                            && node.Type == ResourceType.Alagene);

                bool extractorAlreadyThere = builder.World.Entities
                    .OfType<Unit>()
                    .Any(unit => unit.BoundingRectangle.ContainsPoint(location));

                if (!extractorAlreadyThere && alageneNode != null)
                    location = (Point)alageneNode.Position;
                else
                    hasEnded = true;
            }
        }
        #endregion

        #region Properties
        public override string Description
        {
            get { return "Building"; }
        }

        public override bool HasEnded
        {
            get { return hasEnded; }
        }
        #endregion

        #region Methods
        public override void Update(float timeDelta)
        {
            if (hasEnded) return;

            if (!move.HasEnded)
            {
                move.Update(timeDelta);
                return;
            }
                //Unable To reach Destination
            else if ((builder.Position - (Vector2)location).Length > 1)
            {
                hasEnded = true;
                return;
            }

            if (unit == null)
            {
                int aladdiumCost = builder.Faction.GetStat(buildingType, UnitStat.AladdiumCost);
                int alageneCost = builder.Faction.GetStat(buildingType, UnitStat.AlageneCost);

                if (builder.Faction.AladdiumAmount >= aladdiumCost
                    && builder.Faction.AlageneAmount >= alageneCost)
                {
                    builder.Faction.AladdiumAmount -= aladdiumCost;
                    builder.Faction.AlageneAmount -= alageneCost;
                    unit = builder.Faction.CreateUnit(buildingType, location);
                    unit.Health = 1;
                    builder.Task = new Repair(builder, unit);
                }
                else
                {
                    Debug.WriteLine("Not Enough Resources");
                    hasEnded = true;
                    return;
                }
            }
            

            if (hasBegunBuilding)
            {
                float maxHealth = builder.Faction.GetStat(buildingType, UnitStat.MaxHealth);
                float buildingSpeed = builder.GetStat(UnitStat.BuildingSpeed);
                heathPointsBuilt += buildingSpeed * timeDelta;
                if (heathPointsBuilt >= maxHealth)
                {
                   
                    hasEnded = true;
                }
            }
        }
        #endregion
    }
}
