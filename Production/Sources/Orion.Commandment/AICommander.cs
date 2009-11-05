using System;
using System.Collections.Generic;
using System.Linq;
using OpenTK.Math;
using Orion.Commandment.Commands;
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
            foreach (Unit unit in Faction.Units)
            {
                if (unit.Type.IsBuilding)
                {
                    hasBuilding = true;
                    break; 
                }
                else
                    hasBuilding = false;
            }

            //When no base has been started, start one
            if (!baseStarted)
                BuildBase();

            if (!commandCenterBuilt && Faction.AladdiumAmount > 50)
                BuildMainCommandCenter();

            if (Faction.AladdiumAmount >= 50 && hasBuilding)
            {
                TrainUnits();
                BuildFactories();
            }

            if (commandCenterBuilt)
            {
                Attack();
                dispatchIdleHarvesters();
            }
        }

        private void dispatchIdleHarvesters()
        {
            List<Unit> harvesters = Faction.Units.Where(unit => unit.IsIdle && unit.Type.HasSkill<Skills.Harvest>()).ToList();
            ResourceNode node = startingNode;
            int tries = 0;
            if (harvesters.Count != 0)
            {
                    node = findIdealNode();

                if (node == null)
                    return;

                usedNodes.Add(node);
            }
            

            if (harvesters.Count != 0)
            {
                Harvest command = new Harvest(Faction, harvesters, node);
                GenerateCommand(command);
            }

        }

        private ResourceNode findIdealNode()
        {
            ResourceNode bestNode = World.Entities.OfType<ResourceNode>().First();

            foreach (ResourceNode node in World.Entities.OfType<ResourceNode>().Where(node => node.Type == ResourceType.Aladdium && !usedNodes.Contains(node)))
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

            for (int i = 0; i < builders.Count; i++)
            {
                Vector2 position = new Vector2();

                do
                {
                    position.X = random.Next((int)World.Bounds.Width);
                    position.Y = random.Next((int)World.Bounds.Height);
                } while (!World.Terrain.IsWalkable(position) || !World.IsWithinBounds(position));

                Command command = new Build(builders.ElementAt(i), position, World.UnitTypes.FromName("Factory"));
                GenerateCommand(command);
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

                List<Unit> harvesterTrainer = new List<Unit>();
                harvesterTrainer.Add(Faction.Units.Where(unit => unit.Type.HasSkill<Skills.Train>()).First());

                Command harvesterTrainingCommand = new Train(harvesterTrainer, World.UnitTypes.FromName("Harvester"), Faction);
                GenerateCommand(harvesterTrainingCommand);
            }
        }

        private void DispatchHarvesters(ResourceNode node)
        {
            List<Unit> harvesters = Faction.Units
                .Where(unit => unit.Type.HasSkill<Skills.Harvest>())
                .ToList();

            if (harvesters.Count() != 0)
            {
                Command command = new Harvest(Faction, harvesters, node);
                GenerateCommand(command);
                baseStarted = true;
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
            DispatchHarvesters(startingNode);
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
            startingNode = findIdealNode();

            usedNodes.Add(startingNode);
            
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
