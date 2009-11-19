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
            RegisterScout();
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
            var builder = new UnitTypeBuilder
            {
                Name = "Harvester",
                Size = new Size(1, 1),
                SightRange = 8,
                MaxHealth = 5,
                AladdiumCost = 25,
                AlageneCost = 0
            };
            builder.Skills.Add(new Skills.Move(15));
            builder.Skills.Add(new Skills.Harvest(1, 10));
            Register(builder);
        }

        public void RegisterBuilder()
        {
            var builder = new UnitTypeBuilder
            {
                Name = "Builder",
                Size = new Size(1, 1),
                SightRange = 6,
                MaxHealth = 8,
                AladdiumCost = 40,
                AlageneCost = 0
            };
            builder.Skills.Add(new Skills.Move(10));
            builder.Skills.Add(new Skills.Build(type => type.IsBuilding, 20));
            builder.Skills.Add(new Skills.Harvest(1, 10));
            Register(builder);
        }

        public void RegisterScout()
        {
            var builder = new UnitTypeBuilder
            {
                Name = "Scout",
                Size = new Size(1, 1),
                SightRange = 10,
                MaxHealth = 4,
                AladdiumCost = 75,
                AlageneCost = 0
            };
            builder.Skills.Add(new Skills.Move(25));
            Register(builder);
        }

        public void RegisterMeleeAttacker()
        {
            var builder = new UnitTypeBuilder
            {
                Name = "MeleeAttacker",
                Size = new Size(1, 1),
                SightRange = 6,
                MaxHealth = 15,
                AladdiumCost = 50,
                AlageneCost = 0
            };
            builder.Skills.Add(new Skills.Move(10));
            builder.Skills.Add(new Skills.Attack(4, 1));
            Register(builder);
        }

        public void RegisterRangedAttacker()
        {
            var builder = new UnitTypeBuilder
            {
                Name = "RangedAttacker",
                Size = new Size(1, 1),
                SightRange = 10,
                MaxHealth = 15,
                AladdiumCost = 50,
                AlageneCost = 10
            };
            builder.Skills.Add(new Skills.Move(10));
            builder.Skills.Add(new Skills.Attack(4, 7));
            Register(builder);
        }

        public void RegisterFactory()
        {
            var builder = new UnitTypeBuilder
            {
                Name = "Factory",
                Size = new Size(3, 3),
                SightRange = 4,
                MaxHealth = 400,
                AladdiumCost = 100,
                AlageneCost = 0
            };
            builder.Skills.Add(new Skills.Train(type => !type.IsBuilding, 10));
            builder.Skills.Add(new Skills.StoreResources());
            Register(builder);
        }

        public void RegisterTower()
        {
            var builder = new UnitTypeBuilder
            {
                Name = "Tower",
                Size = new Size(2, 2),
                SightRange = 10,
                MaxHealth = 30,
                AladdiumCost = 80,
                AlageneCost = 20
            };
            builder.Skills.Add(new Skills.Attack(10, 7));
            Register(builder);
        }

        public void RegisterAlageneExtractor()
        {
            var builder = new UnitTypeBuilder
            {
                Name = "AlageneExtractor",
                Size = new Size(2, 2),
                SightRange = 4,
                MaxHealth = 25,
                AladdiumCost = 75,
                AlageneCost = 0
            };
            builder.Skills.Add(new Skills.ExtractAlagene());
            Register(builder);
        } 
        #endregion

        public UnitType Register(UnitTypeBuilder builder)
        {
            Argument.EnsureNotNull(builder, "builder");
            UnitType unitType = builder.Build(types.Count);
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
