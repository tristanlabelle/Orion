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
    /// to be assigned to some <see cref="Entity"/>s.
    /// </summary>
    public sealed class TrainCommand : Command
    {
        #region Fields
        private readonly ReadOnlyCollection<Handle> trainerHandles;
        private readonly Handle traineeTypeHandle;
        private readonly int traineeCount;
        #endregion

        #region Constructors
        public TrainCommand(Handle factionHandle, IEnumerable<Handle> trainerHandles, Handle traineeTypeHandle, int traineeCount)
            : base(factionHandle)
        {
            Argument.EnsureNotNull(trainerHandles, "trainerHandles");
            Argument.EnsureStrictlyPositive(traineeCount, "traineeCount");
            Debug.Assert(traineeCount <= byte.MaxValue);

            this.trainerHandles = trainerHandles.Distinct().ToList().AsReadOnly();
            this.traineeTypeHandle = traineeTypeHandle;
            this.traineeCount = traineeCount;
        }

        public TrainCommand(Handle factionHandle, Handle trainerHandle, Handle traineeTypeHandle, int traineeCount)
            : this(factionHandle, new[] { trainerHandle }, traineeTypeHandle, traineeCount) { }

        public TrainCommand(Handle factionHandle, IEnumerable<Handle> trainerHandles, Handle traineeTypeHandle)
            : this(factionHandle, trainerHandles, traineeTypeHandle, 1) { }

        public TrainCommand(Handle factionHandle, Handle trainerHandle, Handle traineeTypeHandle)
            : this(factionHandle, trainerHandle, traineeTypeHandle, 1) { }
        #endregion

        #region Properties
        public override IEnumerable<Handle> ExecutingUnitHandles
        {
            get { return trainerHandles; }
        }

        public Handle TraineeTypeHandle
        {
            get { return traineeTypeHandle; }
        }

        /// <summary>
        /// Gets the number of trainees to be queued for training.
        /// </summary>
        public int TraineeCount
        {
            get { return traineeCount; }
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

            Unit traineeType = match.UnitTypes.FromHandle(traineeTypeHandle);
            int foodCost = faction.GetStat(traineeType, BasicSkill.FoodCostStat);
            int aladdiumCost = faction.GetStat(traineeType, BasicSkill.AladdiumCostStat);
            int alageneCost = faction.GetStat(traineeType, BasicSkill.AlageneCostStat);

            for (int i = 0; i < traineeCount; ++i)
            {
                foreach (Handle trainerHandle in trainerHandles)
                {
                    Unit trainer = (Unit)match.World.Entities.FromHandle(trainerHandle);

                    if (alageneCost > faction.AlageneAmount || aladdiumCost > faction.AladdiumAmount)
                    {
                        faction.RaiseWarning("Pas assez de ressources pour entraîner un {0}."
                            .FormatInvariant(traineeType.Name));
                        return;
                    }

                    if (foodCost > faction.RemainingFoodAmount)
                    {
                        faction.RaiseWarning("Pas assez de nourriture pour entraîner un {0}."
                            .FormatInvariant(traineeType.Name));
                        return;
                    }

                    faction.AlageneAmount -= alageneCost;
                    faction.AladdiumAmount -= aladdiumCost;

                    // The hero randomization must be done here to so that every individual train
                    // an be a hero or not. Otherwise, heroes are created on all training units.
                    var actualTraineeType = match.RandomizeHero(traineeType);
                    trainer.TaskQueue.Enqueue(new TrainTask(trainer, actualTraineeType));
                }
            }
        }

        public override string ToString()
        {
            return "Faction {0} trains {1} {2} with {3}"
                .FormatInvariant(FactionHandle, traineeCount, traineeTypeHandle,
                trainerHandles.ToCommaSeparatedValues());
        }
        
        #region Serialization
        public static void Serialize(TrainCommand command, BinaryWriter writer)
        {
            Argument.EnsureNotNull(command, "command");
            Argument.EnsureNotNull(writer, "writer");

            WriteHandle(writer, command.FactionHandle);
            WriteLengthPrefixedHandleArray(writer, command.trainerHandles);
            WriteHandle(writer, command.traineeTypeHandle);
            writer.Write((byte)command.traineeCount);
        }

        public static TrainCommand Deserialize(BinaryReader reader)
        {
            Argument.EnsureNotNull(reader, "reader");

            Handle factionHandle = ReadHandle(reader);
            var trainerHandles = ReadLengthPrefixedHandleArray(reader);
            Handle traineeTypeHandle = ReadHandle(reader);
            int traineeCount = reader.ReadByte();
            return new TrainCommand(factionHandle, trainerHandles, traineeTypeHandle, traineeCount);
        }
        #endregion
        #endregion
    }
}
