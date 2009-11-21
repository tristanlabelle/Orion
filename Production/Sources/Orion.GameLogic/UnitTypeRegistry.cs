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
            RegisterSmurfs();
            RegisterPirates();
            RegisterNinjas();
            RegisterVikings();
            RegisterJedis();
            RegisterSwineFlu();
            RegisterUFO();
            RegisterFlyingCarpet();
            RegisterFactory();
            RegisterTower();
            RegisterAlageneExtractor();
            RegisterSupplyDepot();
        }
        #endregion

        #region Methods
        #region Hard-Coded UnitTypes
        public void RegisterSmurfs()
        {
            var builder = new UnitTypeBuilder
            {
                Name = "Schtroumpf",
                Size = new Size(1, 1),
                SightRange = 5,
                MaxHealth = 15,
                AladdiumCost = 25,
                AlageneCost = 0,
                FoodCost = 1
            };
            builder.Skills.Add(new Skills.Move(10, false));
            builder.Skills.Add(new Skills.Attack(1, 1, 5));
            builder.Skills.Add(new Skills.Harvest(1, 10));
            builder.Skills.Add(new Skills.Build(type => type.IsBuilding, 20));
            Register(builder);
        }

        public void RegisterPirates()
        {
            var builder = new UnitTypeBuilder
            {
                Name = "Pirate",
                Size = new Size(1, 1),
                SightRange = 6,
                MaxHealth = 45,
                AladdiumCost = 50,
                AlageneCost = 0,
                FoodCost = 2
            };
            builder.Skills.Add(new Skills.Move(12, false));
            builder.Skills.Add(new Skills.Attack(3, 1, 3));
            Register(builder);
        }

        public void RegisterNinjas()
        {
            var builder = new UnitTypeBuilder
            {
                Name = "Ninja",
                Size = new Size(1, 1),
                SightRange = 8,
                MaxHealth = 30,
                AladdiumCost = 50,
                AlageneCost = 25,
                FoodCost = 2
            };
            builder.Skills.Add(new Skills.Move(16, false));
            builder.Skills.Add(new Skills.Attack(2, 5, 2));
            Register(builder);
        }

        public void RegisterVikings()
        {
            var builder = new UnitTypeBuilder
            {
                Name = "Viking",
                Size = new Size(1, 1),
                SightRange = 6,
                MaxHealth = 75,
                AladdiumCost = 100,
                AlageneCost = 0,
                FoodCost = 3
            };
            builder.Skills.Add(new Skills.Move(10, false));
            builder.Skills.Add(new Skills.Attack(8, 1, 5));
            Register(builder);
        }

        public void RegisterJedis()
        {
            var builder = new UnitTypeBuilder
            {
                Name = "Jedi",
                Size = new Size(1, 1),
                SightRange = 6,
                MaxHealth = 50,
                AladdiumCost = 75,
                AlageneCost = 50,
                FoodCost = 3
            };
            builder.Skills.Add(new Skills.Move(14, false));
            builder.Skills.Add(new Skills.Attack(4, 10, 3));
            Register(builder);
        }

        public void RegisterSwineFlu()
        {
            var builder = new UnitTypeBuilder
            {
                Name = "Grippe A(H1N1)",
                Size = new Size(1, 1),
                SightRange = 6,
                MaxHealth = 35,
                AladdiumCost = 0,
                AlageneCost = 0,
                FoodCost = 3
            };
            builder.Skills.Add(new Skills.Move(18, true));
            builder.Skills.Add(new Skills.Attack(1, 8, 1));
            Register(builder);
        }

        public void RegisterUFO()
        {
            var builder = new UnitTypeBuilder
            {
                Name = "OVNI",
                Size = new Size(2, 2),
                SightRange = 8,
                MaxHealth = 80,
                AladdiumCost = 125,
                AlageneCost = 75,
                FoodCost = 5
            };
            builder.Skills.Add(new Skills.Move(8, true));
            builder.Skills.Add(new Skills.Attack(18, 12, 8));
            Register(builder);
        }

        public void RegisterFlyingCarpet()
        {
            var builder = new UnitTypeBuilder
            {
                Name = "Tapis Volant",
                Size = new Size(1, 1),
                SightRange = 8,
                MaxHealth = 45,
                AladdiumCost = 50,
                AlageneCost = 25,
                FoodCost = 2
            };
            builder.Skills.Add(new Skills.Move(14, true));
            Register(builder);
        }

        public void RegisterFactory()
        {
            var builder = new UnitTypeBuilder
            {
                Name = "Factory",
                Size = new Size(3, 3),
                SightRange = 8,
                MaxHealth = 400,
                AladdiumCost = 250,
                AlageneCost = 0
            };
            builder.Skills.Add(new Skills.Train(type => !type.IsBuilding, 10));
            builder.Skills.Add(new Skills.StoreResources());
            builder.Skills.Add(new Skills.StoreFood(10)); 
            Register(builder);
        }

        public void RegisterTower()
        {
            var builder = new UnitTypeBuilder
            {
                Name = "Tower",
                Size = new Size(2, 2),
                SightRange = 12,
                MaxHealth = 30,
                AladdiumCost = 80,
                AlageneCost = 20
            };
            builder.Skills.Add(new Skills.Attack(6, 8, 4));
            Register(builder);
        }

        public void RegisterAlageneExtractor()
        {
            var builder = new UnitTypeBuilder
            {
                Name = "AlageneExtractor",
                Size = new Size(2, 2),
                SightRange = 6,
                MaxHealth = 25,
                AladdiumCost = 75,
                AlageneCost = 0
            };
            builder.Skills.Add(new Skills.ExtractAlagene());
            Register(builder);
        }

        public void RegisterSupplyDepot()
        {
            var builder = new UnitTypeBuilder
            {
                Name = "Supply",
                Size = new Size(2, 2),
                SightRange = 6,
                MaxHealth = 70,
                AladdiumCost = 50,
                AlageneCost = 0
            };
            builder.Skills.Add(new Skills.StoreFood(10)); 
            Register(builder);
        }
        #endregion

        public UnitType Register(UnitTypeBuilder builder)
        {
            Argument.EnsureNotNull(builder, "builder");
            UnitType unitType = builder.Build(new Handle((uint)types.Count));
            types.Add(unitType.Name, unitType);
            return unitType;
        }

        public UnitType FromHandle(Handle handle)
        {
            return types.Values.FirstOrDefault(unitType => unitType.Handle == handle);
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
