using System;
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
        private bool baseStarted;
        private bool commandCenterBuilt;
        private bool hasBuilding;
        private ResourceNode startingNode;
        private List<Circle> regions = new List<Circle>();
        private Random random;
        private List<ResourceNode> usedNodes = new List<ResourceNode>();
        private bool extractorBuilt = false;
        private ResourceNode alageneStartingNode;
        private int costThreshold = 500;
        private List<Unit> allUnits = new List<Unit>();
        #endregion

        #region Contructors

        public AICommander(Faction faction, Random random)
            : base(faction)
        {
            baseStarted = false;

            foreach (Unit unit in Faction.Units)
            {
                if (unit.Type.IsBuilding)
                    hasBuilding = true;
                else
                    hasBuilding = false;
            }

            this.random = random;
        }

        #endregion

        #region Properties
        #endregion

        #region Methods
        public override void AddToPipeline(CommandPipeline pipeline)
        {
            pipeline.AddCommander(this);
            commandsEntryPoint = pipeline.AICommandmentEntryPoint;
        }

        public override void Update(float timeDelta)
        {
            allUnits = Faction.Units.ToList();

            int[] proportions = Evaluate();

            int alageneHarvesters = proportions[0];
            int harvesterTrainers = proportions[1];
            int factoryBuilders = proportions[2];
            int extractorBuilders = proportions[3];

            List<Unit> harvesters = allUnits.Where(unit => unit.Type.HasSkill<Skills.Harvest>()).ToList();
            List<Unit> builders = allUnits.Where(unit => unit.Type.HasSkill<Skills.Build>()).ToList();
            List<Unit> trainers = allUnits.Where(unit => unit.Type.HasSkill<Skills.Train>()).ToList();

            for (int i = 0; i < (harvesters.Count - alageneHarvesters); i++)
            {
                if(harvesters.ElementAt(i).Task.Description == "harvesting Alagene")
                {
                    //Harvest command = new Harvest(Faction, harvesters.ElementAt(i), startingNode);
                }
            }
        }

        private int [] Evaluate()
        {
            int[] proportions = new int[4];
            proportions[0] = 0;
            proportions[1] = 0;
            proportions[2] = 0;
            proportions[3] = 0;

            if (!extractorBuilt)
            {
                proportions[3] = 1;
            }
            else
            {
                proportions[0] = (int)Math.Ceiling((double)((allUnits.Where(unit => unit.Type.HasSkill<Skills.Harvest>()).ToList().Count) / 4));
            }

            proportions[2] = allUnits.Where(unit => unit.HasSkill<Skills.Harvest>()).ToList().Count - proportions[3];

            proportions[1] = 1;

            return proportions;
        }

        private void BuildExtractor()
        {
            ResourceNode alageneNode = alageneStartingNode;

            List<Unit> builders = Faction.Units.Where(unit => unit.Type.HasSkill<Skills.Build>() && unit.IsIdle).ToList();

            if (builders.Count == 0)
            {
                builders = Faction.Units.Where(unit => unit.Type.HasSkill<Skills.Build>() && unit.Task is Orion.GameLogic.Tasks.Harvest).ToList();
            }

            if (builders.Count > 0)
            {
                Build command = new Build(builders.First(), alageneNode.Position, World.UnitTypes.FromName("AlageneExtractor"));
                GenerateCommand(command);
                usedNodes.Add(alageneNode);
                extractorBuilt = true;
            }
        }

        private void DispatchIdleHarvesters()
        {
            List<Unit> harvesters = Faction.Units.Where(unit => unit.IsIdle && unit.Type.HasSkill<Skills.Harvest>()).ToList();
            ResourceNode node = startingNode;

            if (harvesters.Count != 0)
            {
                if (Faction.AlageneAmount == 0)
                {
                    node = alageneStartingNode;
                    Console.WriteLine("pas d'alagene");
                }
                else
                {
                    if (Faction.AladdiumAmount > Faction.AlageneAmount)
                    {
                        node = alageneStartingNode;
                        Console.WriteLine("pas d'alagene2");
                    }
                    else
                    {
                        node = startingNode;
                        Console.WriteLine("alagene");
                    }
                }

                if (node == null)
                    return;
            }

            if (harvesters.Count != 0)
            {
                Harvest command = new Harvest(Faction, harvesters, node);
                GenerateCommand(command);
                Console.WriteLine("commande creee");
            }

        }

        private ResourceNode FindIdealNode(ResourceType type)
        {
            ResourceNode bestNode = World.Entities.OfType<ResourceNode>().First();

            foreach (ResourceNode node in World.Entities.OfType<ResourceNode>().Where(node => node.Type == type && !usedNodes.Contains(node)))
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

        private void BuildFactories()
        {
            List<Unit> builders = Faction.Units.Where(unit => unit.Type.HasSkill<Skills.Build>() && !(unit.Task is Orion.GameLogic.Tasks.Build)).ToList();

            if(builders.Count != 0)
            {
                Vector2 position = new Vector2();

                do
                {
                    position.X = random.Next((int)World.Bounds.Width);
                    position.Y = random.Next((int)World.Bounds.Height);
                } while (!World.Terrain.IsWalkable(position) || !World.IsWithinBounds(position));

                Command command = new Build(builders.First(), position, World.UnitTypes.FromName("Factory"));
                GenerateCommand(command);

                int amountOfFactories = Faction.Units.Where(unit => unit.Type.Name == "Factory").ToList().Count;
                int amountOfOtherUnits = Faction.Units.Where(unit => unit.Type.Name != "Factory").ToList().Count;

                if (amountOfFactories * 12 <= amountOfOtherUnits)
                    costThreshold = 300;
                else
                    costThreshold = 60;
            }
        }

        private void TrainUnits()
        {
            // Select all building idle that can train warrior, TODO: Need to select a same type of building and check what type of unit they can train.
            List<Unit> allBuildingIdleThatCanBuild = Faction.Units
                .Where(unit => unit.Type.HasSkill<Skills.Train>() && unit.IsIdle)
                .ToList();

            //If there is building fitting to the condition
            if (allBuildingIdleThatCanBuild.Count != 0)
            {
                Command command = new Train(allBuildingIdleThatCanBuild,
                    World.UnitTypes.First(type => !type.IsBuilding && type.HasSkill<Skills.Attack>()), Faction);
                GenerateCommand(command);

                if (Faction.Units.Where(unit => unit.Type.HasSkill<Skills.Harvest>()).ToList().Count <= 15)
                {
                    List<Unit> harvesterTrainer = new List<Unit>();
                    harvesterTrainer.Add(Faction.Units.Where(unit => unit.Type.HasSkill<Skills.Train>()).First());

                    Command harvesterTrainingCommand = new Train(harvesterTrainer, World.UnitTypes.FromName("Harvester"), Faction);
                    GenerateCommand(harvesterTrainingCommand);
                }
            }

            int amountOfFactories = Faction.Units.Where(unit => unit.Type.Name == "Factory").ToList().Count;
            int amountOfOtherUnits = Faction.Units.Where(unit => unit.Type.Name != "Factory").ToList().Count;

            if (amountOfFactories * 12 <= amountOfOtherUnits)
                costThreshold = 300;
            else
                costThreshold = 60;
        }

        private void DispatchHarvesters(ResourceNode node, ResourceNode alageneNode)
        {
            List<Unit> harvesters = Faction.Units
                .Where(unit => unit.Type.HasSkill<Skills.Harvest>())
                .ToList();

            if (harvesters.Count() != 0)
            {
                Command command = new Harvest(Faction, harvesters, node);
                GenerateCommand(command);
                baseStarted = true;

                List<Unit> alageneHarvesters = new List<Unit>();
                alageneHarvesters.Add(harvesters.First());

                Harvest command2 = new Harvest(Faction, alageneHarvesters, alageneNode);
                GenerateCommand(command2);
            }
        }

        private void Defense()
        {
        }

        private void Attack()
        {
            List<Unit> potentialAttackers = Faction.Units
                .Where(unit => !(unit.Task is Orion.GameLogic.Tasks.Harvest)
                    && !(unit.Task is Orion.GameLogic.Tasks.Build)
                    && unit.Type.HasSkill<Skills.Attack>()
                    && !(unit.Task is Orion.GameLogic.Tasks.Attack))
                .ToList();

            int amountOfAttackers = (int)Math.Ceiling(0.75 * potentialAttackers.Count);
            List<Unit> attackers = new List<Unit>();

            for (int i = 0; i < amountOfAttackers; i++)
                attackers.Add(potentialAttackers.ElementAt(i));
            
            Faction factionToAttack = World.Factions.First();

            if(factionToAttack == Faction)
                factionToAttack = World.Factions.ElementAt(1);

            if (attackers.Count != 0 && factionToAttack.Units.Count() > 0)
            {
                Command command = new Attack(Faction, attackers, factionToAttack.Units.First());
                GenerateCommand(command);
            }
        }

        private void DevelopTechnology()
        {
        }

        private void BuildBase()
        {
            //All units will meet near an Alladium node to start a base there
            Meet();
            //The AI then dispatches Harvesting units to start gathering resources from that node
            DispatchHarvesters(startingNode, alageneStartingNode);
        }

        private void BuildMainCommandCenter()
        {
            Unit builder = Faction.Units
                .FirstOrDefault(unit => unit.Type.HasSkill<Skills.Build>() && unit.IsIdle);
            Vector2 position = new Vector2((startingNode.Position.X + startingNode.BoundingRectangle.Width), (startingNode.Position.Y + startingNode.BoundingRectangle.Height));
            
            if (!World.Bounds.ContainsPoint(position))
                position = new Vector2((startingNode.Position.X - startingNode.BoundingRectangle.Width), (startingNode.Position.Y - startingNode.BoundingRectangle.Height));

            if (builder != null)
            {
                Command command = new Build(builder, position, builder.World.UnitTypes.FromName("Factory"));
                GenerateCommand(command);
                commandCenterBuilt = true;
            }
        }

        private void Meet()
        {
            startingNode = FindIdealNode(ResourceType.Aladdium);
            alageneStartingNode = FindIdealNode(ResourceType.Alagene);

            usedNodes.Add(startingNode);
            usedNodes.Add(alageneStartingNode);
            
            regions.Add(new Circle(startingNode.Position, 10));

            Vector2 position = new Vector2((startingNode.Position.X + startingNode.BoundingRectangle.Width), (startingNode.Position.Y + startingNode.BoundingRectangle.Height));
            
            if(!World.Bounds.ContainsPoint(position))
                position = new Vector2((startingNode.Position.X - startingNode.BoundingRectangle.Width), (startingNode.Position.Y - startingNode.BoundingRectangle.Height));

            List<Unit> unitsToMeet = Faction.Units.Where(unit => !unit.Type.IsBuilding).ToList();

            if (unitsToMeet.Count != 0)
            {
                Command command = new Move(Faction, unitsToMeet, position);
                GenerateCommand(command);
            }
        }

        #endregion
    }
}
