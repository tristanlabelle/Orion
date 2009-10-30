using System;
using Orion.Commandment;
using Orion.GameLogic;
using Color = System.Drawing.Color;

namespace Orion.Main
{
    sealed class SinglePlayerMatchConfigurer : MatchConfigurer
    {
        public SinglePlayerMatchConfigurer()
        {
            seed = (int)Environment.TickCount;
        }

        public override Match Start()
        {
            CreateMap();

            CommandPipeline pipeline = new SinglePlayerCommandPipeline();

            Faction redFaction = world.CreateFaction("Red", Color.Red);
            UserInputCommander userCommander = new UserInputCommander(redFaction);
            userCommander.AddToPipeline(pipeline);

            for (int i = 1; i < numberOfPlayers; i++)
            {
                Faction aiFaction = world.CreateFaction(playerColors[i].Name, playerColors[i]);
                AICommander aiCommander = new AICommander(aiFaction);
                aiCommander.AddToPipeline(pipeline);
            }

            return new Match(random, terrain, world, userCommander, pipeline);
        }
    }
}
