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

            Faction redFaction = world.CreateFaction("Red", Color.Red);
            UserInputCommander redCommander = new UserInputCommander(redFaction);

            Faction blueFaction = world.CreateFaction("Blue", Color.Cyan);
            DummyAICommander blueCommander = new DummyAICommander(blueFaction, random);

            CommandPipeline pipeline = new SinglePlayerCommandPipeline();

            redCommander.AddToPipeline(pipeline);
            blueCommander.AddToPipeline(pipeline);

            return new Match(random, terrain, world, redCommander, pipeline);
		}
	}
}
