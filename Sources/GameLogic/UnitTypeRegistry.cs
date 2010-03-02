﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Orion.GameLogic.Skills;

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
            RegisterMentos();
            RegisterDietCoke();

            RegisterTower();
            RegisterWatchTower();
            RegisterAlageneExtractor();
            RegisterSupplyDepot();

            RegisterChuckNorris();
            RegisterMrT();

            RegisterHeroJedihad();
            RegisterHeroJesus();
            RegisterHeroNinja();
            RegisterHeroPirate();
            RegisterHeroSmurf();
            RegisterHeroViking();
            RegisterHeroFlyingSpaghettiMonster();
            RegisterHeroFlyingCarpet();
            RegisterHeroSwineFlu();
            RegisterHeroUfo();
        }
        #endregion

        #region Methods
        #region Hard-Coded UnitTypes
        #region Pyramid & Units
        private void RegisterPyramid()
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
            builder.Skills.Add(new TrainSkill(type => type.Name == "Schtroumpf", 10));
            builder.Skills.Add(new StoreResourcesSkill());
            builder.Skills.Add(new StoreFoodSkill(10));
            Register(builder);
        }

        private void RegisterSmurf()
        {
            var builder = new UnitTypeBuilder
            {
                Name = "Schtroumpf",
                Size = new Size(1, 1),
                SightRange = 5,
                MaxHealth = 20,
                MeleeArmor = 0,
                RangedArmor = 2,
                AladdiumCost = 50,
                AlageneCost = 0,
                FoodCost = 1
            };
            builder.Skills.Add(new MoveSkill(8, false));
            builder.Skills.Add(new AttackSkill(2, 0, 5));
            builder.Skills.Add(new HarvestSkill(1, 10));
            builder.Skills.Add(new BuildSkill(type => type.IsBuilding, 20));
            Register(builder);
        }
        #endregion

        #region Barracks & Units
        private void RegisterBarracks()
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
            builder.Skills.Add(new TrainSkill(type =>
                type.Name == "Pirate" || type.Name == "Ninja" || type.Name == "Viking", 10));
            Register(builder);
        }

        private void RegisterPirate()
        {
            var builder = new UnitTypeBuilder
            {
                Name = "Pirate",
                Size = new Size(1, 1),
                SightRange = 8,
                MaxHealth = 40,
                MeleeArmor = 2,
                RangedArmor = 1,
                AladdiumCost = 50,
                AlageneCost = 0,
                FoodCost = 1
            };
            builder.Skills.Add(new MoveSkill(8, false));
            builder.Skills.Add(new AttackSkill(5, 0, 3));
            Register(builder);
        }

        private void RegisterNinja()
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
            builder.Skills.Add(new MoveSkill(10, false));
            builder.Skills.Add(new AttackSkill(4, 5, 2));
            Register(builder);
        }

        private void RegisterViking()
        {
            var builder = new UnitTypeBuilder
            {
                Name = "Viking",
                Size = new Size(1, 1),
                SightRange = 8,
                MaxHealth = 75,
                MeleeArmor = 4,
                RangedArmor = 2,
                AladdiumCost = 100,
                AlageneCost = 25,
                FoodCost = 3
            };
            builder.Skills.Add(new MoveSkill(6, false));
            builder.Skills.Add(new AttackSkill(14, 0, 4));
            Register(builder);
        }
        #endregion

        #region StarPort & Units
        private void RegisterStarPort()
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
            builder.Skills.Add(new TrainSkill(type =>
                type.Name == "Grippe A(H1N1)" || type.Name == "OVNI" || type.Name == "Tapis Volant", 10));
            Register(builder);
        }

        private void RegisterSwineFlu()
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
                FoodCost = 2
            };
            builder.Skills.Add(new MoveSkill(15, true));
            builder.Skills.Add(new AttackSkill(1, 7, 1));
            Register(builder);
        }

        private void RegisterUfo()
        {
            var builder = new UnitTypeBuilder
            {
                Name = "OVNI",
                Size = new Size(2, 2),
                SightRange = 10,
                MaxHealth = 100,
                MeleeArmor = 1,
                RangedArmor = 4,
                AladdiumCost = 140,
                AlageneCost = 140,
                FoodCost = 2
            };
            builder.Skills.Add(new MoveSkill(8, true));
            builder.Skills.Add(new AttackSkill(13, 8, 8));
            Register(builder);
        }

        private void RegisterFlyingCarpet()
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
            builder.Skills.Add(new MoveSkill(10, true));
            builder.Skills.Add(new TransportSkill(5));
            Register(builder);
        }
        #endregion

        #region Propaganda Center & Units
        private void RegisterPropagandaCenter()
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
            builder.Skills.Add(new TrainSkill(type =>
                type.Name == "Jedihad" || type.Name == "Jésus"
                || type.Name == "Flying Spaghetti Monster", 10));
            Register(builder);
        }

        private void RegisterJedihad()
        {
            var builder = new UnitTypeBuilder
            {
                Name = "Jedihad",
                Size = new Size(1, 1),
                SightRange = 8,
                MaxHealth = 65,
                MeleeArmor = 1,
                RangedArmor = 3,
                AladdiumCost = 20,
                AlageneCost = 75,
                FoodCost = 2
            };
            builder.Skills.Add(new MoveSkill(9, false));
            builder.Skills.Add(new AttackSkill(4, 3, 4));
            Register(builder);
        }

        private void RegisterJesus()
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
                FoodCost = 2
            };
            builder.Skills.Add(new MoveSkill(14, true));
            builder.Skills.Add(new HealSkill(3,4));
            Register(builder);
        }

        private void RegisterFlyingSpaghettiMonster()
        {
            var builder = new UnitTypeBuilder
            {
                Name = "Flying Spaghetti Monster",
                Size = new Size(3, 3),
                SightRange = 10,
                MaxHealth = 150,
                MeleeArmor = 4,
                RangedArmor = 4,
                AladdiumCost = 250,
                AlageneCost = 175,
                FoodCost = 3
            };
            builder.Skills.Add(new MoveSkill(2, true));
            builder.Skills.Add(new AttackSkill(16, 5, 5));
            Register(builder);
        }
        #endregion

        #region Tech Center
        private void RegisterTechCenter()
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
            builder.Skills.Add(new ResearchSkill(technology => true));
            builder.Skills.Add(new TrainSkill(type =>
                type.Name == "Mentos" || type.Name == "Coke diète", 10));
            Register(builder);
        }

        private void RegisterMentos()
        {
            var builder = new UnitTypeBuilder
            {
                Name = "Mentos",
                Size = new Size(1, 1),
                SightRange = 3,
                MaxHealth = 160,
                MeleeArmor = 4,
                RangedArmor = 4,
                AladdiumCost = 3000,
                FoodCost = 5
            };
            builder.Skills.Add(new MoveSkill(3, false));
            builder.Skills.Add(new SuicideBombSkill(type => type.Name == "Coke diète", 10, 400));
            Register(builder);
        }

        private void RegisterDietCoke()
        {
            var builder = new UnitTypeBuilder
            {
                Name = "Coke diète",
                Size = new Size(1, 1),
                SightRange = 3,
                MaxHealth = 160,
                MeleeArmor = 4,
                RangedArmor = 4,
                AlageneCost = 3000,
                FoodCost = 5
            };
            builder.Skills.Add(new MoveSkill(3, false));
            builder.Skills.Add(new SuicideBombSkill(type => type.Name == "Mentos", 10, 400));
            Register(builder);
        }
        #endregion

        #region Other Buildings
        private void RegisterTower()
        {
            var builder = new UnitTypeBuilder
            {
                Name = "Jean-Marc",
                Size = new Size(3, 3),
                SightRange = 7,
                MaxHealth = 120,
                MeleeArmor = 0,
                RangedArmor = 0,
                AladdiumCost = 120,
                AlageneCost = 40
            };
            builder.Skills.Add(new AttackSkill(8, 7, 2));
            Register(builder);
        }

        private void RegisterWatchTower()
        {
            var builder = new UnitTypeBuilder
            {
                Name = "Oeil de Sauron",
                Size = new Size(2, 2),
                SightRange = 16,
                MaxHealth = 100,
                MeleeArmor = 1,
                RangedArmor = 1,
                AladdiumCost = 100,
                AlageneCost = 25
            };
            Register(builder);
        }

        private void RegisterAlageneExtractor()
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
            builder.Skills.Add(new ExtractAlageneSkill());
            Register(builder);
        }

        private void RegisterSupplyDepot()
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
            builder.Skills.Add(new StoreFoodSkill(10));
            Register(builder);
        }
        #endregion

        #region Heroes
        private void RegisterChuckNorris()
        {
            var builder = new UnitTypeBuilder
            {
                Name = "Chuck Norris",
                Size = new Size(5, 5),
                SightRange = 10,
                MaxHealth = 500,
                MeleeArmor = 5,
                RangedArmor = 5,
            };
            builder.Skills.Add(new MoveSkill(20, true));
            builder.Skills.Add(new AttackSkill(75, 0, 1));
            Register(builder);
        }

        private void RegisterMrT()
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
            builder.Skills.Add(new MoveSkill(15, true));
            builder.Skills.Add(new AttackSkill(50, 20, 1));
            Register(builder);
        }

        private void RegisterHeroSmurf()
        {
            var builder = new UnitTypeBuilder
            {
                Name = "Grand Schtroumpf",
                Size = new Size(1, 1),
                SightRange = 5,
                MaxHealth = 60,
                MeleeArmor = 1,
                RangedArmor = 2,
                AladdiumCost = 50,
                AlageneCost = 0,
                FoodCost = 1
            };
            builder.Skills.Add(new MoveSkill(8, false));
            builder.Skills.Add(new AttackSkill(8, 0, 4));
            builder.Skills.Add(new HarvestSkill(1, 10));
            builder.Skills.Add(new BuildSkill(type => type.IsBuilding, 100));
            Register(builder);
        }

        private void RegisterHeroPirate()
        {
            var builder = new UnitTypeBuilder
            {
                Name = "Barbe Bleu",
                Size = new Size(1, 1),
                SightRange = 8,
                MaxHealth = 120,
                MeleeArmor = 4,
                RangedArmor = 2,
                AladdiumCost = 50,
                AlageneCost = 0,
                FoodCost = 1
            };
            builder.Skills.Add(new MoveSkill(8, false));
            builder.Skills.Add(new AttackSkill(16, 0, 3));
            Register(builder);
        }

        private void RegisterHeroNinja()
        {
            var builder = new UnitTypeBuilder
            {
                Name = "Léonardo",
                Size = new Size(1, 1),
                SightRange = 8,
                MaxHealth = 120,
                MeleeArmor = 2,
                RangedArmor = 4,
                AladdiumCost = 50,
                AlageneCost = 25,
                FoodCost = 2
            };
            builder.Skills.Add(new MoveSkill(10, false));
            builder.Skills.Add(new AttackSkill(10, 5, 2));
            Register(builder);
        }

        private void RegisterHeroViking()
        {
            var builder = new UnitTypeBuilder
            {
                Name = "Thor",
                Size = new Size(1, 1),
                SightRange = 8,
                MaxHealth = 200,
                MeleeArmor = 8,
                RangedArmor = 3,
                AladdiumCost = 100,
                AlageneCost = 25,
                FoodCost = 3
            };
            builder.Skills.Add(new MoveSkill(6, false));
            builder.Skills.Add(new AttackSkill(50, 0, 4));
            Register(builder);
        }

        private void RegisterHeroJedihad()
        {
            var builder = new UnitTypeBuilder
            {
                Name = "Allah Skywalker",
                Size = new Size(1, 1),
                SightRange = 8,
                MaxHealth = 160,
                MeleeArmor = 3,
                RangedArmor = 5,
                AladdiumCost = 20,
                AlageneCost = 75,
                FoodCost = 2
            };
            builder.Skills.Add(new MoveSkill(9, false));
            builder.Skills.Add(new AttackSkill(14, 3, 3));
            Register(builder);
        }

        private void RegisterHeroJesus()
        {
            var builder = new UnitTypeBuilder
            {
                Name = "Jésus-Raptor",
                Size = new Size(2, 2),
                SightRange = 8,
                MaxHealth = 200,
                MeleeArmor = 2,
                RangedArmor = 2,
                AladdiumCost = 80,
                AlageneCost = 40,
                FoodCost = 2
            };
            builder.Skills.Add(new MoveSkill(14, true));
            builder.Skills.Add(new AttackSkill(14, 0, 3));
            builder.Skills.Add(new HealSkill(2, 6));
            Register(builder);
        }

        private void RegisterHeroFlyingSpaghettiMonster()
        {
            var builder = new UnitTypeBuilder
            {
                Name = "Ta Mère",
                Size = new Size(3, 3),
                SightRange = 10,
                MaxHealth = 300,
                MeleeArmor = 4,
                RangedArmor = 5,
                AladdiumCost = 250,
                AlageneCost = 175,
                FoodCost = 3
            };
            builder.Skills.Add(new MoveSkill(3, true));
            builder.Skills.Add(new AttackSkill(50, 5, 4));
            Register(builder);
        }

        private void RegisterHeroSwineFlu()
        {
            var builder = new UnitTypeBuilder
            {
                Name = "Anthrax",
                Size = new Size(2, 2),
                SightRange = 9,
                MaxHealth = 100,
                MeleeArmor = 2,
                RangedArmor = 2,
                AladdiumCost = 75,
                AlageneCost = 100,
                FoodCost = 2
            };
            builder.Skills.Add(new MoveSkill(15, true));
            builder.Skills.Add(new AttackSkill(5, 9, 1));
            Register(builder);
        }

        private void RegisterHeroUfo()
        {
            var builder = new UnitTypeBuilder
            {
                Name = "Vaisseau Mère",
                Size = new Size(3, 3),
                SightRange = 10,
                MaxHealth = 250,
                MeleeArmor = 1,
                RangedArmor = 8,
                AladdiumCost = 140,
                AlageneCost = 140,
                FoodCost = 2
            };
            builder.Skills.Add(new MoveSkill(8, true));
            builder.Skills.Add(new AttackSkill(13, 8, 8));
            Register(builder);
        }

        private void RegisterHeroFlyingCarpet()
        {
            var builder = new UnitTypeBuilder
            {
                Name = "Le Tapis d'Aladdin",
                Size = new Size(2, 2),
                SightRange = 8,
                MaxHealth = 100,
                MeleeArmor = 2,
                RangedArmor = 2,
                AladdiumCost = 25,
                AlageneCost = 50,
                FoodCost = 2
            };
            builder.Skills.Add(new MoveSkill(15, true));
            builder.Skills.Add(new TransportSkill(12));
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