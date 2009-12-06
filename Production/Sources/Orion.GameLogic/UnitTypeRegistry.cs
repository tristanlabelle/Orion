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
            RegisterPyramid();
            RegisterSmurf();

            RegisterBarracks();
            RegisterPirate();
            RegisterNinja();
            RegisterViking();

            RegisterStarPort();
            RegisterSwineFlu();
            RegisterUfo();
            RegisterFlyingCarpet();

            RegisterPropagandaCenter();
            RegisterJedihad();
            RegisterJesus();
            RegisterFlyingSpaghettiMonster();

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
        #region Pyramid & Units
        public void RegisterPyramid()
        {
            var builder = new UnitTypeBuilder
            {
                Name = "Pyramide",
                Size = new Size(3, 3),
                SightRange = 8,
                MaxHealth = 400,
                MeleeArmor = 4,
                RangedArmor = 4,
                AladdiumCost = 250,
                AlageneCost = 0
            };
            builder.Skills.Add(new Skills.Train(type => type.Name == "Schtroumpf", 10));
            builder.Skills.Add(new Skills.StoreResources());
            builder.Skills.Add(new Skills.StoreFood(10));
            Register(builder);
        }

        public void RegisterSmurf()
        {
            var builder = new UnitTypeBuilder
            {
                Name = "Schtroumpf",
                Size = new Size(1, 1),
                SightRange = 5,
                MaxHealth = 15,
                MeleeArmor = 0,
                RangedArmor = 2,
                AladdiumCost = 50,
                AlageneCost = 0,
                FoodCost = 1
            };
            builder.Skills.Add(new Skills.Move(8, false));
            builder.Skills.Add(new Skills.Attack(1, 0, 5));
            builder.Skills.Add(new Skills.Harvest(1, 10));
            builder.Skills.Add(new Skills.Build(type => type.IsBuilding, 20));
            Register(builder);
        }
        #endregion

        #region Barracks & Units
        public void RegisterBarracks()
        {
            var builder = new UnitTypeBuilder
            {
                Name = "Temple de 2012",
                Size = new Size(3, 3),
                SightRange = 8,
                MaxHealth = 200,
                MeleeArmor = 3,
                RangedArmor = 3,
                AladdiumCost = 150,
                AlageneCost = 0,
            };
            builder.Skills.Add(new Skills.Train(type =>
                type.Name == "Pirate" || type.Name == "Ninja" || type.Name == "Viking", 10));
            Register(builder);
        }

        public void RegisterPirate()
        {
            var builder = new UnitTypeBuilder
            {
                Name = "Pirate",
                Size = new Size(1, 1),
                SightRange = 6,
                MaxHealth = 40,
                MeleeArmor = 2,
                RangedArmor = 1,
                AladdiumCost = 50,
                AlageneCost = 0,
                FoodCost = 2
            };
            builder.Skills.Add(new Skills.Move(8, false));
            builder.Skills.Add(new Skills.Attack(5, 0, 3));
            Register(builder);
        }

        public void RegisterNinja()
        {
            var builder = new UnitTypeBuilder
            {
                Name = "Ninja",
                Size = new Size(1, 1),
                SightRange = 8,
                MaxHealth = 40,
                MeleeArmor = 1,
                RangedArmor = 2,
                AladdiumCost = 50,
                AlageneCost = 25,
                FoodCost = 2
            };
            builder.Skills.Add(new Skills.Move(10, false));
            builder.Skills.Add(new Skills.Attack(4, 5, 2));
            Register(builder);
        }

        public void RegisterViking()
        {
            var builder = new UnitTypeBuilder
            {
                Name = "Viking",
                Size = new Size(1, 1),
                SightRange = 6,
                MaxHealth = 50,
                MeleeArmor = 3,
                RangedArmor = 2,
                AladdiumCost = 100,
                AlageneCost = 25,
                FoodCost = 3
            };
            builder.Skills.Add(new Skills.Move(6, false));
            builder.Skills.Add(new Skills.Attack(8, 0, 5));
            Register(builder);
        }
        #endregion

        #region StarPort & Units
        public void RegisterStarPort()
        {
            var builder = new UnitTypeBuilder
            {
                Name = "Portail Démoniaque",
                Size = new Size(3, 3),
                SightRange = 8,
                MaxHealth = 300,
                MeleeArmor = 3,
                RangedArmor = 3,
                AladdiumCost = 200,
                AlageneCost = 75
            };
            builder.Skills.Add(new Skills.Train(type =>
                type.Name == "Grippe A(H1N1)" || type.Name == "OVNI" || type.Name == "Tapis Volant", 10));
            Register(builder);
        }

        public void RegisterSwineFlu()
        {
            var builder = new UnitTypeBuilder
            {
                Name = "Grippe A(H1N1)",
                Size = new Size(2, 2),
                SightRange = 7,
                MaxHealth = 40,
                MeleeArmor = 1,
                RangedArmor = 1,
                AladdiumCost = 75,
                AlageneCost = 100,
                FoodCost = 3
            };
            builder.Skills.Add(new Skills.Move(15, true));
            builder.Skills.Add(new Skills.Attack(1, 7, 1));
            Register(builder);
        }

        public void RegisterUfo()
        {
            var builder = new UnitTypeBuilder
            {
                Name = "OVNI",
                Size = new Size(2, 2),
                SightRange = 10,
                MaxHealth = 100,
                MeleeArmor = 1,
                RangedArmor = 4,
                AladdiumCost = 150,
                AlageneCost = 200,
                FoodCost = 5
            };
            builder.Skills.Add(new Skills.Move(8, true));
            builder.Skills.Add(new Skills.Attack(13, 8, 8));
            Register(builder);
        }

        public void RegisterFlyingCarpet()
        {
            var builder = new UnitTypeBuilder
            {
                Name = "Tapis Volant",
                Size = new Size(2, 2),
                SightRange = 8,
                MaxHealth = 75,
                MeleeArmor = 1,
                RangedArmor = 1,
                AladdiumCost = 25,
                AlageneCost = 50,
                FoodCost = 2
            };
            builder.Skills.Add(new Skills.Move(14, true));
            Register(builder);
        }
        #endregion

        #region Propaganda Center & Units
        public void RegisterPropagandaCenter()
        {
            var builder = new UnitTypeBuilder
            {
                Name = "Centre de propagande",
                Size = new Size(3, 3),
                SightRange = 8,
                MaxHealth = 200,
                MeleeArmor = 3,
                RangedArmor = 3,
                AladdiumCost = 250,
                AlageneCost = 125
            };
            builder.Skills.Add(new Skills.Train(type =>
                type.Name == "Jedihad" || type.Name == "Jésus"
                || type.Name == "Flying Spaghetti Monster", 10));
            Register(builder);
        }

        public void RegisterJedihad()
        {
            var builder = new UnitTypeBuilder
            {
                Name = "Jedihad",
                Size = new Size(1, 1),
                SightRange = 8,
                MaxHealth = 65,
                MeleeArmor = 3,
                RangedArmor = 3,
                AladdiumCost = 100,
                AlageneCost = 75,
                FoodCost = 3
            };
            builder.Skills.Add(new Skills.Move(14, false));
            builder.Skills.Add(new Skills.Attack(9, 8, 3));
            Register(builder);
        }

        public void RegisterJesus()
        {
            var builder = new UnitTypeBuilder
            {
                Name = "Jésus",
                Size = new Size(2, 2),
                SightRange = 8,
                MaxHealth = 80,
                MeleeArmor = 1,
                RangedArmor = 1,
                AladdiumCost = 80,
                AlageneCost = 40,
                FoodCost = 3
            };
            builder.Skills.Add(new Skills.Move(14, true));
            Register(builder);
        }

        public void RegisterFlyingSpaghettiMonster()
        {
            var builder = new UnitTypeBuilder
            {
                Name = "Flying Spaghetti Monster",
                Size = new Size(3, 3),
                SightRange = 10,
                MaxHealth = 150,
                MeleeArmor = 4,
                RangedArmor = 4,
                AladdiumCost = 300,
                AlageneCost = 200,
                FoodCost = 8
            };
            builder.Skills.Add(new Skills.Move(2, true));
            builder.Skills.Add(new Skills.Attack(18, 3, 5));
            Register(builder);
        }
        #endregion

        #region Other Buildings
        public void RegisterTechCenter()
        {
            var builder = new UnitTypeBuilder
            {
                Name = "Maison de Tristan",
                Size = new Size(3, 3),
                SightRange = 4,
                MaxHealth = 200,
                MeleeArmor = 3,
                RangedArmor = 3,
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
                SightRange = 10,
                MaxHealth = 150,
                MeleeArmor = 0,
                RangedArmor = 0,
                AladdiumCost = 120,
                AlageneCost = 40
            };
            builder.Skills.Add(new Skills.Attack(12, 8, 2));
            Register(builder);
        }

        public void RegisterAlageneExtractor()
        {
            var builder = new UnitTypeBuilder
            {
                Name = "AlageneExtractor",
                Size = new Size(2, 2),
                SightRange = 4,
                MaxHealth = 75,
                MeleeArmor = 1,
                RangedArmor = 1,
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
                MeleeArmor = 2,
                RangedArmor = 3,
                AladdiumCost = 50,
                AlageneCost = 0
            };
            builder.Skills.Add(new Skills.StoreFood(10));
            Register(builder);
        }
        #endregion

        #region Heroes
        public void RegisterChuckNorris()
        {
            var builder = new UnitTypeBuilder
            {
                Name = "Chuck Norris",
                Size = new Size(5, 5),
                SightRange = 10,
                MaxHealth = 5000,
                MeleeArmor = 50,
                RangedArmor = 50,
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
                MaxHealth = 1000,
                MeleeArmor = 10,
                RangedArmor = 10,
            };
            builder.Skills.Add(new Skills.Move(15, true));
            builder.Skills.Add(new Skills.Attack(50, 20, 1));
            Register(builder);
        }
        #endregion
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
