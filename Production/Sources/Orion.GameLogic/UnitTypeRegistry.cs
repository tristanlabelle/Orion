using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Size = System.Drawing.Size;

namespace Orion.GameLogic
{
    /// <summary>
    /// Keeps the collection of registered <see cref="UnitType"/>s.
    /// </summary>
    [Serializable]
    public sealed class UnitTypeRegistry : IEnumerable<UnitType>
    {
        #region Fields
        private readonly Dictionary<string, UnitType> types = new Dictionary<string, UnitType>();
        #endregion

        #region Constructors
        public UnitTypeRegistry()
        {
            RegisterSmurf();
            RegisterPirate();
            RegisterNinja();
            RegisterViking();
            RegisterJedi();
            RegisterSwineFlu();
            RegisterUfo();
            RegisterFlyingCarpet();
            RegisterPyramid();
            RegisterBarack();
            RegisterStarPort();
            RegisterTechCenter();
            RegisterTower();
            RegisterAlageneExtractor();
            RegisterSupplyDepot();
            RegisterChuckNorris();
            RegisterMrT();
        }
        #endregion

        #region Methods
        #region Hard-Coded UnitTypes
        public void RegisterSmurf()
        {
            var builder = new UnitTypeBuilder
            {
                Name = "Schtroumpf",
                Size = new Size(1, 1),
                SightRange = 5,
                MaxHealth = 15,
                AladdiumCost = 50,
                AlageneCost = 0,
                FoodCost = 1
            };
            builder.Skills.Add(new Skills.Move(10, false));
            builder.Skills.Add(new Skills.Attack(1, 0, 5));
            builder.Skills.Add(new Skills.Harvest(1, 10));
            builder.Skills.Add(new Skills.Build(type => type.IsBuilding, 20));
            Register(builder);
        }

        public void RegisterPirate()
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
            builder.Skills.Add(new Skills.Attack(3, 0, 3));
            Register(builder);
        }

        public void RegisterNinja()
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

        public void RegisterViking()
        {
            var builder = new UnitTypeBuilder
            {
                Name = "Viking",
                Size = new Size(1, 1),
                SightRange = 6,
                MaxHealth = 75,
                AladdiumCost = 100,
                AlageneCost = 25,
                FoodCost = 3
            };
            builder.Skills.Add(new Skills.Move(10, false));
            builder.Skills.Add(new Skills.Attack(8, 0, 5));
            Register(builder);
        }

        public void RegisterJedi()
        {
            var builder = new UnitTypeBuilder
            {
                Name = "Jedi",
                Size = new Size(1, 1),
                SightRange = 6,
                MaxHealth = 50,
                AladdiumCost = 50,
                AlageneCost = 75,
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
                Size = new Size(2, 2),
                SightRange = 6,
                MaxHealth = 35,
                AladdiumCost = 75,
                AlageneCost = 100,
                FoodCost = 3
            };
            builder.Skills.Add(new Skills.Move(18, true));
            builder.Skills.Add(new Skills.Attack(1, 8, 1));
            Register(builder);
        }

        public void RegisterUfo()
        {
            var builder = new UnitTypeBuilder
            {
                Name = "OVNI",
                Size = new Size(2, 2),
                SightRange = 8,
                MaxHealth = 80,
                AladdiumCost = 150,
                AlageneCost = 200,
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
                Size = new Size(2, 2),
                SightRange = 8,
                MaxHealth = 45,
                AladdiumCost = 25,
                AlageneCost = 50,
                FoodCost = 2
            };
            builder.Skills.Add(new Skills.Move(14, true));
            Register(builder);
        }

        public void RegisterPyramid()
        {
            var builder = new UnitTypeBuilder
            {
                Name = "Pyramide",
                Size = new Size(3, 3),
                SightRange = 8,
                MaxHealth = 400,
                AladdiumCost = 250,
                AlageneCost = 0
            };
            builder.Skills.Add(new Skills.Train(type => type.Name == "Schtroumpf", 10));
            builder.Skills.Add(new Skills.StoreResources());
            builder.Skills.Add(new Skills.StoreFood(10)); 
            Register(builder);
        }

        public void RegisterBarack()
        {
            var builder = new UnitTypeBuilder
            {
                Name = "Temple de 2012",
                Size = new Size(3, 3),
                SightRange = 8,
                MaxHealth = 200,
                AladdiumCost = 150,
                AlageneCost = 0
            };
            builder.Skills.Add(new Skills.Train(type => type.Name == "Pirate" || type.Name == "Ninja" || type.Name == "Viking" || type.Name == "Jedi", 10));
            Register(builder);
        }

        public void RegisterStarPort()
        {
            var builder = new UnitTypeBuilder
            {
                Name = "Portail Démoniaque",
                Size = new Size(3, 3),
                SightRange = 8,
                MaxHealth = 250,
                AladdiumCost = 200,
                AlageneCost = 75
            };
            builder.Skills.Add(new Skills.Train(type => type.Name == "Grippe A(H1N1)" || type.Name == "OVNI" || type.Name == "Tapis Volant", 10));
            Register(builder);
        }

        public void RegisterTechCenter()
        {
            var builder = new UnitTypeBuilder
            {
                Name = "Maison de Tristan",
                Size = new Size(3, 3),
                SightRange = 4,
                MaxHealth = 200,
                AladdiumCost = 150,
                AlageneCost = 50
            };
            builder.Skills.Add(new Skills.Research(technology => technology.Name == "hp boost"));
            Register(builder);
        }

        public void RegisterTower()
        {
            var builder = new UnitTypeBuilder
            {
                Name = "Jean-Marc",
                Size = new Size(3, 3),
                SightRange = 12,
                MaxHealth = 50,
                AladdiumCost = 80,
                AlageneCost = 20
            };
            builder.Skills.Add(new Skills.Attack(17, 10, 2));
            Register(builder);
        }

        public void RegisterAlageneExtractor()
        {
            var builder = new UnitTypeBuilder
            {
                Name = "AlageneExtractor",
                Size = new Size(2, 2),
                SightRange = 4,
                MaxHealth = 12,
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
                SightRange = 4,
                MaxHealth = 80,
                AladdiumCost = 50,
                AlageneCost = 0
            };
            builder.Skills.Add(new Skills.StoreFood(10));
            Register(builder);
        }

        public void RegisterChuckNorris()
        {
            var builder = new UnitTypeBuilder
            {
                Name = "Chuck Norris",
                Size = new Size(5, 5),
                SightRange = 10,
                MaxHealth = 5000
            };
            builder.Skills.Add(new Skills.Move(25, true));
            builder.Skills.Add(new Skills.Attack(100, 0, 1));
            Register(builder);
        }

        public void RegisterMrT()
        {
            var builder = new UnitTypeBuilder
            {
                Name = "Mr T",
                Size = new Size(5, 5),
                SightRange = 20,
                MaxHealth = 1000
            };
            builder.Skills.Add(new Skills.Move(15, true));
            builder.Skills.Add(new Skills.Attack(50, 20, 1));
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
