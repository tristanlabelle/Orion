using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.Engine;
using Orion.Game.Simulation;
using System.Diagnostics;

namespace Orion.Game.Matchmaking.Commands.Pipeline
{
    /// <summary>
    /// Randomly swaps units to be trained with their heroes.
    /// </summary>
    public sealed class RandomHeroTrainer : CommandFilter
    {
        #region Fields
        public static readonly float DefaultProbability = 0.01f;

        private readonly Match match;
        private readonly float probability;
        private readonly Queue<Command> accumulatedCommands = new Queue<Command>();
        #endregion

        #region Constructors
        public RandomHeroTrainer(Match match, float probability)
        {
            Argument.EnsureNotNull(match, "match");
            Argument.EnsureWithin(probability, 0, 1, "probability");

            this.match = match;
            this.probability = probability;
        }

        public RandomHeroTrainer(Match match)
            : this(match, DefaultProbability) { }
        #endregion

        #region Properties
        public float Probability
        {
            get { return probability; }
        }

        private UnitTypeRegistry UnitTypes
        {
            get { return match.UnitTypes; }
        }
        #endregion

        #region Methods
        public override void Handle(Command command)
        {
            Argument.EnsureNotNull(command, "command");
            accumulatedCommands.Enqueue(command);
        }

        public override void Update(int updateNumber, float timeDeltaInSeconds)
        {
            while (accumulatedCommands.Count > 0)
            {
                Command command = accumulatedCommands.Dequeue();
                TrainCommand trainCommand = command as TrainCommand;
                if (trainCommand == null)
                {
                    Flush(command);
                    continue;
                }

                UnitType traineeType = UnitTypes.FromHandle(trainCommand.TraineeTypeHandle);
                if (traineeType == null)
                {
                    Debug.Fail("Failed to resolve unit type from handle {0}."
                        .FormatInvariant(trainCommand.TraineeTypeHandle));
                    Flush(command);
                    continue;
                }
                
                // Split the command into one command per training,
                // randomizing the heroes for each one.
                foreach (Handle executingEntityHandle in trainCommand.ExecutingUnitHandles)
                {
                    TrainCommand generatedCommand = new TrainCommand(
                        trainCommand.FactionHandle,
                        executingEntityHandle,
                        RandomizeHero(traineeType).Handle);
                    Flush(generatedCommand);
                }
            }
        }

        private UnitType RandomizeHero(UnitType unitType)
        {
            while (true)
            {
                UnitTypeUpgrade upgrade = unitType.Upgrades
                    .FirstOrDefault(u => u.AladdiumCost == 0
                        && u.AlageneCost == 0
                        && match.Random.NextDouble() <= probability);
                if (upgrade == null) break;

                UnitType heroUnitType = UnitTypes.FromName(upgrade.Target);
                if (heroUnitType == null)
                {
#if DEBUG
                    Debug.Fail("Failed to retreive hero unit type {0} for unit type {1}."
                        .FormatInvariant(upgrade.Target, unitType.Name));
#endif
                    break;
                }

                unitType = heroUnitType;
            }

            return unitType;
        }
        #endregion
    }
}
