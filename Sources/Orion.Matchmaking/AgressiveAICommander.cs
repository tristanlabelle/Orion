using System;
using System.Collections.Generic;
using System.Linq;
using OpenTK.Math;
using Orion.Matchmaking.Commands;
using Orion.Matchmaking.Commands.Pipeline;
using Orion.GameLogic;
using Orion.Geometry;
using Orion.GameLogic.Skills;

namespace Orion.Matchmaking
{
    public class AgressiveAICommander : AICommander
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

                if (startingNode != null)
                {
                    var positions = new List<Vector2> { alageneStartingNode.Position };
                    DispatchBuilders("AlageneExtractor", positions);
                }

                InitiateTraining("Viking", 1);

                if (World.Factions.First().Units.Any(unit => unit.HasSkill<AttackSkill>()))
                    DispatchAttackers(amountOfAttackers, World.Factions.First().Units.First());
            }

            base.Update(timeDelta);
        }

        #endregion
    }
}
