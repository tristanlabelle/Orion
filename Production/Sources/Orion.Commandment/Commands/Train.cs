using System;
using System.Collections.Generic;
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
        private readonly List<Unit> trainers;
        private readonly UnitType traineeType;
        #endregion

        #region Constructors
        public Train(IEnumerable<Unit> trainers, UnitType traineeType, Faction faction)
            : base(faction)
        {
            Argument.EnsureNotNull(traineeType, "traineeType");
            this.trainers = trainers.ToList();
            this.traineeType = traineeType;
        }
        #endregion

        #region Properties
        public override IEnumerable<Entity> EntitiesInvolved
        {
            get { return trainers.Cast<Entity>(); }
        }
        #endregion

        #region Methods
        public override void Execute()
        {
            int alageneTotalCost = 0;
            int aladdiumTotalCost = 0;
            int alageneCost = traineeType.GetBaseStat(UnitStat.AlageneCost);
            int aladdiumCost = traineeType.GetBaseStat(UnitStat.AladdiumCost);
            foreach (Unit building in trainers)
            {

                if (alageneTotalCost + alageneCost <= base.SourceFaction.AlageneAmount
                   && aladdiumTotalCost + aladdiumCost <= base.SourceFaction.AladdiumAmount)
                {
                    alageneTotalCost += alageneCost;
                    aladdiumTotalCost += aladdiumCost;
                    building.EnqueueTask(new TrainTask(building, traineeType));
                }
                else
                {
                    Console.WriteLine("Not Enough Ressource to Train all wished units");
                    break;
                }
            }

            // Now we take the cost out for all queued units!
            base.SourceFaction.AlageneAmount -= alageneTotalCost;
            base.SourceFaction.AladdiumAmount -= aladdiumTotalCost;
        }

        public override string ToString()
        {
            return "[{0}] build {1}"
                .FormatInvariant(trainers.ToCommaSeparatedValues(), traineeType);
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
                writer.Write(command.SourceFaction.Handle.Value);
                writer.Write(command.trainers.Count);
                foreach (Unit unit in command.trainers)
                    writer.Write(unit.Handle.Value);
                writer.Write(command.traineeType.ID);
            }

            protected override Train DeserializeData(BinaryReader reader, World world)
            {
                Faction faction = ReadFaction(reader,world);
                Unit[] units = ReadLengthPrefixedUnitArray(reader, world);
                UnitType unitToCreate = ReadUnitType(reader, world);
                return new Train(units, unitToCreate,faction);
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
