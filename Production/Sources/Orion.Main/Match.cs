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
        private const short campSize = 6;
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

            // Find a Spot To place a base 10 by 10 
            //if there less that 10% non walkable we will use this spot
            //for a base
            foreach (Faction faction in world.Factions)
            {
                Point16 position = new Point16(0, 0) ;
                int tries = 0;
                bool allWalkable = false;
                while(!allWalkable)
                {
                    allWalkable = true;
                    ++tries;
                    if(tries ==100) throw new Exception("FAIL to find camp site");

                    position = new Point16((short)random.Next(world.Width), (short)random.Next(world.Height));

                    for (short ligne = 0; ligne < campSize; ligne++)
                    {
                        if(!allWalkable) break;

                        for (short colonne = 0; colonne < campSize; colonne++)
                        {
                            Point16 testedPoint = new Point16((short)(position.X + colonne), (short)(position.Y + ligne));
                            if (!world.IsWithinBounds(testedPoint) || !world.Terrain.IsWalkable(testedPoint)) 
                            {
                                allWalkable = false;
                                break;
                            }
                        }
                    }
                }
                
                // creation of the command center in the center of the area
                position = new Point16((short)(position.X + campSize / 2), (short)(position.Y + campSize / 2));

                faction.CreateUnit(world.UnitTypes.FromName("Factory"), position);
                
                //creation of the builder and the harvester
                for(short i = 1 ;i<=2;i++)
                {
                    Unit builder = faction.CreateUnit(
                        world.UnitTypes.FromName("Builder"),
                        new Vector2(position.X + i, position.Y));

                    Unit harvester = faction.CreateUnit(
                        world.UnitTypes.FromName("Harvester"),
                        new Vector2(position.X, position.Y + i));
                }
               
                ResourceNode nodeAladdium = world.Entities.CreateResourceNode
                    (ResourceType.Aladdium, 500, new Vector2(position.X - (campSize / 2), position.Y - 1));

                ResourceNode nodeAlagene = world.Entities.CreateResourceNode
                    (ResourceType.Alagene, 500, new Vector2(position.X - (campSize / 2), position.Y + 1));
            }
            #endregion

            #region Resource Nodes
            for (int i = 0; i < 6; i++)
            {
                Vector2 position;
                do
                {
                    position = new Vector2(random.Next(world.Width), random.Next(world.Height));
                } while (!world.Terrain.IsWalkable((int)position.X, (int)position.Y));
                ResourceType resourceType = (i % 2 == 0) ? ResourceType.Aladdium : ResourceType.Alagene;
                ResourceNode node = world.Entities.CreateResourceNode(resourceType, 500, position);
            }
            #endregion
        }
        #endregion

        //public abstract void Start();
    }
}
