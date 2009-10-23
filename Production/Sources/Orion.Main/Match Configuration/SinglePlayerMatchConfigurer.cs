using System;
using Color = System.Drawing.Color;

using OpenTK.Math;

using Orion.Graphics;
using Orion.GameLogic;
using Orion.Commandment;

namespace Orion.Main
{
    sealed class SinglePlayerMatchConfigurer : MatchConfigurer
    {
        public SinglePlayerMatchConfigurer()
        {
            seed = (int)(DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0)).TotalSeconds;
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
                DummyAICommander aiCommander = new DummyAICommander(aiFaction, random);
                aiCommander.AddToPipeline(pipeline);
            }

            return new Match(random, terrain, world, userCommander, pipeline);
        }
    }
}
