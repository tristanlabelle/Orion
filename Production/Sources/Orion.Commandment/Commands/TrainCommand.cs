using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Orion.GameLogic;
using TrainTask = Orion.GameLogic.Tasks.TrainTask;

namespace Orion.Commandment.Commands
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
        #endregion

        #region Methods
        public override bool ValidateHandles(World world)
        {
            Argument.EnsureNotNull(world, "world");

            return IsValidFactionHandle(world, FactionHandle)
                && trainerHandles.All(handle => IsValidEntityHandle(world, handle))
                && IsValidUnitTypeHandle(world, traineeTypeHandle);
        }

        public override void Execute(Match match)
        {
            Argument.EnsureNotNull(match, "match");

            int alageneTotalCost = 0;
            int aladdiumTotalCost = 0;
            int popNeeded = 0;
            Faction faction = match.World.FindFactionFromHandle(FactionHandle);
            UnitType traineeType = match.World.UnitTypes.FromHandle(traineeTypeHandle);

            int alageneCost = traineeType.GetBaseStat(UnitStat.AlageneCost);
            int aladdiumCost = traineeType.GetBaseStat(UnitStat.AladdiumCost);

            if (trainerHandles.Count * traineeType.FoodCost > faction.RemainingFoodAmount)
            {
#warning Mathieu?
            }

            foreach (Handle trainerHandle in trainerHandles)
            {
                Unit trainer = (Unit)match.World.Entities.FromHandle(trainerHandle);
                //for the first unit we take the food
                if (trainer.TaskQueue.IsEmpty)
                {
                    if ((popNeeded + traineeType.FoodCost) <= faction.RemainingFoodAmount)
                        popNeeded += traineeType.FoodCost;
                    else
                    {
                        faction.RaiseWarning("Not enough food.");
                        continue;
                    }
                }
                if (trainer.TaskQueue.IsFull)
                {
                    faction.RaiseWarning("Cannot train {0}, task queue full."
                        .FormatInvariant(traineeType.Name));
                    continue;
                }

                if (alageneTotalCost + alageneCost <= faction.AlageneAmount
                   && aladdiumTotalCost + aladdiumCost <= faction.AladdiumAmount)
                {
                    alageneTotalCost += alageneCost;
                    aladdiumTotalCost += aladdiumCost;
                    trainer.TaskQueue.Enqueue(new TrainTask(trainer, traineeType));
                }
                else
                {
                    faction.RaiseWarning("Not enough resources to train {0}.".FormatInvariant(traineeType.Name));
                    break;
                }
            }

            // Now we take the cost out for all queued units!
            faction.AlageneAmount -= alageneTotalCost;
            faction.AladdiumAmount -= aladdiumTotalCost;
        }

        public override string ToString()
        {
            return "Faction {0} trains {1} with {2}"
                .FormatInvariant(FactionHandle, traineeTypeHandle, trainerHandles.ToCommaSeparatedValues());
        }
        
        #region Serialization
        protected override void SerializeSpecific(BinaryWriter writer)
        {
            WriteHandle(writer, FactionHandle);
            WriteLengthPrefixedHandleArray(writer, trainerHandles);
            WriteHandle(writer, traineeTypeHandle);
        }

        public static TrainCommand DeserializeSpecific(BinaryReader reader)
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
