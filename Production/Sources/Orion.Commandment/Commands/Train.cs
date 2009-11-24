using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Orion.GameLogic;
using TrainTask = Orion.GameLogic.Tasks.Train;

namespace Orion.Commandment.Commands
{
    /// <summary>
    /// A <see cref="Command"/> which causes the <see cref="TrainTask"/> task
    /// to be assigned to some <see cref="Unit"/>s.
    /// </summary>
    public sealed class Train : Command
    {
        #region Fields
        private readonly ReadOnlyCollection<Handle> trainerHandles;
        private readonly Handle traineeTypeHandle;
        #endregion

        #region Constructors
        public Train(Handle factionHandle, IEnumerable<Handle> trainerHandles, Handle traineeTypeHandle)
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

            Faction faction = match.World.FindFactionFromHandle(FactionHandle);
            UnitType traineeType = match.World.UnitTypes.FromHandle(traineeTypeHandle);

            int alageneCost = traineeType.GetBaseStat(UnitStat.AlageneCost);
            int aladdiumCost = traineeType.GetBaseStat(UnitStat.AladdiumCost);
            foreach (Handle trainerHandle in trainerHandles)
            {
                Unit trainer = (Unit)match.World.Entities.FromHandle(trainerHandle);
                if (alageneTotalCost + alageneCost <= faction.AlageneAmount
                   && aladdiumTotalCost + aladdiumCost <= faction.AladdiumAmount)
                {
                    alageneTotalCost += alageneCost;
                    aladdiumTotalCost += aladdiumCost;
                    trainer.EnqueueTask(new TrainTask(trainer, traineeType));
                }
                else
                {
                    Debug.WriteLine("Not Enough Ressource to Train all wished units");
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

        public static Train DeserializeSpecific(BinaryReader reader)
        {
            Argument.EnsureNotNull(reader, "reader");

            Handle factionHandle = ReadHandle(reader);
            var trainerHandles = ReadLengthPrefixedHandleArray(reader);
            Handle traineeTypeHandle = ReadHandle(reader);
            return new Train(factionHandle, trainerHandles, traineeTypeHandle);
        }
        #endregion
        #endregion
    }
}
