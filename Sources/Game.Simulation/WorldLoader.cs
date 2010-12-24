﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.Engine;
using Orion.Engine.Collections;
using OpenTK;
using System.IO;
using System.Diagnostics;
using Orion.Game.Simulation.IO;

namespace Orion.Game.Simulation
{
    public class WorldLoader : WorldGenerator
    {
        #region Fields
        private readonly WorldReader worldData;
        #endregion

        #region Constructors
        public WorldLoader(string filePath)
        {
            FileStream stream = new FileStream(filePath, FileMode.Open);
            BinaryReader binaryReader = new BinaryReader(stream);
            worldData = new WorldReader(binaryReader);
        }
        #endregion

        #region Methods
        public override Terrain GenerateTerrain()
        {
            return worldData.Terrain;
        }

        public override void PrepareWorld(World world, UnitTypeRegistry unitTypes)
        {
            // create resource nodes
            foreach (ResourceNodeTemplate node in worldData.AladdiumNodes)
            {
                Point nodeLocation = node.Location;
                ResourceNode concreteNode = world.Entities.CreateResourceNode(node.ResourceType, nodeLocation);
                concreteNode.AmountRemaining = node.AmountRemaining;
            }

            // place units
            Debug.Assert(world.Factions.Count() <= worldData.NumberOfFactions,
                "There are more factions than this map supports.");

            IEnumerator<Faction> factionEnumerator = world.Factions.GetEnumerator();
            for (int i = 0; i < worldData.NumberOfFactions; i++)
            {
                factionEnumerator.MoveNext();
                Faction currentFaction = factionEnumerator.Current;
                foreach (UnitTemplate unit in worldData.GetUnitsForFaction(i))
                {
                    UnitType type = unitTypes.FromName(unit.UnitTypeName);
                    currentFaction.CreateUnit(type, unit.Location);
                }
            }
        }
        #endregion
    }
}
