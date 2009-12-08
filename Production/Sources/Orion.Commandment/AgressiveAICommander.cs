using System;
using System.Collections.Generic;
using System.Linq;
using OpenTK.Math;
using Orion.Commandment.Commands;
using Orion.Commandment.Commands.Pipeline;
using Orion.GameLogic;
using Orion.Geometry;
using Skills = Orion.GameLogic.Skills;

namespace Orion.Commandment
{
    public class AgressiveAICommander:AICommander
    {
        #region Constructors
        public AgressiveAICommander(Faction faction, Random random)
            : base(faction, random)
        {
        }
        #endregion

        #region Methods
        public override void Update(float timeDelta)
        {
            if (Faction.Status == FactionStatus.Defeated)
                return;

            allUnits = Faction.Units.ToList();

            if (allUnits.Count > 0)
            {
                int amountOfHarvesters = Evaluate("Schtroumpf", 1);
                int amountOfAttackers = Evaluate("Viking", 1);

                DispatchHarvesters(amountOfHarvesters, startingNode);
                List<Vector2> positions = new List<Vector2>();

                if (startingNode != null)
                {
                    positions.Add(new Vector2(alageneStartingNode.Position.X, alageneStartingNode.Position.Y));
                    DispatchBuilders("AlageneExtractor", positions);
                }

                InitiateTraining("Viking", 1);

                if(World.Factions.First().Units.Where
                    (unit => unit.HasSkill<Skills.AttackSkill>())
                    .Count() != 0)
                    DispatchAttackers(amountOfAttackers, World.Factions.First().Units.First());
            }

            base.Update(timeDelta);
        }

        #endregion
    }
}
