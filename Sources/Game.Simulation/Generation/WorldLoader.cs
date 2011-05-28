using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orion.Engine;
using Orion.Engine.Collections;
using OpenTK;
using System.IO;
using System.Diagnostics;
using Orion.Game.Simulation.IO;
using Orion.Game.Simulation.Components;

namespace Orion.Game.Simulation
{
    public sealed class WorldLoader : WorldBuilder
    {
        #region Fields
        private readonly WorldData worldData;
        #endregion

        #region Constructors
        public WorldLoader(string filePath)
        {
            FileStream stream = new FileStream(filePath, FileMode.Open);
            BinaryReader binaryReader = new BinaryReader(stream);
            worldData = new WorldData(binaryReader);
        }
        #endregion

        #region Properties
        public override Size? FixedSize
        {
            get { return worldData.Terrain.Size; }
        }
        #endregion

        #region Methods
        public override void Build(World world, PrototypeRegistry prototypes)
        {
            for (int y = 0; y < worldData.Terrain.Height; ++y)
                for (int x = 0; x < worldData.Terrain.Width; ++x)
                    world.Terrain[x, y] = worldData.Terrain[x, y];

            CreateResourceNodes(world, prototypes);
            CreateUnits(world, prototypes);
        }

        private void CreateResourceNodes(World world, PrototypeRegistry prototypes)
        {
            foreach (ResourceNodeTemplate node in worldData.AladdiumNodes)
            {
                Point nodeLocation = node.Location;

                Entity concreteNode = CreateResourceNode(world, prototypes, node.ResourceType, node.Location);
                concreteNode.Components.Get<Harvestable>().Amount = node.RemainingAmount;
            }
        }

        private void CreateUnits(World world, PrototypeRegistry prototypes)
        {
            Debug.Assert(world.Factions.Count() <= worldData.NumberOfFactions,
                "There are more factions than this map supports.");

            IEnumerator<Faction> factionEnumerator = world.Factions.GetEnumerator();
            for (int i = 0; i < worldData.NumberOfFactions; i++)
            {
                factionEnumerator.MoveNext();
                Faction currentFaction = factionEnumerator.Current;
                foreach (UnitTemplate unit in worldData.GetUnitsForFaction(i))
                {
                    Entity prototype = prototypes.FromName(unit.UnitTypeName);
                    currentFaction.CreateUnit(prototype, unit.Location);
                }
            }
        }
        #endregion
    }
}
