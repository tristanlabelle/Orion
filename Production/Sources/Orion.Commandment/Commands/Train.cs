using System.Collections.Generic;
using System.IO;
using System.Linq;
using Orion.GameLogic;

namespace Orion.Commandment.Commands
{
    public class Train : Command
    {
        #region Fields
        private readonly List<Unit> identicalsBuildings;
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
            this.identicalsBuildings = selectedsSameBuilding.ToList();
            this.unitType = unitType;
        }
        #endregion

        #region Methods
        public override void Execute()
        {
            int aladiumCost = base.SourceFaction.GetStat(unitType, UnitStat.AladdiumCost);
            int alageneCost = base.SourceFaction.GetStat(unitType, UnitStat.AlageneCost);
            for (int i = 0; i < identicalsBuildings.Count;i++ )
            {
                // If we don't have enought money to continue the production we stop.
                if (!(base.SourceFaction.AladdiumAmount >= (aladiumCost + aladiumCost * i)
                       && base.SourceFaction.AlageneAmount >= (alageneCost + alageneCost * i)))
                    break;
                identicalsBuildings[i].Task = new Orion.GameLogic.Tasks.Train(identicalsBuildings[i], unitType);
            }

        }
        #endregion

        #region Proprieties
        public override IEnumerable<Unit> UnitsInvolved
        {
            get
            {
                foreach (Unit unit in identicalsBuildings)
                    yield return unit;
            }
        }
        #endregion

        public sealed class Serializer : CommandSerializer<Train>
        {
            #region Instance
            #region Properties
            public override byte ID
            {
                get { return 5; }
            }
            #endregion

            #region Methods
            protected override void SerializeData(Train command, BinaryWriter writer)
            {
                writer.Write(command.SourceFaction.ID);
                writer.Write(command.identicalsBuildings.Count());
                foreach (Unit unit in command.identicalsBuildings)
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

    }
}
