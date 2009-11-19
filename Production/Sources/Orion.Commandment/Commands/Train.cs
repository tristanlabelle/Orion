using System.Collections.Generic;
using System.IO;
using System.Linq;
using Orion.GameLogic;
using System;


namespace Orion.Commandment.Commands
{
    public sealed class Train : Command
    {
        #region Instance
        #region Fields
        private readonly List<Unit> buildings;
        private readonly UnitType unitType;
        #endregion

        #region Constructors
        /// <summary>
        /// Command implemented to build.
        /// </summary>
        /// <param name="selectedUnit">The Builder</param>
        /// <param name="position">Where To build</param>
        /// <param name="unitTobuild">What to build</param>
        public Train(IEnumerable<Unit> selectedsSameBuilding, UnitType unitType, Faction faction)
            : base(faction)
        {
            Argument.EnsureNotNull(unitType, "unitType");
            Argument.EnsureNotNull(faction, "faction");
            this.buildings = selectedsSameBuilding.ToList();
            this.unitType = unitType;
            int AlageneTotalCost = 0;
            int AladdiumTotalCost = 0;
            foreach (Unit unit in buildings)
            {
                AlageneTotalCost += unitType.GetBaseStat(UnitStat.AlageneCost);
                AladdiumTotalCost += unitType.GetBaseStat(UnitStat.AladdiumCost);
                if (AlageneTotalCost <= faction.AlageneAmount
                   && AladdiumTotalCost <= faction.AladdiumAmount)
                {
                    unit.AddUnitToQueue(unit.ID, unitType, faction, unit.Position);

                }
                else
                {
                    // We delete the cost of the last tried unit to build because we didn't
                    // have enough money and the last tested unit will not be created
                    AladdiumTotalCost -= unitType.GetBaseStat(UnitStat.AladdiumCost);
                    AlageneTotalCost -= unitType.GetBaseStat(UnitStat.AlageneCost);
                    Console.WriteLine("Not Enough Ressource to Train all wished units");
                    break;
                }
            }
            // Now we take the cost out of all queued units!
            faction.AlageneAmount -= AlageneTotalCost;
            faction.AladdiumAmount -= AladdiumTotalCost;

        }
        #endregion

        #region Properties
        public override IEnumerable<Entity> EntitiesInvolved
        {
            get { return buildings.Cast<Entity>(); }
        }
        #endregion

        #region Methods
        public override void Execute()
        {
            int aladiumCost = base.SourceFaction.GetStat(unitType, UnitStat.AladdiumCost);
            int alageneCost = base.SourceFaction.GetStat(unitType, UnitStat.AlageneCost);
            for (int i = 0; i < buildings.Count;i++ )
            {
                // If we don't have enought money to continue the production we stop.
                if (!(base.SourceFaction.AladdiumAmount >= (aladiumCost + aladiumCost * i)
                       && base.SourceFaction.AlageneAmount >= (alageneCost + alageneCost * i)))
                    break;
                buildings[i].Task = new Orion.GameLogic.Tasks.Train(buildings[i], unitType);
            }
        }

        public override string ToString()
        {
            return "[{0}] build {1}"
                .FormatInvariant(buildings.ToCommaSeparatedValues(), unitType);
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
                writer.Write(command.SourceFaction.ID);
                writer.Write(command.buildings.Count());
                foreach (Unit unit in command.buildings)
                    writer.Write(unit.ID);
                writer.Write(command.unitType.ID);
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
