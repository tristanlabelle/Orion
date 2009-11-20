using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

using Orion.GameLogic;

using TrainTask = Orion.GameLogic.Tasks.Train;

namespace Orion.Commandment.Commands
{
    public sealed class Train : Command
    {
        #region Instance
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
        public override void Execute(World world)
        {
            Argument.EnsureNotNull(world, "world");

            int alageneTotalCost = 0;
            int aladdiumTotalCost = 0;

            Faction faction = world.FindFactionFromHandle(FactionHandle);
            UnitType traineeType = world.UnitTypes.FromHandle(traineeTypeHandle);

            int alageneCost = traineeType.GetBaseStat(UnitStat.AlageneCost);
            int aladdiumCost = traineeType.GetBaseStat(UnitStat.AladdiumCost);
            foreach (Handle trainerHandle in trainerHandles)
            {
                Unit trainer = (Unit)world.Entities.FindFromHandle(trainerHandle);
                if (alageneTotalCost + alageneCost <= faction.AlageneAmount
                   && aladdiumTotalCost + aladdiumCost <= faction.AladdiumAmount)
                {
                    alageneTotalCost += alageneCost;
                    aladdiumTotalCost += aladdiumCost;
                    trainer.EnqueueTask(new TrainTask(trainer, traineeType));
                }
                else
                {
                    Console.WriteLine("Not Enough Ressource to Train all wished units");
                    break;
                }
            }

            // Now we take the cost out for all queued units!
            faction.AlageneAmount -= alageneTotalCost;
            faction.AladdiumAmount -= aladdiumTotalCost;
        }

        public override string ToString()
        {
            return "[{0}] build {1}"
                .FormatInvariant(trainerHandles.ToCommaSeparatedValues(), traineeTypeHandle);
        }
        #endregion
        #endregion

        #region Serializer
        public sealed class Serializer : CommandSerializer<Train>
        {
            #region Instance
            #region Methods
            protected override void SerializeData(Train command, BinaryWriter writer)
            {
                WriteHandle(writer, command.FactionHandle);
                WriteLengthPrefixedHandleArray(writer, command.trainerHandles);
                WriteHandle(writer, command.traineeTypeHandle);
            }

            protected override Train DeserializeData(BinaryReader reader)
            {
                Handle factionHandle = ReadHandle(reader);
                var trainerHandles = ReadLengthPrefixedHandleArray(reader);
                Handle traineeTypeHandle = ReadHandle(reader);
                return new Train(factionHandle, trainerHandles, traineeTypeHandle);
            }
            #endregion
            #endregion

            #region Static
            #region Fields
            /// <summary>
            /// A globally available static instance of this class.
            /// </summary>
            public static readonly Serializer Instance = new Serializer();
            #endregion
            #endregion
        }
        #endregion
    }
}
