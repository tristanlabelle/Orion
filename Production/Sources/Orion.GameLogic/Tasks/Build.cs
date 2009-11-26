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
        private readonly BuildingPlan buildingPlan;
        private Move move;
        private bool hasEnded = false;
        #endregion

        #region Constructors
        public Build(Unit builder, BuildingPlan buildingPlan)
        {
            Argument.EnsureNotNull(builder, "builder");
            if (!builder.HasSkill<Skills.Build>())
                throw new ArgumentException("Cannot build without the build skill.", "builder");
            Argument.EnsureNotNull(buildingPlan, "buildingPlan");
            this.buildingPlan = buildingPlan;
            this.builder = builder;

            this.move = new Move(builder, buildingPlan.Position);

            if (buildingPlan.BuildingType.HasSkill<Skills.ExtractAlagene>())
            {
                ResourceNode alageneNode = builder.World.Entities
                            .OfType<ResourceNode>()
                            .FirstOrDefault(node => node.BoundingRectangle.ContainsPoint(buildingPlan.Position)
                            && node.Type == ResourceType.Alagene);

                bool extractorAlreadyThere = builder.World.Entities
                    .OfType<Unit>()
                    .Any(unit => unit.BoundingRectangle.ContainsPoint(buildingPlan.Position));

                if (!extractorAlreadyThere && alageneNode != null)
                    buildingPlan.Position = (Point)alageneNode.Position;
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
            else if ((builder.Position - (Vector2)buildingPlan.Position).Length > 1)
            {
                hasEnded = true;
                return;
            }

            if (!buildingPlan.ConstructionBegan)
            {
                int aladdiumCost = builder.Faction.GetStat(buildingPlan.BuildingType, UnitStat.AladdiumCost);
                int alageneCost = builder.Faction.GetStat(buildingPlan.BuildingType, UnitStat.AlageneCost);

                if (builder.Faction.AladdiumAmount >= aladdiumCost
                    && builder.Faction.AlageneAmount >= alageneCost)
                {
                    builder.Faction.AladdiumAmount -= aladdiumCost;
                    builder.Faction.AlageneAmount -= alageneCost;
                    buildingPlan.lauchCreationOfThisUnit(builder.Faction.CreateUnit(buildingPlan.BuildingType, buildingPlan.Position));
                    
                    builder.Task = new Repair(builder, buildingPlan.CreatedUnit);
                }
                else
                {
                    Debug.WriteLine("Not Enough Resources");
                    hasEnded = true;
                    return;
                }
            }
            

            else if(buildingPlan.ConstructionBegan)
            {
                builder.Task = new Repair( builder, buildingPlan.CreatedUnit);
            }
        }
        #endregion
    }
}
