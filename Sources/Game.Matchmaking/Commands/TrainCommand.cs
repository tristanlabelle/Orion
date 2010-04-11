using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Orion.Engine;
using Orion.Engine.Collections;
using Orion.Game.Simulation;
using Orion.Game.Simulation.Skills;
using Orion.Game.Simulation.Tasks;

namespace Orion.Game.Matchmaking.Commands
{
    /// <summary>
    /// A <see cref="Command"/> which causes the <see cref="TrainTask"/> task
    /// to be assigned to some <see cref="Unit"/>s.
    /// </summary>
    public sealed class TrainCommand : Command
    {
        #region Fields
        private readonly ReadOnlyCollection<Handle> trainerHandles;
        private readonly Handle traineeTypeHandle;
        #endregion

        #region Constructors
        public TrainCommand(Handle factionHandle, IEnumerable<Handle> trainerHandles, Handle traineeTypeHandle)
            : base(factionHandle)
        {
            Argument.EnsureNotNull(trainerHandles, "trainerHandles");
            this.trainerHandles = trainerHandles.Distinct().ToList().AsReadOnly();
            this.traineeTypeHandle = traineeTypeHandle;
        }
        #endregion

        #region Properties
        public override IEnumerable<Handle> ExecutingEntityHandles
        {
            get { return trainerHandles; }
        }

        public Handle TraineeTypeHandle
        {
            get { return traineeTypeHandle; }
        }
        #endregion

        #region Methods
        public override bool ValidateHandles(Match match)
        {
            Argument.EnsureNotNull(match, "match");

            return IsValidFactionHandle(match, FactionHandle)
                && trainerHandles.All(handle => IsValidEntityHandle(match, handle))
                && IsValidUnitTypeHandle(match, traineeTypeHandle);
        }

        public override void Execute(Match match)
        {
            Argument.EnsureNotNull(match, "match");

            Faction faction = match.World.FindFactionFromHandle(FactionHandle);
            UnitType traineeType = match.UnitTypes.FromHandle(traineeTypeHandle);

            int aladdiumCost = faction.GetStat(traineeType, BasicSkill.AladdiumCostStat);
            int alageneCost = faction.GetStat(traineeType, BasicSkill.AlageneCostStat);

            bool taskQueueFullWarningRaised = false;

            foreach (Handle trainerHandle in trainerHandles)
            {
                Unit trainer = (Unit)match.World.Entities.FromHandle(trainerHandle);
                EnsureTrainingSupported(match, trainer, traineeType);

                if (trainer.TaskQueue.IsFull)
                {
                    // Prevent multiple "task queue full" warnings, the player only needs to know it once,
                    // even when it applies to multiple trainers.
                    if (!taskQueueFullWarningRaised)
                    {
                        faction.RaiseWarning("Impossible d'entraîner un {0}, la queue de tâches est pleine."
                            .FormatInvariant(traineeType.Name));
                        taskQueueFullWarningRaised = true;
                    }

                    continue;
                }

                int foodCost = faction.GetStat(traineeType, BasicSkill.FoodCostStat);
                if (foodCost > faction.RemainingFoodAmount)
                {
                    faction.RaiseWarning("Pas assez de nourriture pour entraîner un {0}."
                        .FormatInvariant(traineeType.Name));
                    break;
                }

                if (alageneCost > faction.AlageneAmount || aladdiumCost > faction.AladdiumAmount)
                {
                    faction.RaiseWarning("Pas assez de ressources pour entraîner un {0}."
                        .FormatInvariant(traineeType.Name));
                    break;
                }

                faction.AlageneAmount -= alageneCost;
                faction.AladdiumAmount -= aladdiumCost;
                trainer.TaskQueue.Enqueue(new TrainTask(trainer, traineeType));
            }
        }

        private void EnsureTrainingSupported(Match match, Unit trainer, UnitType traineeType)
        {
            TrainSkill trainSkill = trainer.Type.TryGetSkill<TrainSkill>();
            if (trainSkill == null)
            {
                throw new InvalidOperationException(
                    "{0} cannot train a {1} without the train skill."
                    .FormatInvariant(trainer, traineeType));
            }

            foreach (string target in trainSkill.Targets)
            {
                UnitType unitType = match.UnitTypes.FromName(target);
                if (unitType == null) continue;

                do
                {
                    if (traineeType == unitType) return;
                    if (unitType.HeroName == null) break;
                    unitType = match.UnitTypes.FromName(unitType.HeroName);
                } while (unitType != null);
            }

            throw new InvalidOperationException(
                "{0} does not support training {1}."
                .FormatInvariant(trainer, traineeType));
        }

        public override string ToString()
        {
            return "Faction {0} trains {1} with {2}"
                .FormatInvariant(FactionHandle, traineeTypeHandle, trainerHandles.ToCommaSeparatedValues());
        }
        
        #region Serialization
        public static void Serialize(TrainCommand command, BinaryWriter writer)
        {
            Argument.EnsureNotNull(command, "command");
            Argument.EnsureNotNull(writer, "writer");

            WriteHandle(writer, command.FactionHandle);
            WriteLengthPrefixedHandleArray(writer, command.trainerHandles);
            WriteHandle(writer, command.traineeTypeHandle);
        }

        public static TrainCommand Deserialize(BinaryReader reader)
        {
            Argument.EnsureNotNull(reader, "reader");

            Handle factionHandle = ReadHandle(reader);
            var trainerHandles = ReadLengthPrefixedHandleArray(reader);
            Handle traineeTypeHandle = ReadHandle(reader);
            return new TrainCommand(factionHandle, trainerHandles, traineeTypeHandle);
        }
        #endregion
        #endregion
    }
}
