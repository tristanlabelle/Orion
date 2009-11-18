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
        public override void AddToPipeline(CommandPipeline pipeline)
        {
            pipeline.AddCommander(this);
            commandsEntryPoint = pipeline.AICommandmentEntryPoint;
        }

        public override void Update(float timeDelta)
        {
            allUnits = Faction.Units.ToList();

            if (allUnits.Count > 0)
            {
                int amountOfHarvesters = Evaluate("Harvester", 1);
                int amountOfAttackers = Evaluate("MeleeAttacker", 1);

                DispatchHarvesters(amountOfHarvesters, startingNode);
                initiateTraining("MeleeAttacker", 1);
            }

            base.Update(timeDelta);
        }

        #endregion
    }
}
