﻿using System;
using System.Collections.Generic;
using System.Linq;
using OpenTK.Math;
using Orion.Commandment.Commands;
using Orion.Commandment.Pipeline;
using Orion.GameLogic;
using Orion.Geometry;
using Skills = Orion.GameLogic.Skills;

namespace Orion.Commandment
{
    public class AICommander : Commander
    {
        #region Fields
        protected ResourceNode startingNode;
        protected ResourceNode alageneStartingNode;
        private List<Circle> regions = new List<Circle>();
        private Random random;
        protected List<Unit> allUnits = new List<Unit>();
        private List<Command> commands = new List<Command>();
        private bool initialized = false;
        #endregion

        #region Contructors

        public AICommander(Faction faction, Random random)
            : base(faction)
        {
            this.random = random;
        }

        #endregion

        #region Properties
        #endregion

        #region Methods
        public override void Update(float timeDelta)
        {
            if (!initialized && Faction.Units.Count() > 0)
            {
                Initialize();
            }

            if (commands.Count > 0)
            {
                foreach (Command command in commands)
                {
                    GenerateCommand(command);
                }
                commands.Clear();
            }
        }

        /// <summary>
        /// This method calculates the amount of existing units of the caller's faction based on their type and a proportion.
        /// </summary>
        /// <param name="typeName">Name of the type</param>
        /// <param name="proportion">proportion of your faction's units you want to calculate</param>
        /// <returns>the amount of units of the specified type based on given proportion</returns>
        protected int Evaluate(string typeName, float proportion)
        {
            List<Unit> units = new List<Unit>();

            units = allUnits.Where(unit => unit.Faction == Faction && unit.Type.Name == typeName).ToList();

            return (int)Math.Ceiling(units.Count * proportion);
        }

        /// <summary>
        /// This methods finds the node in the map where the ration between the faction calling it and it's enemies is the highest.
        /// </summary>
        /// <param name="type">Type of the resource node to be found</param>
        /// <returns>The node of the specified type that has the highest allyUnits:enemyUnits ratio nearby</returns>
        private ResourceNode FindIdealNode(ResourceType type)
        {
            ResourceNode bestNode = World.Entities.OfType<ResourceNode>().First();

            foreach (ResourceNode node in World.Entities.OfType<ResourceNode>().Where(node => node.Type == type))
            {
                if (node != null)
                {
                    Circle effectiveRange = new Circle(node.Position, 20);

                    int enemyUnits = World.Entities
                        .OfType<Unit>()
                        .Where(unit => unit.Faction != Faction && effectiveRange.ContainsPoint(unit.Position))
                        .Count();

                    int alliedUnits = Faction.Units
                        .Where(unit => effectiveRange.ContainsPoint(unit.Position))
                        .Count();

                    int unitScore = alliedUnits - enemyUnits;

                    Circle currentRange = new Circle(bestNode.Position, 20);

                    int currentEnemyUnits = World.Entities
                        .OfType<Unit>()
                        .Where(unit => unit.Faction != Faction && currentRange.ContainsPoint(unit.Position)).Count();

                    int currentAlliedUnits = Faction.Units
                        .Where(unit => currentRange.ContainsPoint(unit.Position))
                        .Count();

                    int currentScore = currentAlliedUnits - currentEnemyUnits;

                    if (unitScore > currentScore)
                        bestNode = node;
                }
                else
                    return null;
            }

            return bestNode;
        }

        /// <summary>
        /// This unit will create Move commands for the specified units to the specified position.
        /// </summary>
        /// <param name="units">Units to be moved</param>
        /// <param name="position">Point to which the units will move</param>
        public void Move(List<Unit> units, Vector2 position)
        {
            List<Unit> unitsToMove = units.Where(unit => unit.IsIdle).ToList();

            if(unitsToMove.Count > 0)
                commands.Add(new Move(Faction, units, position));
        }

        /// <summary>
        /// This method initiates the training of units from the specified type.  If the specified units can't be trained due to lack of training buildings available or required technology, this will not create any command.
        /// </summary>
        /// <param name="unitTypeName">The name of the type of units to be trained</param>
        /// <param name="amountToBeTrained">The amount of the specified units to be trained</param>
        public void InitiateTraining(string unitTypeName, int amountToBeTrained)
        {
            int amountOfTrains;
            UnitType toTrain = World.UnitTypes.FromName(unitTypeName);
            //Train the Max depending of the ressources
            amountToBeTrained =Math.Min(amountToBeTrained, 
                                        Math.Min(
                                            (int)(Faction.AladdiumAmount / toTrain.GetBaseStat(UnitStat.AladdiumCost)),
                                            (int)(Faction.AlageneAmount / toTrain.GetBaseStat(UnitStat.AlageneCost))
                                        )
                                );



            List<Unit> potentialTrainers = allUnits.Where(unit => unit.Faction == Faction && unit.IsIdle && unit.HasSkill<Skills.Train>()).ToList();
            List<Unit> trainers = new List<Unit>();

            if (amountToBeTrained <= potentialTrainers.Count)
                amountOfTrains = amountToBeTrained;
            else
                amountOfTrains = potentialTrainers.Count;

            if (potentialTrainers.Count > 0)
            {
                for (int i = 0; i < amountOfTrains; i++)
                {
                    trainers.Add(potentialTrainers.ElementAt(0));
                    potentialTrainers.Remove(potentialTrainers.ElementAt(0));
                }
            }

            if (trainers.Count > 0)
                commands.Add(new Train(trainers, toTrain, Faction));
        }

        /// <summary>
        /// Creates Attack commands for the amount of attackers specified.
        /// </summary>
        /// <param name="amountOfAttackers">Amount of attacker units to be dispatched</param>
        /// <param name="target">Target of the attack</param>
        public void DispatchAttackers(int amountOfAttackers, Unit target)
        {
            List<Unit> potentialAttackers = 
                allUnits.Where(unit => unit.Faction == Faction 
                && unit.Type.HasSkill<Skills.Attack>() 
                && unit.IsIdle
                ).ToList();
            List<Unit> attackers = new List<Unit>();

            if (potentialAttackers.Count > 0)
            {
                for (int i = 0; i < amountOfAttackers; i++)
                {
                    attackers.Add(potentialAttackers.ElementAt(0));
                    potentialAttackers.Remove(potentialAttackers.ElementAt(0));
                }
            }

            if (attackers.Count > 0 && target != null)
                commands.Add(new Commands.Attack(Faction, attackers, target));
        }

        /// <summary>
        /// Creates harvest commands for the amount of harvesters specified to harvest a given node.
        /// </summary>
        /// <param name="amountOfHarvesters">The amount of harvesters to be assigned</param>
        /// <param name="node">The node to be harvested</param>
        public void DispatchHarvesters(int amountOfHarvesters, ResourceNode node)
        {
            List<Unit> potentialHarvesters = allUnits.Where(unit => unit.Faction == Faction && unit.IsIdle && unit.Type.HasSkill<Skills.Harvest>()).ToList();
            List<Unit> harvesters = new List<Unit>();

            if (potentialHarvesters.Count > 0)
            {
                for (int i = 0; i < amountOfHarvesters; i++)
                {
                    harvesters.Add(potentialHarvesters.ElementAt(0));
                    potentialHarvesters.Remove(potentialHarvesters.ElementAt(0));
                }
            }

            if (harvesters.Count > 0 && node != null)
                commands.Add(new Commands.Harvest(Faction, harvesters, node));
        }

        /// <summary>
        /// This method creates Build commands to build buildings of the specified types at given positions.  If the faction has less builders than the amount of positions specified, it will create commands for the positions in order until there is no more available builders.
        /// </summary>
        /// <param name="buildingType">Type of building to be built</param>
        /// <param name="Positions">Positions at which the buildings will be built</param>
        public void DispatchBuilders(UnitType buildingType, List<Vector2> Positions)
        {
            int amountOfBuildings;
            List<Unit> potentialBuilders = allUnits.Where(unit => unit.Faction == Faction && unit.Type.HasSkill<Skills.Build>() && unit.Type.HasSkill<Skills.Harvest>()).ToList();
            List<Unit> builders = new List<Unit>();

            if (Positions.Count <= potentialBuilders.Count)
                amountOfBuildings = Positions.Count;
            else
                amountOfBuildings = potentialBuilders.Count;

            for (int i = 0; i < amountOfBuildings; i++)
            {
                builders.Add(potentialBuilders.ElementAt(0));
                potentialBuilders.Remove(potentialBuilders.ElementAt(0));
            }

            for (int i = 0; i < amountOfBuildings; i++)
            {
                commands.Add(new Build(builders.ElementAt(i), Positions.ElementAt(i), buildingType));
            }
        }


        /// <summary>
        /// Initializes certain variables that can't be initialized at construction since the things they refer to aren't created yet.
        /// </summary>
        protected void Initialize()
        {
            allUnits = Faction.Units.ToList();
            startingNode = FindIdealNode(ResourceType.Aladdium);

            initialized = true;
        }

        #endregion
    }
}
