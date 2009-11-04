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
        #endregion

        #region Contructors

        public AICommander(Faction faction)
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
                TrainUnits();

            if (commandCenterBuilt)
                Attack();
        }

        private void TrainUnits()
        {
            // Select all building idle that can train warrior, TODO: Need to select a same type of building and check what type of unit they can train.
            List<Unit> allBuildingIdleThatCanBuild = World.Units.Where(unit => unit.Faction == Faction && unit.Type.HasSkill<Skills.Train>() && unit.IsIdle).ToList();

            //If there is building fitting to the condition
            if (allBuildingIdleThatCanBuild.Count != 0)
            {
                Command command = new Train(allBuildingIdleThatCanBuild,
                    World.UnitTypes.First(type => !type.IsBuilding && type.HasSkill<Skills.Attack>()), Faction);
                GenerateCommand(command);
            }
        }

        private void DispatchHarvesters(ResourceNode node)
        {
            List<Unit> harvesters = World.Units.Where(unit => unit.Faction == Faction && unit.Type.HasSkill<Skills.Harvest>()).ToList();

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
            List<Unit> potentialAttackers = World.Units.Where(unit => unit.Faction == Faction 
                && !(unit.Task is Orion.GameLogic.Tasks.Harvest) && !(unit.Task is Orion.GameLogic.Tasks.Build) && unit.Type.HasSkill<Skills.Attack>()).ToList();

            int amountOfAttackers = (int)Math.Ceiling(0.75 * potentialAttackers.Count);
            List<Unit> attackers = new List<Unit>();

            for (int i = 0; i < amountOfAttackers; i++)
                attackers.Add(potentialAttackers.ElementAt(i));
            
            Faction factionToAttack = World.Factions.First();

            if(factionToAttack == Faction)
                factionToAttack = World.Factions.ElementAt(1);

            if (attackers.Count != 0)
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
            Unit builder = World.Units.FirstOrDefault(unit => unit.Faction == Faction && unit.Type.HasSkill<Skills.Build>() && unit.IsIdle);
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
            startingNode = World.ResourceNodes.First();
            
            foreach (ResourceNode node in World.ResourceNodes.Where(node => node.Type == ResourceType.Aladdium))
            {
                Circle effectiveRange = new Circle(node.Position, 20);
                
                int enemyUnits = World.Units.Where(unit => unit.Faction != Faction && effectiveRange.ContainsPoint(unit.Position)).ToList().Count;
                int alliedUnits = World.Units.Where(unit => unit.Faction == Faction && effectiveRange.ContainsPoint(unit.Position)).ToList().Count;

                int unitScore = alliedUnits - enemyUnits;

                Circle currentRange = new Circle(startingNode.Position, 20);

                int currentEnemyUnits = World.Units.Where(unit => unit.Faction != Faction && currentRange.ContainsPoint(unit.Position)).ToList().Count;
                int currentAlliedUnits = World.Units.Where(unit => unit.Faction == Faction && currentRange.ContainsPoint(unit.Position)).ToList().Count;

                int currentScore = currentAlliedUnits - currentEnemyUnits;

                if (unitScore > currentScore)
                    startingNode = node;

            }
            regions.Add(new Circle(startingNode.Position, 10));

            Vector2 position = new Vector2((startingNode.Position.X + startingNode.BoundingRectangle.Width), (startingNode.Position.Y + startingNode.BoundingRectangle.Height));
            
            if(!World.Bounds.ContainsPoint(position))
                position = new Vector2((startingNode.Position.X - startingNode.BoundingRectangle.Width), (startingNode.Position.Y - startingNode.BoundingRectangle.Height));

            List<Unit> unitsToMeet = World.Units.Where(unit => unit.Faction == Faction && !unit.Type.IsBuilding).ToList();

            if (unitsToMeet.Count != 0)
            {
                Command command = new Move(Faction, unitsToMeet, position);
                GenerateCommand(command);
            }
        }

        #endregion
    }
}
