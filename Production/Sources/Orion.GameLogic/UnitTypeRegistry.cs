using System.Collections.Generic;
using System.Linq;

namespace Orion.GameLogic
{

    public sealed class UnitTypeRegistry
    {
        private List<UnitType> allUnitTypes = new List<UnitType>();

        #region Proprieties
        public List<UnitType> AllUnitTypes
        {
            get { return allUnitTypes; }
        }
        #endregion

        #region Methods

        public UnitTypeRegistry()
        {
            Create("Jedi");
            Create("Archer");
            Create("Tank");
            Create("Building");
            Create("Extractor");
        }

        public UnitType Create(string name)
        {
            UnitType unitType = new UnitType(name, allUnitTypes.Count);
            allUnitTypes.Add(unitType);
            return unitType;
        }

        public UnitType FromID(int ID)
        {
            return allUnitTypes.FirstOrDefault(unitType => unitType.ID == ID);
        }
        public UnitType FromName(string name)
        {
            return allUnitTypes.FirstOrDefault(unitType => unitType.Name == name);
        }
        #endregion
    }
}
