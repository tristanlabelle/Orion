using System;
using OpenTK.Math;
using Orion.Commandment;
using Orion.GameLogic;

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
                    foreach (UnitType type in World.UnitTypes)
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
                ResourceType resourceType = (i % 2 == 0) ? ResourceType.Aladdium : ResourceType.Alagene;
                ResourceNode node = world.CreateResourceNode(resourceType, 500, position);
            }
            #endregion
        }
        #endregion

        //public abstract void Start();
    }
}
