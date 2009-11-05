using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Size = System.Drawing.Size;

namespace Orion.GameLogic
{
    [Serializable]
    public sealed class UnitTypeRegistry : IEnumerable<UnitType>
    {
        #region Fields
        private readonly Dictionary<string, UnitType> types = new Dictionary<string, UnitType>();
        #endregion

        #region Constructors
        public UnitTypeRegistry()
        {
            RegisterHarvester();
            RegisterBuilder();
            RegisterMeleeAttacker();
            RegisterRangedAttacker();
            RegisterFactory();
            RegisterTower();
            RegisterAlageneExtractor();
        }
        #endregion

        #region Methods
        #region Hard-Coded UnitTypes
        public void RegisterHarvester()
        {
            UnitTypeBuilder builder = new UnitTypeBuilder();
            builder.SizeInTiles = new Size(1, 1);
            builder.SightRange = 8;
            builder.MaxHealth = 5;
            builder.Skills.Add(new Skills.Move(15));
            builder.Skills.Add(new Skills.Harvest(10));
            Register(builder, "Harvester");
        }

        public void RegisterBuilder()
        {
            UnitTypeBuilder builder = new UnitTypeBuilder();
            builder.SizeInTiles = new Size(1, 1);
            builder.SightRange = 6;
            builder.MaxHealth = 8;
            builder.Skills.Add(new Skills.Move(10));
            builder.Skills.Add(new Skills.Build(type => type.IsBuilding, 10));
            builder.Skills.Add(new Skills.Harvest(10));
            Register(builder, "Builder");
        }

        public void RegisterMeleeAttacker()
        {
            UnitTypeBuilder builder = new UnitTypeBuilder();
            builder.SizeInTiles = new Size(1, 1);
            builder.SightRange = 6;
            builder.MaxHealth = 30;
            builder.Skills.Add(new Skills.Move(10));
            builder.Skills.Add(new Skills.Attack(4, 1)); // to avoid unit to alwals do fallow and never attack
            builder.Skills.Add(new Skills.Harvest(10));
            Register(builder, "MeleeAttacker");
        }

        public void RegisterRangedAttacker()
        {
            UnitTypeBuilder builder = new UnitTypeBuilder();
            builder.SizeInTiles = new Size(1, 1);
            builder.SightRange = 10;
            builder.MaxHealth = 30;
            builder.Skills.Add(new Skills.Move(10));
            builder.Skills.Add(new Skills.Attack(4, 7));
            builder.Skills.Add(new Skills.Harvest(10));
            Register(builder, "RangedAttacker");
        }

        public void RegisterFactory()
        {
            UnitTypeBuilder builder = new UnitTypeBuilder();
            builder.SizeInTiles = new Size(3, 3);
            builder.SightRange = 4;
            builder.MaxHealth = 40;
            builder.Skills.Add(new Skills.Train(type => !type.IsBuilding));
            builder.Skills.Add(new Skills.StoreResources());
            Register(builder, "Factory");
        }

        public void RegisterTower()
        {
            UnitTypeBuilder builder = new UnitTypeBuilder();
            builder.SizeInTiles = new Size(2, 2);
            builder.SightRange = 10;
            builder.MaxHealth = 30;
            builder.Skills.Add(new Skills.Attack(4, 7));
            //builder.Skills.Add(new Skills.Train(type => !type.IsBuilding));
            Register(builder, "Tower");
        }

        public void RegisterAlageneExtractor()
        {
            UnitTypeBuilder builder = new UnitTypeBuilder();
            builder.SizeInTiles = new Size(2, 2);
            builder.SightRange = 4;
            builder.MaxHealth = 25;
            builder.Skills.Add(new Skills.ExtractAlagene());
            Register(builder, "AlageneExtractor");
        } 
        #endregion

        public UnitType Register(UnitTypeBuilder builder, string name)
        {
            Argument.EnsureNotNull(builder, "builder");
            UnitType unitType = builder.Build(types.Count, name);
            types.Add(unitType.Name, unitType);
            return unitType;
        }

        public UnitType FromID(int id)
        {
            return types.Values.FirstOrDefault(unitType => unitType.ID == id);
        }

        public UnitType FromName(string name)
        {
            UnitType type;
            types.TryGetValue(name, out type);
            return type;
        }

        public IEnumerator<UnitType> GetEnumerator()
        {
            return types.Values.GetEnumerator();
        }
        #endregion

        #region Explicit Members
        #region IEnumerable Members
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        #endregion
        #endregion
    }
}
