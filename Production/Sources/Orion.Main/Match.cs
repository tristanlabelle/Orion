using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Orion.Commandment;
using Orion.GameLogic;

using System.Drawing;

using OpenTK.Math;

namespace Orion.Main
{
    class Match
    {
        #region Fields
        private static DateTime unixEpochStart = new DateTime(1970, 1, 1);

        private Random random;
        private Terrain terrain;

        public readonly UserInputCommander UserCommander;
        public readonly World World;
        public readonly CommandPipeline Pipeline;
        #endregion

        #region Constructors
        internal Match(Random randomGenerator, Terrain terrain, World world, UserInputCommander userCommander, CommandPipeline pipeline)
        {
            random = randomGenerator;
            this.terrain = terrain;
            UserCommander = userCommander;
            World = world;
            Pipeline = pipeline;

            #region Units & Buildings Creation
            // this really, really sucks
            // we have to do something better
            foreach (Faction faction in world.Factions)
            {
                    foreach (UnitType type in World.UnitTypes.AllUnitTypes)
                    {
                        for (int i = 0; i < 10; i++)
                        {
                            Vector2 position;
                            do
                            {
                                position = new Vector2(random.Next(world.Width), random.Next(world.Height));
                            } while (!terrain.IsWalkable(position));
                            Unit unit = faction.CreateUnit(type);
                            unit.Position = position;
                        }
                        
                    }
            }
            #endregion

            #region Resource Nodes
            for (int i = 0; i < 10; i++)
            {
                Vector2 position;
                do
                {
                    position = new Vector2(random.Next(world.Width), random.Next(world.Height));
                } while (!world.Terrain.IsWalkable((int)position.X, (int)position.Y));
                ResourceType resourceType = (i % 2 == 0) ? ResourceType.Alladium : ResourceType.Alagene;
                ResourceNode node = new ResourceNode(i, resourceType, 500, position, world);

                world.ResourceNodes.Add(node);
            }
            #endregion
        }
        #endregion

        //public abstract void Start();
    }
}
