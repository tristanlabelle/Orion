using System;
using System.Collections.Generic;
using System.Linq;
using OpenTK.Math;
using Orion.Commandment.Commands;
using Orion.Commandment.Pipeline;
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
            allUnits = Faction.Units.ToList();

            if (allUnits.Count > 0)
            {
                int amountOfHarvesters = Evaluate("Harvester", 0.5f);
                int amountOfAttackers = Evaluate("MeleeAttacker", 1);

                DispatchHarvesters(amountOfHarvesters, startingNode);
                InitiateTraining("MeleeAttacker", 1);

                //TODO: remove this part when the AI is sandboxed as it is only here to test the behavior of the AI
                if(World.Factions.First().Units.Where
                    (unit => unit.HasSkill<Skills.Attack>())
                    .Count() != 0)
                    DispatchAttackers(amountOfAttackers, World.Factions.First().Units.First());
            }

            base.Update(timeDelta);
        }

        #endregion
    }
}
