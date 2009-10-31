﻿using System;
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
        }

        private void TrainUnits()
        {
            // Select all building idle that can train warrior, TODO: Need to select a same type of building and check what type of unit they can train.
            List<Unit> allBuildingIdleThatCanBuild = World.Units.Where(unit => unit.Faction == Faction && unit.Type.IsBuilding && unit.IsIdle).ToList();

            //If there is building fitting to the condition
            if (allBuildingIdleThatCanBuild != null)
            {
                Command command = new Train(allBuildingIdleThatCanBuild,
                    World.UnitTypes.First(type => !type.IsBuilding && type.HasSkill<Skills.Move>()), Faction);
                GenerateCommand(command);
            }
        }

        private void DispatchHarvesters(ResourceNode node)
        {
            // Eventually all Harvesting units will be dispatched.  As of now, only 20% of all units are.

            float amountOfHarvesters = World.Units.Where(unit => unit.Faction == Faction).Count()*0.2f;
            int roundedHarvesters = (int)Math.Ceiling(amountOfHarvesters);

            List<Unit> units = World.Units.Where(unit => unit.Faction == Faction).ToList();
            List<Unit> harvesters = new List<Unit>();
            for (int i = 0; i < roundedHarvesters; i++)
            {
                harvesters.Add(units.ElementAt(i));
            }

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
            Unit builder = World.Units.FirstOrDefault(unit => unit.Faction == Faction && !unit.Type.IsBuilding && unit.IsIdle);
            Vector2 position = new Vector2((startingNode.Position.X + startingNode.Circle.Diameter), (startingNode.Position.Y + startingNode.Circle.Diameter));
            
            if (!World.Bounds.ContainsPoint(position))
                position = new Vector2((startingNode.Position.X - startingNode.Circle.Diameter), (startingNode.Position.Y - startingNode.Circle.Diameter));

            if (builder != null)
            {
                Command command = new Build(builder, position, World.UnitTypes.First(type => type.IsBuilding));
                GenerateCommand(command);
                commandCenterBuilt = true;
            }
        }

        private void Meet()
        {
            startingNode = World.ResourceNodes.First();
            regions.Add(new Circle(startingNode.Position, 10));

            Vector2 position = new Vector2((startingNode.Position.X + startingNode.Circle.Diameter), (startingNode.Position.Y + startingNode.Circle.Diameter));
            
            if(!World.Bounds.ContainsPoint(position))
                position = new Vector2((startingNode.Position.X - startingNode.Circle.Diameter), (startingNode.Position.Y - startingNode.Circle.Diameter));

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
