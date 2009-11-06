using System;
using OpenTK.Math;
using Orion.Commandment;
using Orion.GameLogic;
using System.Linq;
using System.Collections.Generic;

namespace Orion.Main
{
    public sealed class Match
    {
        #region Fields
        private const short campSize = 15;

        private readonly Random random;
        public readonly UserInputCommander userCommander;
        public readonly World world;
        public readonly CommandPipeline pipeline;
        #endregion

        #region Constructors
        internal Match(Random random, World world, UserInputCommander userCommander, CommandPipeline pipeline)
        {
            this.random = random;
            this.userCommander = userCommander;
            this.world = world;
            this.pipeline = pipeline;

            #region Units & Buildings Creation
            List<Vector2> positionOfOthersBases = new List<Vector2>();
            int DistanceBetweenTwoCamps = 175;

            // Find a Spot To place a base
            foreach (Faction faction in world.Factions)
            {
                Point16 position = new Point16(0, 0) ;
                int tries = 0;
                bool allWalkable = false;
                while (!allWalkable)
                {
                    allWalkable = true;
                    ++tries;
                    if (tries == 750)
                    {
                        DistanceBetweenTwoCamps = (int)((float)DistanceBetweenTwoCamps *0.80f);
                        tries = 0;
                    }

                    position = new Point16((short)random.Next(world.Width), (short)random.Next(world.Height));

                    for (short ligne = 0; ligne < campSize; ligne++)
                    {
                        if (!allWalkable) break;

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

                    //finally check if there is another command center near
                    if (allWalkable)
                    {
                        // creation of the command center in the center of the area
                        position = new Point16((short)(position.X + campSize / 2), (short)(position.Y + campSize / 2));
                        foreach (Vector2 positionOfAFaction in positionOfOthersBases)
                        {
                            if ((positionOfAFaction - position).Length < DistanceBetweenTwoCamps)
                            {
                                allWalkable = false;
                                break;
                            }
                        }

                    }

                }

                positionOfOthersBases.Add(position);
                Unit factory = faction.CreateUnit(world.UnitTypes.FromName("Factory"),new Vector2(position.X, position.Y));
                
                //creation of the builder and the harvester
                for(short i = 1 ;i<=2;i++)
                {
                    Unit builder = faction.CreateUnit(world.UnitTypes.FromName("Builder"), new Vector2(position.X + i * 2, position.Y));
                    Unit harvester = faction.CreateUnit(world.UnitTypes.FromName("Harvester"), new Vector2(position.X, position.Y + i * 2));
                }
               
                ResourceNode nodeAladdium = world.Entities.CreateResourceNode
                    (ResourceType.Aladdium, 500, new Vector2(position.X - (campSize / 4), position.Y - 1));

                ResourceNode nodeAlagene = world.Entities.CreateResourceNode
                    (ResourceType.Alagene, 500, new Vector2(position.X - (campSize / 4), position.Y + 1));
            }
            #endregion

            #region Resource Nodes
            for (int i = 0; i < 25; i++)
            {
                Vector2 position;
                do
                {
                    position = new Vector2(random.Next(world.Width), random.Next(world.Height));
                } while (!world.Terrain.IsWalkable((int)position.X, (int)position.Y));
                ResourceType resourceType = (i % 2 == 0) ? ResourceType.Aladdium : ResourceType.Alagene;
                ResourceNode node = world.Entities.CreateResourceNode(resourceType, 5000, position);
            }
            #endregion
        }
        #endregion

        #region Methods
        /// <summary>
        /// Updates this <see cref="Match"/> for the duration of a frame.
        /// </summary>
        /// <param name="timeDeltaInSeconds">The time elapsed since the last frame, in seconds.</param>
        public void Update(float timeDeltaInSeconds)
        {
            pipeline.Update(timeDeltaInSeconds);
            world.Update(timeDeltaInSeconds);
        }
        #endregion
    }
}
